using MAS.Application.Common;
using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly AppDbContext _context;
    public PurchaseReturnService(AppDbContext context) => _context = context;

    public async Task<List<PurchaseReturnDto>> GetReturnsAsync(int brandId)
    {
        return await _context.PurchaseReturns
            .Where(r => r.BrandId == brandId && r.IsActive)
            .Include(r => r.Branch)
            .Include(r => r.PurchaseInvoice)
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Items)
            .OrderByDescending(r => r.ReturnDate)
            .Select(r => new PurchaseReturnDto
            {
                PurchaseReturnId = r.PurchaseReturnId,
                ReturnNumber = r.ReturnNumber,
                ReturnDate = r.ReturnDate,
                PurchaseInvoiceNumber = r.PurchaseInvoice != null ? r.PurchaseInvoice.InvoiceNumber : null,
                SupplierName = r.Supplier != null ? r.Supplier.SupplierName : null,
                WarehouseName = r.Warehouse != null ? r.Warehouse.WarehouseName : null,
                BranchName = r.Branch.BranchName,
                Reason = r.Reason,
                SubTotal = r.SubTotal,
                RefundAmount = r.RefundAmount,
                RefundMethod = r.RefundMethod,
                ItemsCount = r.Items.Count,
                Notes = r.Notes
            }).ToListAsync();
    }

    public async Task<int> CreateReturnAsync(int brandId, int userId, CreatePurchaseReturnRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // توليد رقم
            var lastReturn = await _context.PurchaseReturns
                .Where(r => r.BrandId == brandId)
                .OrderByDescending(r => r.PurchaseReturnId)
                .Select(r => r.ReturnNumber)
                .FirstOrDefaultAsync();
            var prefix = $"PRT-{DateTime.UtcNow:yyyyMM}-";
            var returnNumber = DocumentNumber.Next(prefix, lastReturn, 3);

            var purchaseReturn = new PurchaseReturn
            {
                BrandId = brandId,
                BranchId = request.BranchId,
                PurchaseInvoiceId = request.PurchaseInvoiceId,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                ReturnNumber = returnNumber,
                ReturnDate = DateTime.UtcNow,
                Reason = request.Reason,
                RefundAmount = request.RefundAmount,
                RefundMethod = request.RefundMethod,
                Notes = request.Notes,
                CreatedBy = userId
            };

            decimal subTotal = 0;
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0) continue;
                var lineTotal = item.Quantity * item.UnitCost;
                subTotal += lineTotal;

                purchaseReturn.Items.Add(new PurchaseReturnItem
                {
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = lineTotal
                });

                // خصم الكمية من المخزون
                var inv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == request.BranchId);
                if (inv != null)
                {
                    inv.Quantity -= item.Quantity;
                    inv.LastUpdated = DateTime.UtcNow;
                }

                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = request.BranchId,
                    VariantId = item.VariantId,
                    MovementType = StockMovementType.TransferOut,
                    Quantity = -item.Quantity,
                    UnitCost = item.UnitCost,
                    BalanceAfter = inv?.Quantity ?? 0,
                    ReferenceType = "PurchaseReturn",
                    Notes = $"مرتجع مشتريات - {returnNumber}",
                    UserId = userId
                });
            }

            purchaseReturn.SubTotal = subTotal;

            // تحديث رصيد المورد
            if (request.SupplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId.Value);
                if (supplier != null)
                {
                    supplier.TotalPurchases -= subTotal;
                    if (request.RefundMethod == "Credit")
                        supplier.CurrentBalance += request.RefundAmount;
                }
            }

            _context.PurchaseReturns.Add(purchaseReturn);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return purchaseReturn.PurchaseReturnId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
