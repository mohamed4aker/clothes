using MAS.Domain.Common;

namespace MAS.Domain.Entities;

/// <summary>
/// قائمة مكونات المنتج (BOM - Bill of Materials)
/// الوصفة اللي بنحتاجها عشان نصنع المنتج
/// </summary>
public class BillOfMaterials : BaseEntity
{
    public int BomId { get; set; }
    public int BrandId { get; set; }
    public int ProductId { get; set; }              // المنتج النهائي
    public string BomName { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    
    // التكاليف الإضافية (مش الخامات)
    public decimal DirectLaborCost { get; set; }    // تكلفة العمالة المباشرة
    public decimal OverheadCost { get; set; }       // تكاليف غير مباشرة (كهربا، إيجار)
    public decimal AdminExpenses { get; set; }      // مصاريف إدارية
    public decimal OtherCosts { get; set; }         // مصاريف أخرى
    public string? OtherCostsDescription { get; set; }
    
    // الهالك المتوقع
    public decimal ExpectedWastePercent { get; set; }
    
    // التكلفة الكلية المحسوبة
    public decimal TotalMaterialsCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal SuggestedSellingPrice { get; set; }
    public decimal TargetMarginPercent { get; set; } = 30;
    
    // الكمية اللي تطلع من الوصفة
    public decimal OutputQuantity { get; set; } = 1;
    
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<BomItem> Items { get; set; } = new List<BomItem>();
}

/// <summary>
/// مكون من مكونات الوصفة (خامة)
/// </summary>
public class BomItem
{
    public int BomItemId { get; set; }
    public int BomId { get; set; }
    public int RawMaterialVariantId { get; set; }   // الخامة
    public decimal Quantity { get; set; }            // الكمية المطلوبة
    public int? UnitId { get; set; }                 // وحدة القياس
    public decimal UnitCost { get; set; }            // تكلفة الوحدة
    public decimal LineCost { get; set; }            // التكلفة الإجمالية
    public bool IsCritical { get; set; }
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
    
    // العلاقات
    public BillOfMaterials Bom { get; set; } = null!;
    public ProductVariant RawMaterialVariant { get; set; } = null!;
    public Unit? Unit { get; set; }
}

/// <summary>
/// أمر إنتاج
/// </summary>
public class ProductionOrder : BaseEntity
{
    public int ProductionOrderId { get; set; }
    public int BrandId { get; set; }
    public int BranchId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int BomId { get; set; }
    public int ProductId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
    public decimal WasteQuantity { get; set; }
    public string Status { get; set; } = "Draft";   // Draft, InProgress, Completed, Cancelled
    
    // التكاليف
    public decimal TotalMaterialsCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public decimal TotalOverhead { get; set; }
    public decimal OtherCosts { get; set; }
    public decimal TotalProductionCost { get; set; }
    public decimal UnitProductionCost { get; set; }
    
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? CompletedBy { get; set; }
    public string? Notes { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public BillOfMaterials Bom { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<ProductionMaterialUsage> MaterialsUsed { get; set; } = new List<ProductionMaterialUsage>();
}

/// <summary>
/// استهلاك الخامات الفعلي في الإنتاج
/// </summary>
public class ProductionMaterialUsage
{
    public int UsageId { get; set; }
    public int ProductionOrderId { get; set; }
    public int RawMaterialVariantId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
    public decimal WasteQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
    
    // العلاقات
    public ProductionOrder ProductionOrder { get; set; } = null!;
    public ProductVariant RawMaterialVariant { get; set; } = null!;
}
