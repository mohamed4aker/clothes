using MAS.Domain.Common;
using MAS.Domain.Enums;

namespace MAS.Domain.Entities;

/// <summary>
/// المالك - عميل النظام (لو SaaS) أو صاحب الشركة
/// </summary>
public class Owner : BaseEntity
{
    public int OwnerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string SubscriptionPlan { get; set; } = "Trial";
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionExpiry { get; set; }
    
    // الحدود (الحد الأقصى 10 يوزر)
    public int MaxBrands { get; set; } = 3;
    public int MaxBranches { get; set; } = 10;
    public int MaxUsers { get; set; } = 10;
    
    public DateTime? LastLoginAt { get; set; }
    
    // العلاقات
    public ICollection<Brand> Brands { get; set; } = new List<Brand>();
}

/// <summary>
/// البراند - الكيان التجاري (محل ملابس، صيدلية، إلخ)
/// </summary>
public class Brand : BaseEntity
{
    public int BrandId { get; set; }
    public int OwnerId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? BrandNameEn { get; set; }
    public BusinessType BusinessType { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }
    public string Currency { get; set; } = "EGP";
    public decimal DefaultTaxRate { get; set; } = 14.00m;
    
    // البيع الإلكتروني
    public bool HasOnlineStore { get; set; }
    public string? OnlineStoreSlug { get; set; }
    public string? OnlineStoreDomain { get; set; }
    
    // الإعدادات
    public bool AllowNegativeStock { get; set; }
    public string DefaultLanguage { get; set; } = "ar";
    public int FiscalYearStartMonth { get; set; } = 1;
    
    // العلاقات
    public Owner Owner { get; set; } = null!;
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}

/// <summary>
/// الفرع - فرع من فروع البراند
/// </summary>
public class Branch : BaseEntity
{
    public int BranchId { get; set; }
    public int BrandId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsWarehouse { get; set; }
    public bool AllowOnlineFulfillment { get; set; } = true;
    
    // العلاقات
    public Brand Brand { get; set; } = null!;
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}
