using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

public class ManufacturingService : IManufacturingService
{
    private readonly AppDbContext _context;
    public ManufacturingService(AppDbContext context) => _context = context;

    // ========== BOM ==========

    public async Task<List<BomDto>> GetBomsAsync(int brandId)
    {
        return await _context.BillOfMaterials
            .Where(b => b.BrandId == brandId && b.IsActive)
            .Include(b => b.Product)
            .Include(b => b.Items).ThenInclude(i => i.RawMaterialVariant).ThenInclude(v => v.Product)
            .Include(b => b.Items).ThenInclude(i => i.Unit)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BomDto
            {
                BomId = b.BomId,
                ProductId = b.ProductId,
                ProductName = b.Product.ProductName,
                BomName = b.BomName,
                Version = b.Version,
                DirectLaborCost = b.DirectLaborCost,
                OverheadCost = b.OverheadCost,
                AdminExpenses = b.AdminExpenses,
                OtherCosts = b.OtherCosts,
                OtherCostsDescription = b.OtherCostsDescription,
                ExpectedWastePercent = b.ExpectedWastePercent,
                TotalMaterialsCost = b.TotalMaterialsCost,
                TotalCost = b.TotalCost,
                SuggestedSellingPrice = b.SuggestedSellingPrice,
                TargetMarginPercent = b.TargetMarginPercent,
                OutputQuantity = b.OutputQuantity,
                Notes = b.Notes,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                Items = b.Items.Select(i => new BomItemDto
                {
                    BomItemId = i.BomItemId,
                    RawMaterialVariantId = i.RawMaterialVariantId,
                    RawMaterialName = i.RawMaterialVariant.VariantName ?? i.RawMaterialVariant.Product!.ProductName,
                    Quantity = i.Quantity,
                    UnitId = i.UnitId,
                    UnitName = i.Unit != null ? i.Unit.UnitName : null,
                    UnitCost = i.UnitCost,
                    LineCost = i.LineCost,
                    Notes = i.Notes
                }).ToList()
            }).ToListAsync();
    }

    public async Task<BomDto?> GetBomByIdAsync(int bomId, int brandId)
    {
        var boms = await GetBomsAsync(brandId);
        return boms.FirstOrDefault(b => b.BomId == bomId);
    }

    public async Task<int> CreateBomAsync(int brandId, int userId, CreateBomRequest request)
    {
        var bom = new BillOfMaterials
        {
            BrandId = brandId,
            ProductId = request.ProductId,
            BomName = request.BomName,
            DirectLaborCost = request.DirectLaborCost,
            OverheadCost = request.OverheadCost,
            AdminExpenses = request.AdminExpenses,
            OtherCosts = request.OtherCosts,
            OtherCostsDescription = request.OtherCostsDescription,
            ExpectedWastePercent = request.ExpectedWastePercent,
            TargetMarginPercent = request.TargetMarginPercent,
            OutputQuantity = request.OutputQuantity,
            Notes = request.Notes,
            CreatedBy = userId
        };

        decimal materialsCost = 0;
        foreach (var item in request.Items)
        {
            var lineCost = item.Quantity * item.UnitCost;
            materialsCost += lineCost;

            bom.Items.Add(new BomItem
            {
                RawMaterialVariantId = item.RawMaterialVariantId,
                Quantity = item.Quantity,
                UnitId = item.UnitId,
                UnitCost = item.UnitCost,
                LineCost = lineCost,
                Notes = item.Notes
            });
        }

        // حساب التكاليف
        bom.TotalMaterialsCost = materialsCost;
        var wasteAmount = materialsCost * (bom.ExpectedWastePercent / 100);
        bom.TotalCost = materialsCost + wasteAmount + bom.DirectLaborCost + bom.OverheadCost + bom.AdminExpenses + bom.OtherCosts;
        bom.SuggestedSellingPrice = bom.TotalCost * (1 + bom.TargetMarginPercent / 100);

        // قسمة على عدد الوحدات الناتجة
        if (bom.OutputQuantity > 1)
        {
            bom.TotalCost /= bom.OutputQuantity;
            bom.SuggestedSellingPrice /= bom.OutputQuantity;
        }

        _context.BillOfMaterials.Add(bom);
        await _context.SaveChangesAsync();
        return bom.BomId;
    }

    public async Task<bool> UpdateBomAsync(int bomId, int brandId, CreateBomRequest request)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BomId == bomId && b.BrandId == brandId);
        if (bom == null) return false;

        bom.BomName = request.BomName;
        bom.DirectLaborCost = request.DirectLaborCost;
        bom.OverheadCost = request.OverheadCost;
        bom.AdminExpenses = request.AdminExpenses;
        bom.OtherCosts = request.OtherCosts;
        bom.OtherCostsDescription = request.OtherCostsDescription;
        bom.ExpectedWastePercent = request.ExpectedWastePercent;
        bom.TargetMarginPercent = request.TargetMarginPercent;
        bom.OutputQuantity = request.OutputQuantity;
        bom.Notes = request.Notes;
        bom.UpdatedAt = DateTime.UtcNow;

        // حذف العناصر القديمة وإضافة الجديدة
        _context.BomItems.RemoveRange(bom.Items);

        decimal materialsCost = 0;
        foreach (var item in request.Items)
        {
            var lineCost = item.Quantity * item.UnitCost;
            materialsCost += lineCost;
            bom.Items.Add(new BomItem
            {
                RawMaterialVariantId = item.RawMaterialVariantId,
                Quantity = item.Quantity,
                UnitId = item.UnitId,
                UnitCost = item.UnitCost,
                LineCost = lineCost,
                Notes = item.Notes
            });
        }

        bom.TotalMaterialsCost = materialsCost;
        var wasteAmount = materialsCost * (bom.ExpectedWastePercent / 100);
        bom.TotalCost = materialsCost + wasteAmount + bom.DirectLaborCost + bom.OverheadCost + bom.AdminExpenses + bom.OtherCosts;
        bom.SuggestedSellingPrice = bom.TotalCost * (1 + bom.TargetMarginPercent / 100);

        if (bom.OutputQuantity > 1)
        {
            bom.TotalCost /= bom.OutputQuantity;
            bom.SuggestedSellingPrice /= bom.OutputQuantity;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBomAsync(int bomId, int brandId)
    {
        var bom = await _context.BillOfMaterials.FirstOrDefaultAsync(b => b.BomId == bomId && b.BrandId == brandId);
        if (bom == null) return false;
        bom.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ========== حاسبة الإنتاج العكسية ==========

    public async Task<ProductionCapacityResponse> CalculateCapacityAsync(int bomId, int branchId, int brandId)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Product)
            .Include(b => b.Items).ThenInclude(i => i.RawMaterialVariant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(b => b.BomId == bomId && b.BrandId == brandId);

        if (bom == null) throw new Exception("الوصفة غير موجودة");

        var response = new ProductionCapacityResponse
        {
            BomId = bom.BomId,
            ProductName = bom.Product.ProductName,
            Materials = new List<MaterialAvailability>()
        };

        int minPossible = int.MaxValue;
        string? bottleneck = null;

        foreach (var item in bom.Items)
        {
            // المخزون المتاح من الخامة في الفرع
            var inventory = await _context.Inventory
                .FirstOrDefaultAsync(i => i.VariantId == item.RawMaterialVariantId && i.BranchId == branchId);

            var availableStock = inventory?.Quantity ?? 0;
            var possibleUnits = item.Quantity > 0 ? (int)(availableStock / item.Quantity) : 0;

            var material = new MaterialAvailability
            {
                MaterialName = item.RawMaterialVariant.VariantName ?? item.RawMaterialVariant.Product!.ProductName,
                RequiredPerUnit = item.Quantity,
                AvailableStock = availableStock,
                PossibleUnits = possibleUnits
            };

            if (possibleUnits < minPossible)
            {
                minPossible = possibleUnits;
                bottleneck = material.MaterialName;
            }

            response.Materials.Add(material);
        }

        // تحديد الـ bottleneck
        foreach (var m in response.Materials)
        {
            m.IsBottleneck = m.MaterialName == bottleneck;
            m.NeededForTarget = m.RequiredPerUnit * minPossible;
            m.Shortage = m.NeededForTarget - m.AvailableStock;
            if (m.Shortage < 0) m.Shortage = 0;
        }

        response.MaxPossibleUnits = minPossible == int.MaxValue ? 0 : minPossible;
        response.BottleneckMaterial = bottleneck;

        return response;
    }

    // ========== أوامر الإنتاج ==========

    public async Task<List<ProductionOrderDto>> GetProductionOrdersAsync(int brandId)
    {
        return await _context.ProductionOrders
            .Where(p => p.BrandId == brandId)
            .Include(p => p.Product)
            .Include(p => p.Branch)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductionOrderDto
            {
                ProductionOrderId = p.ProductionOrderId,
                OrderNumber = p.OrderNumber,
                ProductName = p.Product.ProductName,
                BranchName = p.Branch.BranchName,
                PlannedQuantity = p.PlannedQuantity,
                ActualQuantity = p.ActualQuantity,
                WasteQuantity = p.WasteQuantity,
                Status = p.Status,
                TotalProductionCost = p.TotalProductionCost,
                UnitProductionCost = p.UnitProductionCost,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt,
                Notes = p.Notes
            }).ToListAsync();
    }

    public async Task<int> CreateProductionOrderAsync(int brandId, int userId, CreateProductionOrderRequest request)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BomId == request.BomId && b.BrandId == brandId);
        if (bom == null) throw new Exception("الوصفة غير موجودة");

        // التحقق من توفر الخامات
        foreach (var item in bom.Items)
        {
            var needed = item.Quantity * request.PlannedQuantity;
            var inv = await _context.Inventory
                .FirstOrDefaultAsync(i => i.VariantId == item.RawMaterialVariantId && i.BranchId == request.BranchId);

            if (inv == null || inv.Quantity < needed)
            {
                var matName = await _context.ProductVariants
                    .Where(v => v.VariantId == item.RawMaterialVariantId)
                    .Select(v => v.VariantName)
                    .FirstOrDefaultAsync();
                throw new Exception($"المخزون غير كاف من الخامة: {matName}");
            }
        }

        // توليد رقم الأمر
        var lastOrder = await _context.ProductionOrders
            .Where(p => p.BrandId == brandId)
            .OrderByDescending(p => p.ProductionOrderId)
            .Select(p => p.OrderNumber)
            .FirstOrDefaultAsync();
        var orderNumber = GenerateOrderNumber(lastOrder);

        var order = new ProductionOrder
        {
            BrandId = brandId,
            BranchId = request.BranchId,
            OrderNumber = orderNumber,
            BomId = bom.BomId,
            ProductId = bom.ProductId,
            PlannedQuantity = request.PlannedQuantity,
            Status = "Draft",
            TotalMaterialsCost = bom.TotalMaterialsCost * request.PlannedQuantity,
            TotalLaborCost = bom.DirectLaborCost * request.PlannedQuantity,
            TotalOverhead = bom.OverheadCost * request.PlannedQuantity,
            OtherCosts = (bom.AdminExpenses + bom.OtherCosts) * request.PlannedQuantity,
            CreatedBy = userId,
            Notes = request.Notes
        };

        order.TotalProductionCost = order.TotalMaterialsCost + order.TotalLaborCost + order.TotalOverhead + order.OtherCosts;

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();
        return order.ProductionOrderId;
    }

    public async Task<bool> CompleteProductionAsync(int brandId, int userId, CompleteProductionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.ProductionOrders
                .Include(p => p.Bom).ThenInclude(b => b.Items)
                .Include(p => p.Product).ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(p => p.ProductionOrderId == request.ProductionOrderId && p.BrandId == brandId);
            if (order == null) return false;

            // خصم الخامات الفعلية من المخزون
            decimal actualMaterialsCost = 0;
            foreach (var usage in request.MaterialsUsed)
            {
                var inv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == usage.RawMaterialVariantId && i.BranchId == order.BranchId);
                if (inv == null) continue;

                var totalToDeduct = usage.ActualQuantity + usage.WasteQuantity;
                if (inv.Quantity < totalToDeduct)
                    throw new Exception("المخزون غير كاف");

                inv.Quantity -= totalToDeduct;
                inv.LastUpdated = DateTime.UtcNow;

                // خصم الكمية المستخدمة + الهالك
                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = order.BranchId,
                    VariantId = usage.RawMaterialVariantId,
                    MovementType = StockMovementType.ProductionOut,
                    Quantity = -usage.ActualQuantity,
                    BalanceAfter = inv.Quantity,
                    ReferenceType = "ProductionOrder",
                    ReferenceId = order.ProductionOrderId,
                    Notes = $"إنتاج - أمر {order.OrderNumber}",
                    UserId = userId
                });

                if (usage.WasteQuantity > 0)
                {
                    _context.StockMovements.Add(new StockMovement
                    {
                        BranchId = order.BranchId,
                        VariantId = usage.RawMaterialVariantId,
                        MovementType = StockMovementType.Waste,
                        Quantity = -usage.WasteQuantity,
                        BalanceAfter = inv.Quantity,
                        ReferenceType = "ProductionOrder",
                        ReferenceId = order.ProductionOrderId,
                        Notes = $"هالك - أمر {order.OrderNumber}",
                        UserId = userId
                    });
                }

                // حساب التكلفة الفعلية
                var bomItem = order.Bom.Items.FirstOrDefault(i => i.RawMaterialVariantId == usage.RawMaterialVariantId);
                if (bomItem != null)
                {
                    var lineActualCost = totalToDeduct * bomItem.UnitCost;
                    actualMaterialsCost += lineActualCost;

                    order.MaterialsUsed.Add(new ProductionMaterialUsage
                    {
                        RawMaterialVariantId = usage.RawMaterialVariantId,
                        PlannedQuantity = bomItem.Quantity * order.PlannedQuantity,
                        ActualQuantity = usage.ActualQuantity,
                        WasteQuantity = usage.WasteQuantity,
                        UnitCost = bomItem.UnitCost,
                        TotalCost = lineActualCost
                    });
                }
            }

            // إضافة المنتج النهائي للمخزون
            var firstVariant = order.Product.Variants.FirstOrDefault();
            if (firstVariant != null)
            {
                var prodInv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == firstVariant.VariantId && i.BranchId == order.BranchId);

                var unitCost = request.ActualQuantity > 0 
                    ? (actualMaterialsCost + order.TotalLaborCost + order.TotalOverhead + order.OtherCosts) / request.ActualQuantity 
                    : 0;

                if (prodInv == null)
                {
                    prodInv = new Inventory
                    {
                        BranchId = order.BranchId,
                        VariantId = firstVariant.VariantId,
                        Quantity = request.ActualQuantity,
                        AverageCost = unitCost
                    };
                    _context.Inventory.Add(prodInv);
                }
                else
                {
                    var newTotalCost = (prodInv.Quantity * prodInv.AverageCost) + (request.ActualQuantity * unitCost);
                    var newTotalQty = prodInv.Quantity + request.ActualQuantity;
                    prodInv.AverageCost = newTotalQty > 0 ? newTotalCost / newTotalQty : 0;
                    prodInv.Quantity = newTotalQty;
                    prodInv.LastUpdated = DateTime.UtcNow;
                }

                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = order.BranchId,
                    VariantId = firstVariant.VariantId,
                    MovementType = StockMovementType.ProductionIn,
                    Quantity = request.ActualQuantity,
                    UnitCost = unitCost,
                    BalanceAfter = prodInv.Quantity,
                    ReferenceType = "ProductionOrder",
                    ReferenceId = order.ProductionOrderId,
                    Notes = $"منتج جديد - أمر {order.OrderNumber}",
                    UserId = userId
                });

                order.UnitProductionCost = unitCost;
            }

            // تحديث الأمر
            order.ActualQuantity = request.ActualQuantity;
            order.WasteQuantity = request.WasteQuantity;
            order.TotalMaterialsCost = actualMaterialsCost;
            order.TotalProductionCost = actualMaterialsCost + order.TotalLaborCost + order.TotalOverhead + order.OtherCosts;
            order.Status = "Completed";
            order.CompletedAt = DateTime.UtcNow;
            order.CompletedBy = userId;
            if (!string.IsNullOrEmpty(request.Notes))
                order.Notes = (order.Notes ?? "") + "\n" + request.Notes;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> CancelProductionOrderAsync(int orderId, int brandId)
    {
        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(p => p.ProductionOrderId == orderId && p.BrandId == brandId);
        if (order == null) return false;
        if (order.Status == "Completed") throw new Exception("لا يمكن إلغاء أمر مكتمل");
        order.Status = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateOrderNumber(string? lastOrderNumber)
    {
        var prefix = $"PO-{DateTime.UtcNow:yyyyMM}-";
        if (string.IsNullOrEmpty(lastOrderNumber) || !lastOrderNumber.StartsWith(prefix))
            return $"{prefix}001";
        var lastNumber = int.Parse(lastOrderNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D3}";
    }
}
