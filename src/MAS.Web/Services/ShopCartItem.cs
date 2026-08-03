namespace MAS.Web.Services;

public class ShopCartItem
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal MaxStock { get; set; }
}
