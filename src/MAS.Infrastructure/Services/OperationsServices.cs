using MAS.Application.Common;
using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

// ============================================================
// ExpenseService
// ============================================================
public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _context;
    public ExpenseService(AppDbContext context) => _context = context;

    public async Task<List<ExpenseDto>> GetExpensesAsync(int brandId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Expenses.Where(e => e.BrandId == brandId && e.IsActive);
        if (from.HasValue) query = query.Where(e => e.ExpenseDate >= from.Value);
        if (to.HasValue) query = query.Where(e => e.ExpenseDate <= to.Value);

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new ExpenseDto
            {
                ExpenseId = e.ExpenseId,
                Category = e.Category,
                Description = e.Description,
                Amount = e.Amount,
                PaymentMethod = e.PaymentMethod,
                ExpenseDate = e.ExpenseDate,
                Notes = e.Notes
            }).ToListAsync();
    }

    public async Task<int> CreateExpenseAsync(int brandId, int userId, CreateExpenseRequest request)
    {
        var expense = new Expense
        {
            BrandId = brandId,
            BranchId = request.BranchId,
            ExpenseNumber = $"EXP-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(100, 999)}",
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            ExpenseDate = request.ExpenseDate,
            Notes = request.Notes,
            CreatedBy = userId
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense.ExpenseId;
    }

    public async Task<bool> DeleteExpenseAsync(int expenseId, int brandId)
    {
        var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.BrandId == brandId);
        if (expense == null) return false;
        expense.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalExpensesAsync(int brandId, DateTime from, DateTime to)
    {
        return await _context.Expenses
            .Where(e => e.BrandId == brandId && e.IsActive && e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;
    }
}

// ============================================================
// PurchaseService
// ============================================================
public class PurchaseService : IPurchaseService
{
    private readonly AppDbContext _context;
    public PurchaseService(AppDbContext context) => _context = context;

    public async Task<List<PurchaseInvoiceDto>> GetPurchasesAsync(int brandId)
    {
        return await _context.PurchaseInvoices
            .Where(p => p.BrandId == brandId && p.IsActive)
            .Include(p => p.Branch)
            .Include(p => p.Warehouse)
            .Include(p => p.Items)
            .OrderByDescending(p => p.InvoiceDate)
            .Select(p => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = p.PurchaseInvoiceId,
                InvoiceNumber = p.InvoiceNumber,
                SupplierId = p.SupplierId,
                WarehouseId = p.WarehouseId,
                PurchaseType = p.PurchaseType,
                WarehouseName = p.Warehouse != null ? p.Warehouse.WarehouseName : null,
                SupplierName = p.SupplierName,
                SupplierPhone = p.SupplierPhone,
                BranchName = p.Branch.BranchName,
                InvoiceDate = p.InvoiceDate,
                SubTotal = p.SubTotal,
                TaxAmount = p.TaxAmount,
                ShippingCost = p.ShippingCost,
                GrandTotal = p.GrandTotal,
                PaidAmount = p.PaidAmount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                ItemsCount = p.Items.Count
            }).ToListAsync();
    }

    public async Task<int> CreatePurchaseAsync(int brandId, int userId, CreatePurchaseRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var lastInv = await _context.PurchaseInvoices
                .Where(p => p.BrandId == brandId)
                .OrderByDescending(p => p.PurchaseInvoiceId)
                .Select(p => p.InvoiceNumber)
                .FirstOrDefaultAsync();
            var prefix = $"PUR-{DateTime.UtcNow:yyyyMM}-";
            var invoiceNumber = DocumentNumber.Next(prefix, lastInv, 3);

            decimal subTotal = 0;
            var purchase = new PurchaseInvoice
            {
                BrandId = brandId,
                BranchId = request.BranchId,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                PurchaseType = request.PurchaseType,
                InvoiceNumber = invoiceNumber,
                SupplierName = request.SupplierName,
                SupplierPhone = request.SupplierPhone,
                InvoiceDate = request.InvoiceDate,
                ShippingCost = request.ShippingCost,
                PaidAmount = request.PaidAmount,
                PaymentMethod = request.PaymentMethod,
                Status = "Completed",
                CreatedBy = userId
            };

            foreach (var item in request.Items)
            {
                var lineTotal = item.Quantity * item.UnitCost;
                subTotal += lineTotal;

                purchase.Items.Add(new PurchaseInvoiceItem
                {
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = lineTotal
                });

                var inv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == request.BranchId);
                
                if (inv == null)
                {
                    inv = new Inventory
                    {
                        BranchId = request.BranchId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        AverageCost = item.UnitCost
                    };
                    _context.Inventory.Add(inv);
                }
                else
                {
                    var totalValue = (inv.Quantity * inv.AverageCost) + (item.Quantity * item.UnitCost);
                    var totalQty = inv.Quantity + item.Quantity;
                    inv.AverageCost = totalQty > 0 ? totalValue / totalQty : item.UnitCost;
                    inv.Quantity = totalQty;
                    inv.LastUpdated = DateTime.UtcNow;
                }

                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = request.BranchId,
                    VariantId = item.VariantId,
                    MovementType = StockMovementType.Purchase,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    BalanceAfter = inv.Quantity,
                    ReferenceType = "PurchaseInvoice",
                    Notes = $"شراء - فاتورة {invoiceNumber}",
                    UserId = userId
                });
            }

            purchase.SubTotal = subTotal;
            purchase.GrandTotal = subTotal + request.ShippingCost;

            _context.PurchaseInvoices.Add(purchase);
            
            // تحديث رصيد المورد لو محدد
            if (request.SupplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId.Value);
                if (supplier != null)
                {
                    supplier.TotalPurchases += purchase.GrandTotal;
                    supplier.CurrentBalance -= (purchase.GrandTotal - request.PaidAmount);
                }
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return purchase.PurchaseInvoiceId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<decimal> GetTotalPurchasesAsync(int brandId, DateTime from, DateTime to)
    {
        return await _context.PurchaseInvoices
            .Where(p => p.BrandId == brandId && p.IsActive && p.InvoiceDate >= from && p.InvoiceDate <= to)
            .SumAsync(p => (decimal?)p.GrandTotal) ?? 0;
    }
}

