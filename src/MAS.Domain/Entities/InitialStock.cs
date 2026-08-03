using MAS.Domain.Common;

namespace MAS.Domain.Entities;

/// <summary>
/// مخزن أول المدة - رصيد افتتاحي للمنتجات
/// (لا يؤثر على الخزنة، فقط بيضيف كميات للمخزون)
/// </summary>
public class InitialStock : BaseEntity
{
    public int InitialStockId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public int? WarehouseId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Warehouse? Warehouse { get; set; }
    public ICollection<InitialStockItem> Items { get; set; } = new List<InitialStockItem>();
}

public class InitialStockItem
{
    public int ItemId { get; set; }
    public int InitialStockId { get; set; }
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    
    public InitialStock InitialStock { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
