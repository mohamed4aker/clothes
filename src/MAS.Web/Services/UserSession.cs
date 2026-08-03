using MAS.Application.DTOs;

namespace MAS.Web.Services;

/// <summary>
/// إدارة جلسة المستخدم في الـ Blazor (محفوظ في الذاكرة طول الجلسة)
/// </summary>
public class UserSession
{
    public int UserId { get; set; }
    public int OwnerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public List<BranchDto> Branches { get; set; } = new();
    public int CurrentBranchId { get; set; }
    public bool IsAuthenticated => UserId > 0;

    public event Action? OnBrandChanged;

    public void SetUser(LoginResponse user)
    {
        UserId = user.UserId;
        OwnerId = user.OwnerId;
        FullName = user.FullName;
        Role = user.Role;
        BrandId = user.BrandId;
        BrandName = user.BrandName;
        Branches = user.Branches;
        CurrentBranchId = user.Branches.FirstOrDefault()?.BranchId ?? 0;
    }

    public void SwitchBrand(int newBrandId, string newBrandName, List<BranchDto> newBranches)
    {
        BrandId = newBrandId;
        BrandName = newBrandName;
        Branches = newBranches;
        CurrentBranchId = newBranches.FirstOrDefault()?.BranchId ?? 0;
        OnBrandChanged?.Invoke();
    }

    public bool CanAccess(string permission)
    {
        // Owner و BrandAdmin له صلاحيات كاملة
        if (Role == "Owner" || Role == "BrandAdmin") return true;
        
        // BranchManager له صلاحيات محدودة
        if (Role == "BranchManager")
        {
            return permission switch
            {
                "ViewProducts" => true,
                "ViewSales" => true,
                "ViewPOS" => true,
                "ViewCustomers" => true,
                "ViewReports" => true,
                "ManageProducts" => true,
                _ => false
            };
        }
        
        // Cashier له صلاحيات أقل
        if (Role == "Cashier")
        {
            return permission switch
            {
                "ViewPOS" => true,
                "ViewProducts" => true,
                "ViewCustomers" => true,
                _ => false
            };
        }
        
        return false;
    }

    public void Logout()
    {
        UserId = 0;
        OwnerId = 0;
        FullName = string.Empty;
        Role = string.Empty;
        BrandId = 0;
        BrandName = string.Empty;
        Branches.Clear();
        CurrentBranchId = 0;
    }
}
