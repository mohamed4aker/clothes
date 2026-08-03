using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

// ============================================================
// SalesReturnService - المرتجعات
// ============================================================
public class SalesReturnService : ISalesReturnService
{
    private readonly AppDbContext _context;
    public SalesReturnService(AppDbContext context) => _context = context;

    public async Task<List<SalesReturnDto>> GetReturnsAsync(int brandId)
    {
        return await _context.SalesReturns
            .Where(r => r.BrandId == brandId && r.IsActive)
            .Include(r => r.OriginalInvoice)
            .Include(r => r.Customer)
            .Include(r => r.CashierUser)
            .Include(r => r.Branch)
            .Include(r => r.Items)
            .OrderByDescending(r => r.ReturnDate)
            .Select(r => new SalesReturnDto
            {
                ReturnId = r.ReturnId,
                ReturnNumber = r.ReturnNumber,
                OriginalInvoiceNumber = r.OriginalInvoice.InvoiceNumber,
                OriginalInvoiceId = r.OriginalInvoiceId,
                CustomerName = r.Customer != null ? r.Customer.FullName : null,
                CashierName = r.CashierUser.FullName,
                BranchName = r.Branch.BranchName,
                GrandTotal = r.GrandTotal,
                RefundAmount = r.RefundAmount,
                RefundMethod = r.RefundMethod,
                Reason = r.Reason,
                ReturnDate = r.ReturnDate,
                ItemsCount = r.Items.Count
            }).ToListAsync();
    }

    public async Task<SaleDetailDto?> GetInvoiceForReturnAsync(int invoiceId, int brandId)
    {
        var invoice = await _context.SalesInvoices
            .Where(i => i.InvoiceId == invoiceId && i.BrandId == brandId)
            .Include(i => i.Branch)
            .Include(i => i.Customer)
            .Include(i => i.CashierUser)
            .Include(i => i.Items).ThenInclude(it => it.Variant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync();

        if (invoice == null) return null;

        return new SaleDetailDto
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerName = invoice.Customer?.FullName,
            CashierName = invoice.CashierUser.FullName,
            BranchName = invoice.Branch.BranchName,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            GrandTotal = invoice.GrandTotal,
            PaidAmount = invoice.PaidAmount,
            ChangeAmount = invoice.ChangeAmount,
            CreatedAt = invoice.CreatedAt,
            Status = invoice.Status.ToString(),
            Items = invoice.Items.Select(it => new SaleItemDetailDto
            {
                VariantId = it.VariantId,
                ProductName = it.Variant.VariantName ?? it.Variant.Product!.ProductName,
                SKU = it.Variant.SKU,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                LineTotal = it.LineTotal
            }).ToList()
        };
    }

    public async Task<int> CreateReturnAsync(int brandId, int userId, CreateSalesReturnRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var originalInvoice = await _context.SalesInvoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == request.OriginalInvoiceId && i.BrandId == brandId);
            if (originalInvoice == null) throw new Exception("الفاتورة الأصلية غير موجودة");

            var lastReturn = await _context.SalesReturns
                .Where(r => r.BrandId == brandId)
                .OrderByDescending(r => r.ReturnId)
                .Select(r => r.ReturnNumber)
                .FirstOrDefaultAsync();

            var prefix = $"RET-{DateTime.UtcNow:yyyyMM}-";
            var returnNumber = string.IsNullOrEmpty(lastReturn) || !lastReturn.StartsWith(prefix)
                ? $"{prefix}001"
                : $"{prefix}{(int.Parse(lastReturn.Substring(prefix.Length)) + 1):D3}";

            decimal subTotal = 0;
            var returnObj = new SalesReturn
            {
                BrandId = brandId,
                BranchId = originalInvoice.BranchId,
                ReturnNumber = returnNumber,
                OriginalInvoiceId = originalInvoice.InvoiceId,
                CustomerId = originalInvoice.CustomerId,
                CashierUserId = userId,
                RefundMethod = request.RefundMethod,
                Reason = request.Reason,
                Notes = request.Notes
            };

