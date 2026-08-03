using MAS.Domain.Common;

namespace MAS.Domain.Entities;

public class Supplier : BaseEntity
{
    public int SupplierId { get; set; }
    public int BrandId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public decimal CurrentBalance { get; set; }  // الرصيد الحالي (موجب=ليه عندنا، سالب=علينا)
    public decimal TotalPurchases { get; set; }
    public string? PaymentTerms { get; set; }    // شروط الدفع
    
    public Brand Brand { get; set; } = null!;
}
