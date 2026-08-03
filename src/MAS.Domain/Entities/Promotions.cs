using MAS.Domain.Common;

namespace MAS.Domain.Entities;

/// <summary>
/// عرض / كوبون
/// </summary>
public class Promotion : BaseEntity
{
    public int PromotionId { get; set; }
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CouponCode { get; set; }
    public string DiscountType { get; set; } = "Percentage"; // Percentage, FixedAmount
    public decimal DiscountValue { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool ApplyAutomatically { get; set; }
    public string? Notes { get; set; }
    
    public Brand Brand { get; set; } = null!;
}