            foreach (var item in request.Items)
            {
                var lineTotal = item.UnitPrice * item.Quantity;
                subTotal += lineTotal;

                returnObj.Items.Add(new SalesReturnItem
                {
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = lineTotal,
                    BackToStock = item.BackToStock,
                    ItemReason = item.ItemReason
                });

                if (item.BackToStock)
                {
                    var inv = await _context.Inventory
                        .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == originalInvoice.BranchId);
                    if (inv != null)
                    {
                        inv.Quantity += item.Quantity;
                        inv.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        inv = new Inventory
                        {
                            BranchId = originalInvoice.BranchId,
                            VariantId = item.VariantId,
                            Quantity = item.Quantity
                        };
                        _context.Inventory.Add(inv);
                    }

                    _context.StockMovements.Add(new StockMovement
                    {
                        BranchId = originalInvoice.BranchId,
                        VariantId = item.VariantId,
                        MovementType = StockMovementType.Return,
                        Quantity = item.Quantity,
                        BalanceAfter = inv.Quantity,
                        ReferenceType = "SalesReturn",
                        Notes = $"مرتجع - {returnNumber}",
                        UserId = userId
                    });
                }
            }

            var taxAmount = subTotal * 0.14m;
            returnObj.SubTotal = subTotal;
            returnObj.TaxAmount = taxAmount;
            returnObj.GrandTotal = subTotal + taxAmount;
            returnObj.RefundAmount = returnObj.GrandTotal;

            originalInvoice.Status = InvoiceStatus.PartiallyRefunded;
            _context.SalesReturns.Add(returnObj);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return returnObj.ReturnId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

// ============================================================
// StockTakeService - الجرد
// ============================================================
public class StockTakeService : IStockTakeService
{
    private readonly AppDbContext _context;
    public StockTakeService(AppDbContext context) => _context = context;

    public async Task<List<StockTakeDto>> GetStockTakesAsync(int brandId)
    {
        return await _context.StockTakes
            .Where(s => s.BrandId == brandId && s.IsActive)
            .Include(s => s.Branch)
            .Include(s => s.Items)
            .OrderByDescending(s => s.StockTakeDate)
            .Select(s => new StockTakeDto
            {
                StockTakeId = s.StockTakeId,
                StockTakeNumber = s.StockTakeNumber,
                BranchName = s.Branch.BranchName,
                StockTakeDate = s.StockTakeDate,
                Status = s.Status,
                TotalDifferenceValue = s.TotalDifferenceValue,
                ItemsCount = s.Items.Count,
                Notes = s.Notes
            }).ToListAsync();
    }

