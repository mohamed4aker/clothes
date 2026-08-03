using MAS.Domain.Common;
using MAS.Domain.Enums;

namespace MAS.Domain.Entities;

/// <summary>
/// تصنيف المنتجات (هرمي)
/// </summary>
public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryNameEn { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool ShowOnline { get; set; } = true;
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

/// <summary>
/// وحدات القياس
/// </summary>
public class Unit : BaseEntity
{
    public int UnitId { get; set; }
    public int BrandId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? UnitNameEn { get; set; }
    public string? UnitSymbol { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
}

/// <summary>
/// المنتجات
/// </summary>
public class Product : BaseEntity
{
    public int ProductId { get; set; }
    public int BrandId { get; set; }
    public int? CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductNameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    
    // الأسعار
    public decimal BaseCostPrice { get; set; }
    public decimal BaseSellingPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? OnlinePrice { get; set; }
    public decimal TaxRate { get; set; } = 14.00m;
    
    public int? BaseUnitId { get; set; }
    public decimal MinStock { get; set; }
    public decimal? MaxStock { get; set; }
    
    // النوع والخصائص
    public ProductType ProductType { get; set; } = ProductType.Standard;
    public bool HasVariants { get; set; }
    public bool HasExpiry { get; set; }
    public bool HasBatchTracking { get; set; }
    
    // البيع الإلكتروني
    public bool ShowOnline { get; set; }
    public bool OnlineFeatured { get; set; }
    public string? OnlineSlug { get; set; }
    
    public string? MainImageUrl { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public Category? Category { get; set; }
    public Unit? BaseUnit { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}

/// <summary>
/// صور المنتج
/// </summary>
public class ProductImage
{
    public int ImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
    
    public Product Product { get; set; } = null!;
}

/// <summary>
/// المنتجات المتنوعة (variants)
/// مثلاً: تيشيرت أحمر مقاس L
/// </summary>
public class ProductVariant : BaseEntity
{
    public int VariantId { get; set; }
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? VariantName { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? OnlinePrice { get; set; }
    public decimal? WeightGrams { get; set; }
    public string? ImageUrl { get; set; }
    
    // العلاقات
    public Product Product { get; set; } = null!;
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}

/// <summary>
/// المخزون لكل variant في كل فرع
/// </summary>
public class Inventory
{
    public int InventoryId { get; set; }
    public int BranchId { get; set; }
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public Branch Branch { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
