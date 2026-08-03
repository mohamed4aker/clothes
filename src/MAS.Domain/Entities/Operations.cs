using MAS.Domain.Common;

namespace MAS.Domain.Entities;

/// <summary>
/// المصاريف
/// </summary>
public class Expense : BaseEntity
{
    public int ExpenseId { get; set; }
    public int BrandId { get; set; }
    public int? BranchId { get; set; }
    public string? ExpenseNumber { get; set; }
    public string Category { get; set; } = string.Empty; // إيجار، كهربا، رواتب، نقل
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public string? Notes { get; set; }
    
    public Brand Brand { get; set; } = null!;
}

/// <summary>
/// فاتورة مشتريات (مبسطة)
/// </summary>
public class PurchaseInvoice : BaseEntity
{
    public int PurchaseInvoiceId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public string PurchaseType { get; set; } = "Finished";  // Finished, RawMaterial
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public string? SupplierPhone { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}

public class PurchaseInvoiceItem
{
    public int ItemId { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    
    public PurchaseInvoice Invoice { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}

/// <summary>
/// إعدادات البراند (إخفاء الضريبة، الربحية، إلخ)
/// </summary>
public class BrandSettings
{
    public int BrandSettingsId { get; set; }
    public int BrandId { get; set; }
    public bool ShowTaxInPOS { get; set; } = true;
    public bool ShowProfitInPOS { get; set; } = false;
    public bool ShowProductImages { get; set; } = true;
    public bool HideZeroStockProducts { get; set; } = true;
    public string PrimaryColor { get; set; } = "#0d9488";
    public string ReceiptHeader { get; set; } = string.Empty;
    public string ReceiptFooter { get; set; } = "شكراً لتسوقكم معنا";
    
    public Brand Brand { get; set; } = null!;
}