    public async Task<StockTakeDetailDto?> GetStockTakeByIdAsync(int stockTakeId, int brandId)
    {
        var st = await _context.StockTakes
            .Where(s => s.StockTakeId == stockTakeId && s.BrandId == brandId)
            .Include(s => s.Items).ThenInclude(i => i.Variant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync();
        if (st == null) return null;

        return new StockTakeDetailDto
        {
            StockTakeId = st.StockTakeId,
            StockTakeNumber = st.StockTakeNumber,
            BranchId = st.BranchId,
            StockTakeDate = st.StockTakeDate,
            Status = st.Status,
            TotalDifferenceValue = st.TotalDifferenceValue,
            Items = st.Items.Select(i => new StockTakeItemDto
            {
                ItemId = i.ItemId,
                VariantId = i.VariantId,
                ProductName = i.Variant.VariantName ?? i.Variant.Product!.ProductName,
                SKU = i.Variant.SKU,
                SystemQuantity = i.SystemQuantity,
                CountedQuantity = i.CountedQuantity,
                Difference = i.Difference,
                UnitCost = i.UnitCost,
                DifferenceValue = i.DifferenceValue,
                Notes = i.Notes
            }).ToList()
        };
    }

    public async Task<List<StockTakeItemDto>> PrepareStockTakeAsync(int branchId, int brandId)
    {
        return await _context.Inventory
            .Where(i => i.BranchId == branchId && i.Variant.Product.BrandId == brandId && i.Variant.Product.IsActive)
            .Include(i => i.Variant).ThenInclude(v => v.Product)
            .Select(i => new StockTakeItemDto
            {
                VariantId = i.VariantId,
                ProductName = i.Variant.VariantName ?? i.Variant.Product!.ProductName,
                SKU = i.Variant.SKU,
                SystemQuantity = i.Quantity,
                CountedQuantity = i.Quantity,
                Difference = 0,
                UnitCost = i.AverageCost,
                DifferenceValue = 0
            }).ToListAsync();
    }

    public async Task<int> CreateStockTakeAsync(int brandId, int userId, CreateStockTakeRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var lastSt = await _context.StockTakes
                .Where(s => s.BrandId == brandId)
                .OrderByDescending(s => s.StockTakeId)
                .Select(s => s.StockTakeNumber)
                .FirstOrDefaultAsync();
            var prefix = $"ST-{DateTime.UtcNow:yyyyMM}-";
            var stNumber = string.IsNullOrEmpty(lastSt) || !lastSt.StartsWith(prefix)
                ? $"{prefix}001"
                : $"{prefix}{(int.Parse(lastSt.Substring(prefix.Length)) + 1):D3}";

            var stockTake = new StockTake
            {
                BrandId = brandId,
                BranchId = request.BranchId,
                StockTakeNumber = stNumber,
                Status = "Completed",
                UserId = userId,
                Notes = request.Notes
            };

            decimal totalDiff = 0;
            foreach (var item in request.Items)
            {
                var inv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == request.BranchId);
                
                var systemQty = inv?.Quantity ?? 0;
                var unitCost = inv?.AverageCost ?? 0;
                var difference = item.CountedQuantity - systemQty;
                var diffValue = difference * unitCost;
                totalDiff += diffValue;

                stockTake.Items.Add(new StockTakeItem
                {
                    VariantId = item.VariantId,
                    SystemQuantity = systemQty,
                    CountedQuantity = item.CountedQuantity,
                    Difference = difference,
                    UnitCost = unitCost,
                    DifferenceValue = diffValue,
                    Notes = item.Notes
                });

                if (inv != null)
                {
                    inv.Quantity = item.CountedQuantity;
                    inv.LastUpdated = DateTime.UtcNow;

                    if (difference != 0)
                    {
                        _context.StockMovements.Add(new StockMovement
                        {
                            BranchId = request.BranchId,
                            VariantId = item.VariantId,
                            MovementType = StockMovementType.Adjustment,
                            Quantity = difference,
                            UnitCost = unitCost,
                            BalanceAfter = item.CountedQuantity,
                            ReferenceType = "StockTake",
                            Notes = $"جرد - {stNumber}",
                            UserId = userId
                        });
                    }
                }
            }

            stockTake.TotalDifferenceValue = totalDiff;
            _context.StockTakes.Add(stockTake);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return stockTake.StockTakeId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

// ============================================================
// CashierSessionService - تسليم الدرج
// (يستخدم SessionStart/SessionEnd بدل OpenedAt/ClosedAt)
// ============================================================
public class CashierSessionService : ICashierSessionService
{
    private readonly AppDbContext _context;
    public CashierSessionService(AppDbContext context) => _context = context;

    public async Task<CashierSessionDto?> GetActiveSessionAsync(int userId, int branchId)
    {
        var session = await _context.CashierSessions
            .Include(s => s.User)
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.BranchId == branchId && s.SessionEnd == null);
        if (session == null) return null;
        return MapSession(session);
    }

    public async Task<List<CashierSessionDto>> GetSessionsAsync(int brandId, int? userId = null)
    {
        var query = _context.CashierSessions
            .Include(s => s.User)
            .Include(s => s.Branch)
            .Where(s => s.User.BrandId == brandId);
        if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);

        return await query
            .OrderByDescending(s => s.SessionStart)
            .Take(100)
            .Select(s => new CashierSessionDto
            {
                SessionId = s.SessionId,
                SessionNumber = s.SessionNumber,
                UserName = s.User.FullName,
                BranchName = s.Branch.BranchName,
                OpenedAt = s.SessionStart,
                ClosedAt = s.SessionEnd,
                OpeningBalance = s.OpeningBalance,
                ClosingBalance = s.ClosingBalance,
                ExpectedBalance = s.ExpectedBalance,
                Difference = s.Difference,
                TotalSales = s.TotalSales,
                TotalCash = s.TotalCash,
                TotalCard = s.TotalCard,
                TotalReturns = s.TotalReturns,
                Status = s.SessionEnd == null ? "Active" : "Closed"
            }).ToListAsync();
    }

