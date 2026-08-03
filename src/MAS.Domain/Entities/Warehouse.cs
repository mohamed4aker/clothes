using MAS.Domain.Common;

namespace MAS.Domain.Entities;

public class Warehouse : BaseEntity
{
    public int WarehouseId { get; set; }
    public int BrandId { get; set; }
    public int? BranchId { get; set; }                  // ممكن يكون مرتبط بفرع
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseType { get; set; } = "Finished";  // Finished, RawMaterial, Mixed
    public string? Address { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public bool IsMainWarehouse { get; set; }
    public string? Notes { get; set; }
    
    public Brand Brand { get; set; } = null!;
    public Branch? Branch { get; set; }
}
