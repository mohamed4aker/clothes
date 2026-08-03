using MAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CashierSession> CashierSessions => Set<CashierSession>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    
    // التصنيع
    public DbSet<BillOfMaterials> BillOfMaterials => Set<BillOfMaterials>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionMaterialUsage> ProductionMaterialUsages => Set<ProductionMaterialUsage>();

    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<BrandSettings> BrandSettings => Set<BrandSettings>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
    public DbSet<StockTake> StockTakes => Set<StockTake>();
    public DbSet<StockTakeItem> StockTakeItems => Set<StockTakeItem>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<OnlineOrder> OnlineOrders => Set<OnlineOrder>();
    public DbSet<OnlineOrderItem> OnlineOrderItems => Set<OnlineOrderItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InitialStock> InitialStocks => Set<InitialStock>();
    public DbSet<InitialStockItem> InitialStockItems => Set<InitialStockItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
        {
            if (fk.DeleteBehavior == DeleteBehavior.Cascade)
                fk.DeleteBehavior = DeleteBehavior.NoAction;
        }

        modelBuilder.Entity<Owner>(e =>
        {
            e.HasKey(x => x.OwnerId);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.SubscriptionPlan).HasMaxLength(50);
        });

        modelBuilder.Entity<Brand>(e =>
        {
            e.HasKey(x => x.BrandId);
            e.Property(x => x.BrandName).HasMaxLength(200).IsRequired();
            e.Property(x => x.BusinessType).HasConversion<string>();
            e.Property(x => x.DefaultTaxRate).HasColumnType("decimal(5,2)");
            e.HasOne(x => x.Owner).WithMany(x => x.Brands).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.OnlineStoreSlug).IsUnique().HasFilter("[OnlineStoreSlug] IS NOT NULL");
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.BranchId);
            e.Property(x => x.BranchName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
            e.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
            e.HasOne(x => x.Brand).WithMany(x => x.Branches).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasConversion<string>();
            e.HasOne(x => x.Brand).WithMany(x => x.Users).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.BrandId, x.Username }).IsUnique();
        });

        modelBuilder.Entity<UserBranch>(e =>
        {
            e.HasKey(x => x.UserBranchId);
            e.HasOne(x => x.User).WithMany(x => x.UserBranches).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany(x => x.UserBranches).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.UserId, x.BranchId }).IsUnique();
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryName).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Brand).WithMany(x => x.Categories).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.ParentCategory).WithMany(x => x.SubCategories).HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Unit>(e =>
        {
            e.HasKey(x => x.UnitId);
            e.Property(x => x.UnitName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.ProductId);
            e.Property(x => x.SKU).HasMaxLength(50).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
            e.Property(x => x.BaseCostPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.BaseSellingPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.WholesalePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.OnlinePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.TaxRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.MinStock).HasColumnType("decimal(18,3)");
            e.Property(x => x.MaxStock).HasColumnType("decimal(18,3)");
            e.Property(x => x.ProductType).HasConversion<string>();
            e.HasOne(x => x.Brand).WithMany(x => x.Products).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.BaseUnit).WithMany().HasForeignKey(x => x.BaseUnitId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BrandId, x.SKU }).IsUnique();
            e.HasIndex(x => x.Barcode);
        });

        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasKey(x => x.ImageId);
            e.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.HasKey(x => x.VariantId);
            e.Property(x => x.SKU).HasMaxLength(50).IsRequired();
            e.Property(x => x.CostPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.SellingPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.OnlinePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.WeightGrams).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Product).WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.SKU).IsUnique();
            e.HasIndex(x => x.Barcode);
        });

        modelBuilder.Entity<Inventory>(e =>
        {
            e.HasKey(x => x.InventoryId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.ReservedQuantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.AverageCost).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Branch).WithMany(x => x.Inventory).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Variant).WithMany(x => x.Inventory).HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.BranchId, x.VariantId }).IsUnique();
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.CustomerId);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.TotalSpent).HasColumnType("decimal(18,2)");
            e.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.CreditLimit).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.Phone);
            e.HasIndex(x => x.Email);
        });

        modelBuilder.Entity<CashierSession>(e =>
        {
            e.HasKey(x => x.SessionId);
            e.Property(x => x.OpeningBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.ClosingBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.ExpectedBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.Difference).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalSales).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalReturns).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalCash).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalCard).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalWallet).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SalesInvoice>(e =>
        {
            e.HasKey(x => x.InvoiceId);
            e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.PromotionDiscount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ShippingCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ChangeAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CashierUser).WithMany().HasForeignKey(x => x.CashierUserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Session).WithMany(x => x.Invoices).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BrandId, x.InvoiceNumber }).IsUnique();
        });

        modelBuilder.Entity<SalesInvoiceItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PromotionDiscount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TaxRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ConversionFactor).HasColumnType("decimal(18,4)");
            e.HasOne(x => x.Invoice).WithMany(x => x.Items).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<InvoicePayment>(e =>
        {
            e.HasKey(x => x.PaymentId);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaymentMethod).HasConversion<string>();
            e.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.HasKey(x => x.MovementId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.BalanceAfter).HasColumnType("decimal(18,3)");
            e.Property(x => x.MovementType).HasConversion<string>();
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.BranchId, x.VariantId });
            e.HasIndex(x => x.CreatedAt);
        });

        // ========== Manufacturing ==========
        modelBuilder.Entity<BillOfMaterials>(e =>
        {
            e.HasKey(x => x.BomId);
            e.Property(x => x.BomName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Version).HasMaxLength(20);
            e.Property(x => x.DirectLaborCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.OverheadCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.AdminExpenses).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtherCosts).HasColumnType("decimal(18,2)");
            e.Property(x => x.ExpectedWastePercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.TotalMaterialsCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.SuggestedSellingPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.TargetMarginPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.OutputQuantity).HasColumnType("decimal(18,3)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<BomItem>(e =>
        {
            e.HasKey(x => x.BomItemId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineCost).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Bom).WithMany(x => x.Items).HasForeignKey(x => x.BomId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RawMaterialVariant).WithMany().HasForeignKey(x => x.RawMaterialVariantId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductionOrder>(e =>
        {
            e.HasKey(x => x.ProductionOrderId);
            e.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.PlannedQuantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.ActualQuantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.WasteQuantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.TotalMaterialsCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalLaborCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalOverhead).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtherCosts).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalProductionCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.UnitProductionCost).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Bom).WithMany().HasForeignKey(x => x.BomId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.BrandId, x.OrderNumber }).IsUnique();
        });

        modelBuilder.Entity<ProductionMaterialUsage>(e =>
        {
            e.HasKey(x => x.UsageId);
            e.Property(x => x.PlannedQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.ActualQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.WasteQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.ProductionOrder).WithMany(x => x.MaterialsUsed).HasForeignKey(x => x.ProductionOrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RawMaterialVariant).WithMany().HasForeignKey(x => x.RawMaterialVariantId).OnDelete(DeleteBehavior.NoAction);
        });

        // ========== Operations ==========
        modelBuilder.Entity<Expense>(e =>
        {
            e.HasKey(x => x.ExpenseId);
            e.Property(x => x.Category).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaymentMethod).HasMaxLength(50);
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PurchaseInvoice>(e =>
        {
            e.HasKey(x => x.PurchaseInvoiceId);
            e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.SupplierName).HasMaxLength(200);
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ShippingCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PurchaseType).HasMaxLength(30);
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BrandId, x.InvoiceNumber }).IsUnique();
        });

        modelBuilder.Entity<PurchaseInvoiceItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Invoice).WithMany(x => x.Items).HasForeignKey(x => x.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<BrandSettings>(e =>
        {
            e.HasKey(x => x.BrandSettingsId);
            e.Property(x => x.PrimaryColor).HasMaxLength(10);
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.BrandId).IsUnique();
        });

        modelBuilder.Entity<SalesReturn>(e =>
        {
            e.HasKey(x => x.ReturnId);
            e.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.RefundMethod).HasMaxLength(50);
            e.Property(x => x.Reason).HasMaxLength(500);
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.OriginalInvoice).WithMany().HasForeignKey(x => x.OriginalInvoiceId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CashierUser).WithMany().HasForeignKey(x => x.CashierUserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SalesReturnItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.SalesReturn).WithMany(x => x.Items).HasForeignKey(x => x.ReturnId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<StockTake>(e =>
        {
            e.HasKey(x => x.StockTakeId);
            e.Property(x => x.StockTakeNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.TotalDifferenceValue).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<StockTakeItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.SystemQuantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.CountedQuantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.Difference).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.DifferenceValue).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.StockTake).WithMany(x => x.Items).HasForeignKey(x => x.StockTakeId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Promotion>(e =>
        {
            e.HasKey(x => x.PromotionId);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CouponCode).HasMaxLength(50);
            e.Property(x => x.DiscountType).HasMaxLength(20);
            e.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            e.Property(x => x.MinPurchaseAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.MaxDiscountAmount).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.CouponCode);
        });

        modelBuilder.Entity<OnlineOrder>(e =>
        {
            e.HasKey(x => x.OrderId);
            e.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.CustomerName).HasMaxLength(200);
            e.Property(x => x.CustomerPhone).HasMaxLength(50);
            e.Property(x => x.ShippingAddress).HasMaxLength(500);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(30);
            e.Property(x => x.PaymentMethod).HasMaxLength(30);
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.ShippingCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.SellerShippingShare).HasColumnType("decimal(18,2)");
            e.Property(x => x.CustomerShippingShare).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasIndex(x => x.CustomerPhone);
        });

        modelBuilder.Entity<OnlineOrderItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.ProductName).HasMaxLength(300);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasKey(x => x.SupplierId);
            e.Property(x => x.SupplierName).HasMaxLength(200).IsRequired();
            e.Property(x => x.CompanyName).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.TaxNumber).HasMaxLength(50);
            e.Property(x => x.PaymentTerms).HasMaxLength(100);
            e.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalPurchases).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.Phone);
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.HasKey(x => x.WarehouseId);
            e.Property(x => x.WarehouseName).HasMaxLength(200).IsRequired();
            e.Property(x => x.WarehouseCode).HasMaxLength(50);
            e.Property(x => x.WarehouseType).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.ManagerName).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BrandId, x.WarehouseCode }).IsUnique();
        });

        modelBuilder.Entity<InitialStock>(e =>
        {
            e.HasKey(x => x.InitialStockId);
            e.Property(x => x.EntryNumber).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BrandId, x.EntryNumber }).IsUnique();
        });

        modelBuilder.Entity<InitialStockItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.InitialStock).WithMany(x => x.Items).HasForeignKey(x => x.InitialStockId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PurchaseReturn>(e =>
        {
            e.HasKey(x => x.PurchaseReturnId);
            e.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.RefundMethod).HasMaxLength(20);
            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.PurchaseInvoice).WithMany().HasForeignKey(x => x.PurchaseInvoiceId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.BrandId, x.ReturnNumber }).IsUnique();
        });

        modelBuilder.Entity<PurchaseReturnItem>(e =>
        {
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.PurchaseReturn).WithMany(x => x.Items).HasForeignKey(x => x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}