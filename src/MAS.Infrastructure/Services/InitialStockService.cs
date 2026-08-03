using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

public class InitialStockService : IInitialStockService
{
    private readonly AppDbContext _context;
    public InitialStockService(AppDbContext context) => _context = context;

    public async Task<List<InitialStockDto>> GetInitialStocksAsync(int brandId)
    {
        return await _context.InitialStocks
            .Where(s => s.BrandId == brandId && s.IsActive)
            .Include(s => s.Branch)
            .Include(s => s.Warehouse)
            .Include(s => s.Items)
            .OrderByDescending(s => s.EntryDate)
            .Select(s => new InitialStockDto
            {
                InitialStockId = s.InitialStockId,
                EntryNumber = s.EntryNumber,
                BranchName = s.Branch.BranchName,
                WarehouseName = s.Warehouse != null ? s.Warehouse.WarehouseName : null,
                EntryDate = s.EntryDate,
                ItemsCount = s.Items.Count,
                TotalValue = s.Items.Sum(i => i.LineTotal),
                Notes = s.Notes
            }).ToListAsync();
    }

    public async Task<int> CreateInitialStockAsync(int brandId, int userId, CreateInitialStockRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // توليد رقم
            var lastEntry = await _context.InitialStocks
                .Where(s => s.BrandId == brandId)
                .OrderByDescending(s => s.InitialStockId)
                .Select(s => s.EntryNumber)
                .FirstOrDefaultAsync();
            var prefix = $"INI-{DateTime.UtcNow:yyyyMM}-";
            var entryNumber = string.IsNullOrEmpty(lastEntry) || !lastEntry.StartsWith(prefix)
                ? $"{prefix}001"
                : $"{prefix}{(int.Parse(lastEntry.Substring(prefix.Length)) + 1):D3}";

            var entry = new InitialStock
            {
                BrandId = brandId,
                BranchId = request.BranchId,
                WarehouseId = request.WarehouseId,
                EntryNumber = entryNumber,
                EntryDate = request.EntryDate,
                Notes = request.Notes,
                CreatedBy = userId
            };

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0) continue;
                var lineTotal = item.Quantity * item.UnitCost;
                entry.Items.Add(new InitialStockItem
                {
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = lineTotal
                });

                // إضافة الكمية للمخزون (بدون التأثير على الخزنة)
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
                    MovementType = StockMovementType.Adjustment,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    BalanceAfter = inv.Quantity,
                    ReferenceType = "InitialStock",
                    ReferenceId = entry.InitialStockId,
                    Notes = $"مخزن أول المدة - {entryNumber}",
                    UserId = userId
                });
            }

            _context.InitialStocks.Add(entry);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return entry.InitialStockId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
