using MAS.Domain.Common;

namespace MAS.Domain.Entities;

/// <summary>
/// مرتجع مبيعات
/// </summary>
public class SalesReturn : BaseEntity
{
    public int ReturnId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public int OriginalInvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public int CashierUserId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundMethod { get; set; } = "Cash"; // Cash, StoreCredit
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public SalesInvoice OriginalInvoice { get; set; } = null!;
    public Customer? Customer { get; set; }
    public User CashierUser { get; set; } = null!;
    public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}

public class SalesReturnItem
{
    public int ItemId { get; set; }
    public int ReturnId { get; set; }
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public bool BackToStock { get; set; } = true;
    public string? ItemReason { get; set; }
    
    public SalesReturn SalesReturn { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}

/// <summary>
/// عملية جرد للمخزون
/// </summary>
public class StockTake : BaseEntity
{
    public int StockTakeId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public string StockTakeNumber { get; set; } = string.Empty;
    public DateTime StockTakeDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Draft"; // Draft, Completed
    public int? UserId { get; set; }
    public string? Notes { get; set; }
    public decimal TotalDifferenceValue { get; set; }
    
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<StockTakeItem> Items { get; set; } = new List<StockTakeItem>();
}

public class StockTakeItem
{
    public int ItemId { get; set; }
    public int StockTakeId { get; set; }
    public int VariantId { get; set; }
    public decimal SystemQuantity { get; set; }      // الموجود في النظام
    public decimal CountedQuantity { get; set; }     // الموجود فعلاً
    public decimal Difference { get; set; }          // الفرق
    public decimal UnitCost { get; set; }
    public decimal DifferenceValue { get; set; }
    public string? Notes { get; set; }
    
    public StockTake StockTake { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
