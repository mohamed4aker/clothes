using MAS.Domain.Common;
using MAS.Domain.Enums;

namespace MAS.Domain.Entities;

/// <summary>
/// مستخدمي النظام
/// </summary>
public class User : BaseEntity
{
    public int UserId { get; set; }
    public int BrandId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? PinCode { get; set; }
    public string? AvatarUrl { get; set; }
    public bool Has2FA { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}

/// <summary>
/// ربط المستخدم بالفروع (يوزر واحد ممكن يكون له أكتر من فرع)
/// </summary>
public class UserBranch
{
    public int UserBranchId { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public User User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
