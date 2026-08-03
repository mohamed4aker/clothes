using MAS.Domain.Common;

namespace MAS.Domain.Entities;

/// <summary>
/// طلب أونلاين من الموقع الإلكتروني
/// </summary>
public class OnlineOrder : BaseEntity
{
    public int OrderId { get; set; }
    public int BrandId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    // بيانات العميل
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // المبالغ
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal SellerShippingShare { get; set; }    // اللي البائع بيدفعه من الشحن
    public decimal CustomerShippingShare { get; set; }  // اللي العميل بيدفعه
    public decimal GrandTotal { get; set; }
    
    // الحالة
    public string Status { get; set; } = "Pending"; 
    // Pending, Confirmed, Preparing, OutForDelivery, Delivered, Cancelled, Returned
    public string PaymentMethod { get; set; } = "COD"; // COD = Cash on Delivery
    public bool IsPaid { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; set; }
    public int? ProcessedByUserId { get; set; }
    public int? LinkedInvoiceId { get; set; } // لما يحول لفاتورة فعلية
    
    public Brand Brand { get; set; } = null!;
    public ICollection<OnlineOrderItem> Items { get; set; } = new List<OnlineOrderItem>();
}

public class OnlineOrderItem
{
    public int ItemId { get; set; }
    public int OrderId { get; set; }
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    
    public OnlineOrder Order { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