// ============================================================
// BrandSettingsService
// ============================================================
public class BrandSettingsService : IBrandSettingsService
{
    private readonly AppDbContext _context;
    public BrandSettingsService(AppDbContext context) => _context = context;

    public async Task<BrandSettingsDto> GetSettingsAsync(int brandId)
    {
        // إرجاع إعدادات افتراضية لو brandId غير صحيح
        if (brandId <= 0)
            return new BrandSettingsDto
            {
                ShowTaxInPOS = true,
                ShowProfitInPOS = false,
                ShowProductImages = true,
                HideZeroStockProducts = true,
                PrimaryColor = "#92400e",
                ReceiptHeader = "",
                ReceiptFooter = "شكراً لتعاملكم معنا"
            };

        var settings = await _context.BrandSettings.FirstOrDefaultAsync(s => s.BrandId == brandId);
        if (settings == null)
        {
            // التأكد من إن البراند موجود قبل ما نضيف settings
            var brandExists = await _context.Brands.AnyAsync(b => b.BrandId == brandId);
            if (!brandExists)
            {
                return new BrandSettingsDto
                {
                    ShowTaxInPOS = true,
                    ShowProfitInPOS = false,
                    ShowProductImages = true,
                    HideZeroStockProducts = true,
                    PrimaryColor = "#92400e",
                    ReceiptHeader = "",
                    ReceiptFooter = "شكراً لتعاملكم معنا"
                };
            }
            settings = new BrandSettings { BrandId = brandId };
            _context.BrandSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return new BrandSettingsDto
        {
            ShowTaxInPOS = settings.ShowTaxInPOS,
            ShowProfitInPOS = settings.ShowProfitInPOS,
            ShowProductImages = settings.ShowProductImages,
            HideZeroStockProducts = settings.HideZeroStockProducts,
            PrimaryColor = settings.PrimaryColor,
            ReceiptHeader = settings.ReceiptHeader,
            ReceiptFooter = settings.ReceiptFooter
        };
    }

    public async Task<bool> UpdateSettingsAsync(int brandId, BrandSettingsDto dto)
    {
        var settings = await _context.BrandSettings.FirstOrDefaultAsync(s => s.BrandId == brandId);
        if (settings == null)
        {
            settings = new BrandSettings { BrandId = brandId };
            _context.BrandSettings.Add(settings);
        }

        settings.ShowTaxInPOS = dto.ShowTaxInPOS;
        settings.ShowProfitInPOS = dto.ShowProfitInPOS;
        settings.ShowProductImages = dto.ShowProductImages;
        settings.HideZeroStockProducts = dto.HideZeroStockProducts;
        settings.PrimaryColor = dto.PrimaryColor;
        settings.ReceiptHeader = dto.ReceiptHeader;
        settings.ReceiptFooter = dto.ReceiptFooter;

        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// AdvancedDashboardService - مبسط جداً
// ============================================================
public class AdvancedDashboardService : IAdvancedDashboardService
{
    private readonly AppDbContext _context;
    public AdvancedDashboardService(AppDbContext context) => _context = context;

    public async Task<AdvancedDashboardDto> GetDashboardAsync(int brandId, int? branchId = null, DateTime? customFrom = null, DateTime? customTo = null)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var yearStart = new DateTime(today.Year, 1, 1);
        var allTimeStart = new DateTime(2000, 1, 1);

        var dashboard = new AdvancedDashboardDto
        {
            Today = await GetPeriodStats(brandId, branchId, today, now, "اليوم"),
            Week = await GetPeriodStats(brandId, branchId, weekStart, now, "الأسبوع"),
            Month = await GetPeriodStats(brandId, branchId, monthStart, now, "الشهر"),
            Year = await GetPeriodStats(brandId, branchId, yearStart, now, "السنة"),
            AllTime = await GetPeriodStats(brandId, branchId, allTimeStart, now, "كل الوقت")
        };

        // إحصائيات عامة
        try
        {
            dashboard.TotalProducts = await _context.Products.CountAsync(p => p.BrandId == brandId && p.IsActive);
            dashboard.TotalCustomers = await _context.Customers.CountAsync(c => c.BrandId == brandId && c.IsActive);
            
            dashboard.LowStockCount = await _context.Inventory
                .Where(i => i.Variant.Product.BrandId == brandId)
                .CountAsync(i => i.Quantity <= i.Variant.Product.MinStock);

            // قيمة المخزون - استخدم client side عشان نتجنب مشاكل linq
            var invItems = await _context.Inventory
                .Where(i => i.Variant.Product.BrandId == brandId)
                .Select(i => new { i.Quantity, i.AverageCost })
                .ToListAsync();
            dashboard.InventoryValue = invItems.Sum(i => i.Quantity * i.AverageCost);

            // أكتر المنتجات مبيعاً
            var thirtyDaysAgo = now.AddDays(-30);
            dashboard.TopProducts = await _context.SalesInvoiceItems
                .Where(it => it.Invoice.BrandId == brandId && it.Invoice.CreatedAt >= thirtyDaysAgo)
                .GroupBy(it => new { it.VariantId, ProductName = it.Variant.VariantName ?? it.Variant.Product!.ProductName })
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToListAsync();
        }
        catch
        {
            // لو في خطأ في الإحصائيات الإضافية، نرجع dashboard بدون مشكلة
        }

        return dashboard;
    }

    private async Task<PeriodStatsDto> GetPeriodStats(int brandId, int? branchId, DateTime from, DateTime to, string periodName)
    {
        var stats = new PeriodStatsDto { PeriodName = periodName };

        try
        {
            var salesQuery = _context.SalesInvoices.Where(i => i.BrandId == brandId && i.CreatedAt >= from && i.CreatedAt <= to);
            if (branchId.HasValue) salesQuery = salesQuery.Where(i => i.BranchId == branchId.Value);

            stats.TotalSales = await salesQuery.SumAsync(i => (decimal?)i.GrandTotal) ?? 0;
            stats.InvoicesCount = await salesQuery.CountAsync();

            // COGS - تجنب الحسابات المعقدة
            var soldItems = await _context.SalesInvoiceItems
                .Where(it => it.Invoice.BrandId == brandId && it.Invoice.CreatedAt >= from && it.Invoice.CreatedAt <= to)
                .Select(it => new { it.UnitCost, it.Quantity })
                .ToListAsync();
            stats.CostOfGoodsSold = soldItems.Sum(it => (it.UnitCost ?? 0) * it.Quantity);

            // المشتريات
            var purchasesQuery = _context.PurchaseInvoices.Where(p => p.BrandId == brandId && p.InvoiceDate >= from && p.InvoiceDate <= to);
            if (branchId.HasValue) purchasesQuery = purchasesQuery.Where(p => p.BranchId == branchId.Value);
            stats.TotalPurchases = await purchasesQuery.SumAsync(p => (decimal?)p.GrandTotal) ?? 0;

            // المصاريف
            var expensesQuery = _context.Expenses.Where(e => e.BrandId == brandId && e.IsActive && e.ExpenseDate >= from && e.ExpenseDate <= to);
            stats.TotalExpenses = await expensesQuery.SumAsync(e => (decimal?)e.Amount) ?? 0;

            // الحسابات
            stats.GrossProfit = stats.TotalSales - stats.CostOfGoodsSold;
            stats.NetProfit = stats.GrossProfit - stats.TotalExpenses;
            stats.AverageProfitPerSale = stats.InvoicesCount > 0 ? stats.GrossProfit / stats.InvoicesCount : 0;
            stats.ExpensesCovered = stats.GrossProfit >= stats.TotalExpenses;
        }
        catch
        {
            // في حالة الخطأ، نرجع stats فاضية
        }

        return stats;
    }
}
