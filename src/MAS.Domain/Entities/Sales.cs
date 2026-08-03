using MAS.Domain.Common;
using MAS.Domain.Enums;

namespace MAS.Domain.Entities;

/// <summary>
/// العملاء
/// </summary>
public class Customer : BaseEntity
{
    public int CustomerId { get; set; }
    public int BrandId { get; set; }
    public string? CustomerCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? TaxNumber { get; set; }
    public string CustomerType { get; set; } = "Retail";
    public int LoyaltyPoints { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public bool HasOnlineAccount { get; set; }
    public string? PasswordHash { get; set; }
    public string PreferredLanguage { get; set; } = "ar";
    public bool AcceptMarketing { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime? LastPurchaseAt { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
}

/// <summary>
/// جلسة الكاشير (شفت)
/// </summary>
public class CashierSession
{
    public int SessionId { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int UserId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ExpectedBalance { get; set; }
    public decimal? Difference { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal TotalWallet { get; set; }
    public DateTime SessionStart { get; set; } = DateTime.UtcNow;
    public DateTime? SessionEnd { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    
    // العلاقات
    public Branch Branch { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<SalesInvoice> Invoices { get; set; } = new List<SalesInvoice>();
}

/// <summary>
/// فاتورة البيع
/// </summary>
public class SalesInvoice
{
    public int InvoiceId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = "POS";
    public string SaleType { get; set; } = "Cash";
    public int? CustomerId { get; set; }
    public int CashierUserId { get; set; }
    public int? SessionId { get; set; }
    
    // المبالغ
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal PromotionDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    
    public int? AppliedPromotionId { get; set; }
    public string? AppliedCouponCode { get; set; }
    public int LoyaltyPointsEarned { get; set; }
    public int LoyaltyPointsRedeemed { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Completed;
    
    // الفوترة الإلكترونية
    public string? ETASubmissionStatus { get; set; }
    public string? ETAUUID { get; set; }
    public DateTime? ETASubmittedAt { get; set; }
    
    public bool IsOnlineOrder { get; set; }
    public int? OnlineOrderId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Customer? Customer { get; set; }
    public User CashierUser { get; set; } = null!;
    public CashierSession? Session { get; set; }
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
}

/// <summary>
/// بنود الفاتورة
/// </summary>
public class SalesInvoiceItem
{
    public int ItemId { get; set; }
    public int InvoiceId { get; set; }
    public int VariantId { get; set; }
    public int? BatchId { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitId { get; set; }
    public decimal ConversionFactor { get; set; } = 1;
    public decimal? UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PromotionDiscount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int? AppliedPromotionId { get; set; }
    public string? Notes { get; set; }
    
    // العلاقات
    public SalesInvoice Invoice { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}

/// <summary>
/// مدفوعات الفاتورة
/// </summary>
public class InvoicePayment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public SalesInvoice Invoice { get; set; } = null!;
}

/// <summary>
/// حركة المخزون
/// </summary>
public class StockMovement
{
    public long MovementId { get; set; }
    public int BranchId { get; set; }
    public int VariantId { get; set; }
    public int? BatchId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public Branch Branch { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
