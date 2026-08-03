using MAS.Application.Interfaces;
using MAS.Infrastructure.Data;
using MAS.Infrastructure.Services;
using MAS.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// تحديد البورتات بشكل صريح للـ Web (في التطوير فقط —
// على السيرفر/IIS البورتات بيحددها الاستضافة نفسها أو ASPNETCORE_URLS)
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5002", "https://localhost:7000");
}

// تسجيل الأخطاء في ملف logs/errors.log عشان نعرف سبب أي مشكلة على السيرفر
builder.Logging.AddProvider(new MAS.Web.Services.FileLoggerProvider(
    Path.Combine(AppContext.BaseDirectory, "logs")));

// ===== Database =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Transient);

// ===== Services =====
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<ISalesService, SalesService>();
builder.Services.AddTransient<IDashboardService, DashboardService>();
builder.Services.AddTransient<IBranchService, BranchService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IUnitService, UnitService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<ISupplierService, SupplierService>();
builder.Services.AddTransient<IWarehouseService, WarehouseService>();
builder.Services.AddTransient<IInitialStockService, InitialStockService>();
builder.Services.AddTransient<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddTransient<IStockTransferService, StockTransferService>();
builder.Services.AddTransient<IManufacturingService, ManufacturingService>();
builder.Services.AddTransient<IExpenseService, ExpenseService>();
builder.Services.AddTransient<IPurchaseService, PurchaseService>();
builder.Services.AddTransient<IBrandSettingsService, BrandSettingsService>();
builder.Services.AddTransient<IAdvancedDashboardService, AdvancedDashboardService>();
builder.Services.AddTransient<ISalesReturnService, SalesReturnService>();
builder.Services.AddTransient<IStockTakeService, StockTakeService>();
builder.Services.AddTransient<ICashierSessionService, CashierSessionService>();
builder.Services.AddTransient<IPromotionService, PromotionService>();
builder.Services.AddTransient<IReportsService, ReportsService>();
builder.Services.AddTransient<IOnlineOrderService, OnlineOrderService>();
builder.Services.AddScoped<UserSession>();

// ===== Authentication (Cookie-based for Web) =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

// ===== Blazor + MudBlazor =====
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(o => o.DetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ===== تجهيز الداتابيز والبيانات الأولية =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"خطأ في تجهيز قاعدة البيانات: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
