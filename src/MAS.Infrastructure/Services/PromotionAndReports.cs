using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

public class PromotionService : IPromotionService
{
    private readonly AppDbContext _context;
    public PromotionService(AppDbContext context) => _context = context;

    public async Task<List<PromotionDto>> GetPromotionsAsync(int brandId)
    {
        return await _context.Promotions
            .Where(p => p.BrandId == brandId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromotionDto
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                CouponCode = p.CouponCode,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MinPurchaseAmount = p.MinPurchaseAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                ApplyAutomatically = p.ApplyAutomatically,
                IsActive = p.IsActive
            }).ToListAsync();
    }

    public async Task<int> CreatePromotionAsync(int brandId, CreatePromotionRequest request)
    {
        var promo = new Promotion
        {
            BrandId = brandId,
            Name = request.Name,
            Description = request.Description,
            CouponCode = request.CouponCode,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinPurchaseAmount = request.MinPurchaseAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            ApplyAutomatically = request.ApplyAutomatically
        };
        _context.Promotions.Add(promo);
        await _context.SaveChangesAsync();
        return promo.PromotionId;
    }

    public async Task<bool> UpdatePromotionAsync(int promotionId, int brandId, CreatePromotionRequest request)
    {
        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == promotionId && p.BrandId == brandId);
        if (promo == null) return false;
        promo.Name = request.Name;
        promo.Description = request.Description;
        promo.CouponCode = request.CouponCode;
        promo.DiscountType = request.DiscountType;
        promo.DiscountValue = request.DiscountValue;
        promo.MinPurchaseAmount = request.MinPurchaseAmount;
        promo.MaxDiscountAmount = request.MaxDiscountAmount;
        promo.StartDate = request.StartDate;
        promo.EndDate = request.EndDate;
        promo.UsageLimit = request.UsageLimit;
        promo.ApplyAutomatically = request.ApplyAutomatically;
        promo.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePromotionAsync(int promotionId, int brandId)
    {
        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == promotionId && p.BrandId == brandId);
        if (promo == null) return false;
        promo.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PromotionDto?> ValidateCouponAsync(string code, int brandId, decimal cartTotal)
    {
        var now = DateTime.UtcNow;
        var promo = await _context.Promotions
            .FirstOrDefaultAsync(p => p.BrandId == brandId && p.IsActive && p.CouponCode == code
                && p.StartDate <= now && p.EndDate >= now);
        if (promo == null) return null;
        if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value) return null;
        if (promo.MinPurchaseAmount.HasValue && cartTotal < promo.MinPurchaseAmount.Value) return null;

        return new PromotionDto
        {
            PromotionId = promo.PromotionId,
            Name = promo.Name,
            CouponCode = promo.CouponCode,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            MaxDiscountAmount = promo.MaxDiscountAmount,
            IsActive = true
        };
    }
}

public class ReportsService : IReportsService
{
    private readonly AppDbContext _context;
    public ReportsService(AppDbContext context) => _context = context;

    public async Task<SalesReportDto> GetSalesReportAsync(int brandId, DateTime from, DateTime to, int? branchId = null)
    {
        var report = new SalesReportDto { From = from, To = to };

        try
        {
            var query = _context.SalesInvoices.Where(i => i.BrandId == brandId && i.CreatedAt >= from && i.CreatedAt <= to);
            if (branchId.HasValue) query = query.Where(i => i.BranchId == branchId.Value);

            report.TotalSales = await query.SumAsync(i => (decimal?)i.GrandTotal) ?? 0;
            report.TotalDiscounts = await query.SumAsync(i => (decimal?)i.DiscountAmount) ?? 0;
            report.TotalTax = await query.SumAsync(i => (decimal?)i.TaxAmount) ?? 0;
            report.InvoicesCount = await query.CountAsync();

            var soldItems = await _context.SalesInvoiceItems
                .Where(it => it.Invoice.BrandId == brandId && it.Invoice.CreatedAt >= from && it.Invoice.CreatedAt <= to)
                .Select(it => new { it.UnitCost, it.Quantity })
                .ToListAsync();
            report.TotalCOGS = soldItems.Sum(it => (it.UnitCost ?? 0) * it.Quantity);
            report.GrossProfit = report.TotalSales - report.TotalCOGS;

            var rawDaily = await query
                .Select(i => new { Date = i.CreatedAt.Date, i.GrandTotal })
                .ToListAsync();
            
            report.DailySales = rawDaily
                .GroupBy(x => x.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    Sales = g.Sum(x => x.GrandTotal),
                    InvoicesCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            report.TopProducts = await _context.SalesInvoiceItems
                .Where(it => it.Invoice.BrandId == brandId && it.Invoice.CreatedAt >= from && it.Invoice.CreatedAt <= to)
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

            report.TopCustomers = await query
                .Where(i => i.CustomerId.HasValue)
                .GroupBy(i => new { i.CustomerId, CustomerName = i.Customer!.FullName })
                .Select(g => new TopCustomerDto
                {
                    CustomerName = g.Key.CustomerName,
                    InvoicesCount = g.Count(),
                    TotalSpent = g.Sum(x => x.GrandTotal)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();
        }
        catch { }

        return report;
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync(int brandId, int? branchId = null)
    {
        var report = new InventoryReportDto();

        try
        {
            var query = _context.Inventory.Where(i => i.Variant.Product.BrandId == brandId);
            if (branchId.HasValue) query = query.Where(i => i.BranchId == branchId.Value);

            var items = await query
                .Include(i => i.Variant).ThenInclude(v => v.Product)
                .Include(i => i.Branch)
                .Select(i => new InventoryItemDto
                {
                    ProductName = i.Variant.VariantName ?? i.Variant.Product!.ProductName,
                    SKU = i.Variant.SKU,
                    BranchName = i.Branch.BranchName,
                    Quantity = i.Quantity,
                    AverageCost = i.AverageCost,
                    SellingPrice = i.Variant.SellingPrice ?? i.Variant.Product!.BaseSellingPrice,
                    TotalValue = i.Quantity * i.AverageCost,
                    MinStock = i.Variant.Product!.MinStock,
                    Status = i.Quantity == 0 ? "نفذ" : i.Quantity <= i.Variant.Product.MinStock ? "منخفض" : "متوفر"
                }).ToListAsync();

            report.Items = items;
            report.TotalValue = items.Sum(i => i.TotalValue);
            report.TotalProducts = items.Count;
            report.LowStockCount = items.Count(i => i.Status == "منخفض");
            report.OutOfStockCount = items.Count(i => i.Status == "نفذ");
        }
        catch { }

        return report;
    }
}