    public async Task<int> OpenSessionAsync(int userId, OpenSessionRequest request)
    {
        var existing = await _context.CashierSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.BranchId == request.BranchId && s.SessionEnd == null);
        if (existing != null) throw new Exception("في جلسة مفتوحة بالفعل");

        var session = new CashierSession
        {
            UserId = userId,
            BranchId = request.BranchId,
            SessionNumber = $"SES-{DateTime.UtcNow:yyyyMMdd-HHmm}-{userId}",
            SessionStart = DateTime.UtcNow,
            OpeningBalance = request.OpeningBalance,
            Status = "Open",
            Notes = request.Notes
        };
        _context.CashierSessions.Add(session);
        await _context.SaveChangesAsync();
        return session.SessionId;
    }

    public async Task<bool> CloseSessionAsync(int userId, CloseSessionRequest request)
    {
        var session = await _context.CashierSessions
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId && s.UserId == userId);
        if (session == null || session.SessionEnd != null) return false;

        var invoices = await _context.SalesInvoices
            .Where(i => i.SessionId == session.SessionId)
            .ToListAsync();

        session.TotalSales = invoices.Sum(i => i.GrandTotal);
        session.TotalCash = await _context.InvoicePayments
            .Where(p => p.Invoice.SessionId == session.SessionId && p.PaymentMethod == PaymentMethod.Cash)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
        session.TotalCard = await _context.InvoicePayments
            .Where(p => p.Invoice.SessionId == session.SessionId && p.PaymentMethod == PaymentMethod.Visa)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        session.ExpectedBalance = session.OpeningBalance + session.TotalCash;
        session.ClosingBalance = request.CountedCash;
        session.Difference = session.ClosingBalance - session.ExpectedBalance;
        session.SessionEnd = DateTime.UtcNow;
        session.Status = "Closed";
        if (!string.IsNullOrEmpty(request.Notes))
            session.Notes = (session.Notes ?? "") + "\n" + request.Notes;

        await _context.SaveChangesAsync();
        return true;
    }

    private CashierSessionDto MapSession(CashierSession s) => new()
    {
        SessionId = s.SessionId,
        SessionNumber = s.SessionNumber,
        UserName = s.User.FullName,
        BranchName = s.Branch.BranchName,
        OpenedAt = s.SessionStart,
        ClosedAt = s.SessionEnd,
        OpeningBalance = s.OpeningBalance,
        ClosingBalance = s.ClosingBalance,
        ExpectedBalance = s.ExpectedBalance,
        Difference = s.Difference,
        TotalSales = s.TotalSales,
        TotalCash = s.TotalCash,
        TotalCard = s.TotalCard,
        TotalReturns = s.TotalReturns,
        Status = s.SessionEnd == null ? "Active" : "Closed"
    };
}
