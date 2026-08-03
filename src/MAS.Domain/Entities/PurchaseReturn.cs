using MAS.Domain.Common;

namespace MAS.Domain.Entities;

public class PurchaseReturn : BaseEntity
{
    public int PurchaseReturnId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public int? PurchaseInvoiceId { get; set; }  // الفاتورة الأصلية (اختياري)
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal RefundAmount { get; set; }       // المبلغ المسترد
    public string RefundMethod { get; set; } = "Cash";  // Cash, Visa, Credit (إنقاص دين المورد)
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public PurchaseInvoice? PurchaseInvoice { get; set; }
    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}

public class PurchaseReturnItem
{
    public int ItemId { get; set; }
    public int PurchaseReturnId { get; set; }
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    
    public PurchaseReturn PurchaseReturn { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
