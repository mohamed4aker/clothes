using MAS.Domain.Entities;
using MAS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // لو مستخدم admin موجود بالفعل، يبقى الـ seed اتعمل خلاص
        if (await context.Users.AnyAsync(u => u.Username == "admin" || u.Email == "admin@maspos.com"))
            return;

        // كله جوا transaction واحدة: يا يتم كله يا ميتمش حاجة (مفيش نص-نص تاني)
        await using var tx = await context.Database.BeginTransactionAsync();

        // ===== المالك: لو موجود استخدمه، لو مش موجود اعمله =====
        var owner = await context.Owners.FirstOrDefaultAsync(o => o.Email == "admin@maspos.com");
        if (owner == null)
        {
            owner = new Owner
            {
                FullName = "المالك",
                Email = "admin@maspos.com",
                Phone = "01000000000",
                PasswordHash = "admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Owners.Add(owner);
            await context.SaveChangesAsync();
        }

        // ===== البراند: لو للمالك ده فيه براند استخدمه، لو لأ اعمل واحد =====
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.OwnerId == owner.OwnerId);
        if (brand == null)
        {
            brand = new Brand
            {
                OwnerId = owner.OwnerId,
                BrandName = "شركتي",
                BrandNameEn = "My Company",
                BusinessType = BusinessType.Generic,
                Phone = "01000000000",
                Email = "info@mycompany.com",
                Address = "العنوان",
                DefaultTaxRate = 0m,
                HasOnlineStore = false,
                OnlineStoreSlug = "shop"
            };
            context.Brands.Add(brand);
            await context.SaveChangesAsync();
        }

        // ===== الفرع الرئيسي: لو موجود استخدمه، لو لأ اعمله =====
        var branch = await context.Branches.FirstOrDefaultAsync(b => b.BrandId == brand.BrandId);
        if (branch == null)
        {
            branch = new Branch
            {
                BrandId = brand.BrandId,
                BranchName = "الفرع الرئيسي",
                BranchCode = "MAIN",
                Address = "العنوان",
                Phone = "01000000000",
                IsMainBranch = true
            };
            context.Branches.Add(branch);
            await context.SaveChangesAsync();
        }

        // ===== مستخدم admin (وصلنا هنا يبقى مش موجود) =====
        var adminUser = new User
        {
            BrandId = brand.BrandId,
            Username = "admin",
            FullName = "المدير",
            Email = "admin@maspos.com",
            Phone = "01000000000",
            PasswordHash = "admin",
            Role = UserRole.Owner,
            IsActive = true
        };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        // ربط المستخدم بالفرع
        context.UserBranches.Add(new UserBranch
        {
            UserId = adminUser.UserId,
            BranchId = branch.BranchId
        });
        await context.SaveChangesAsync();

        // ===== وحدات قياس أساسية (بس لو مش موجودة للبراند ده) =====
        if (!await context.Units.AnyAsync(u => u.BrandId == brand.BrandId))
        {
            var units = new List<Unit>
            {
                new() { BrandId = brand.BrandId, UnitName = "قطعة", UnitSymbol = "قطعة" },
                new() { BrandId = brand.BrandId, UnitName = "كيلو", UnitSymbol = "كجم" },
                new() { BrandId = brand.BrandId, UnitName = "جرام", UnitSymbol = "جم" },
                new() { BrandId = brand.BrandId, UnitName = "لتر", UnitSymbol = "لتر" },
                new() { BrandId = brand.BrandId, UnitName = "متر", UnitSymbol = "م" },
                new() { BrandId = brand.BrandId, UnitName = "علبة", UnitSymbol = "علبة" }
            };
            context.Units.AddRange(units);
            await context.SaveChangesAsync();
        }

        // ===== إعدادات افتراضية للبراند (بس لو مش موجودة) =====
        if (!await context.BrandSettings.AnyAsync(s => s.BrandId == brand.BrandId))
        {
            context.BrandSettings.Add(new BrandSettings
            {
                BrandId = brand.BrandId,
                ShowTaxInPOS = false,
                ShowProfitInPOS = false,
                ShowProductImages = true,
                HideZeroStockProducts = true,
                PrimaryColor = "#92400e",
                ReceiptHeader = "شركتي",
                ReceiptFooter = "شكراً لتعاملكم معنا"
            });
            await context.SaveChangesAsync();
        }

        await tx.CommitAsync();
    }
}
