namespace MAS.Domain.Common;

/// <summary>
/// الكلاس الأساسي لكل الكيانات
/// </summary>
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
