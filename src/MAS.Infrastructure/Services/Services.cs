using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

// ============================================================
// AuthService
// ============================================================
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    public AuthService(AppDbContext context) => _context = context;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Brand)
            .Include(u => u.UserBranches).ThenInclude(ub => ub.Branch)
            .FirstOrDefaultAsync(u => (u.Email == request.Email || u.Username == request.Email) && u.IsActive);

        if (user == null) return null;
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow) return null;

        if (request.Password != user.PasswordHash)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();
            return null;
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            UserId = user.UserId,
            OwnerId = user.Brand.OwnerId,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            BrandId = user.BrandId,
            BrandName = user.Brand.BrandName,
            Branches = user.UserBranches.Select(ub => new BranchDto
            {
                BranchId = ub.Branch.BranchId,
                BranchName = ub.Branch.BranchName,
                BranchCode = ub.Branch.BranchCode,
                IsMainBranch = ub.Branch.IsMainBranch,
                IsActive = ub.Branch.IsActive
            }).ToList()
        };
    }

    public async Task<bool> ValidateUserAsync(int userId) =>
        await _context.Users.AnyAsync(u => u.UserId == userId && u.IsActive);
}

// ============================================================
// ProductService
// ============================================================
public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    public ProductService(AppDbContext context) => _context = context;

    public async Task<List<ProductDto>> GetProductsAsync(int brandId)
    {
        return await _context.Products
            .Where(p => p.BrandId == brandId && p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Inventory)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                SKU = p.SKU,
                Barcode = p.Barcode,
                ProductName = p.ProductName,
                Description = p.Description,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                BaseCostPrice = p.BaseCostPrice,
                BaseSellingPrice = p.BaseSellingPrice,
                OnlinePrice = p.OnlinePrice,
                TaxRate = p.TaxRate,
                MinStock = p.MinStock,
                HasVariants = p.HasVariants,
                ShowOnline = p.ShowOnline,
                Status = p.Status,
                MainImageUrl = p.MainImageUrl,
                Variants = p.Variants.Select(v => new ProductVariantDto
                {
                    VariantId = v.VariantId,
                    SKU = v.SKU,
                    Barcode = v.Barcode,
                    VariantName = v.VariantName,
                    CostPrice = v.CostPrice,
                    SellingPrice = v.SellingPrice ?? p.BaseSellingPrice,
                    TotalStock = v.Inventory.Sum(i => i.Quantity)
                }).ToList()
            }).ToListAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId, int brandId)
    {
        var products = await GetProductsAsync(brandId);
        return products.FirstOrDefault(p => p.ProductId == productId);
    }

    public async Task<ProductDto?> GetByBarcodeAsync(string barcode, int brandId)
    {
        // البحث في Barcode للـ Variant أو الـ Product أو SKU
        var variant = await _context.ProductVariants
            .Include(v => v.Product).ThenInclude(p => p!.Category)
            .Include(v => v.Inventory)
            .FirstOrDefaultAsync(v => v.Product!.BrandId == brandId && (
                v.Barcode == barcode || 
                v.SKU == barcode ||
                v.Product.Barcode == barcode ||
                v.Product.SKU == barcode));

        if (variant != null)
        {
            return new ProductDto
            {
                ProductId = variant.Product!.ProductId,
                SKU = variant.Product.SKU,
                Barcode = variant.Product.Barcode,
                ProductName = variant.Product.ProductName,
                BaseSellingPrice = variant.Product.BaseSellingPrice,
                Variants = new List<ProductVariantDto>
                {
                    new()
                    {
                        VariantId = variant.VariantId,
                        SKU = variant.SKU,
                        Barcode = variant.Barcode,
                        VariantName = variant.VariantName,
                        SellingPrice = variant.SellingPrice ?? variant.Product.BaseSellingPrice,
                        TotalStock = variant.Inventory.Sum(i => i.Quantity)
                    }
                }
            };
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode && p.BrandId == brandId);
        return product != null ? await GetProductByIdAsync(product.ProductId, brandId) : null;
    }

    public async Task<int> CreateProductAsync(int brandId, int userId, CreateProductRequest request)
    {
        var product = new Product
        {
            BrandId = brandId,
            CategoryId = request.CategoryId,
            SKU = request.SKU,
            Barcode = request.Barcode,
            ProductName = request.ProductName,
            Description = request.Description,
            BaseCostPrice = request.BaseCostPrice,
            BaseSellingPrice = request.BaseSellingPrice,
            OnlinePrice = request.OnlinePrice,
            TaxRate = request.TaxRate,
            BaseUnitId = request.BaseUnitId,
            MinStock = request.MinStock,
            ShowOnline = request.ShowOnline,
            CreatedBy = userId,
            Status = "Active"
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.ProductId,
            SKU = product.SKU,
            Barcode = product.Barcode,
            VariantName = product.ProductName,
            CostPrice = product.BaseCostPrice,
            SellingPrice = product.BaseSellingPrice
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        return product.ProductId;
    }

    public async Task<bool> UpdateProductAsync(int productId, int brandId, CreateProductRequest request)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId && p.BrandId == brandId);
        if (product == null) return false;

        product.CategoryId = request.CategoryId;
        product.SKU = request.SKU;
        product.Barcode = request.Barcode;
        product.ProductName = request.ProductName;
        product.Description = request.Description;
        product.BaseCostPrice = request.BaseCostPrice;
        product.BaseSellingPrice = request.BaseSellingPrice;
        product.OnlinePrice = request.OnlinePrice;
        product.TaxRate = request.TaxRate;
        product.MinStock = request.MinStock;
        product.ShowOnline = request.ShowOnline;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int productId, int brandId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId && p.BrandId == brandId);
        if (product == null) return false;
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> AddVariantAsync(int productId, int brandId, CreateVariantRequest request)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId && p.BrandId == brandId);
        if (product == null) throw new Exception("المنتج غير موجود");

        var variant = new ProductVariant
        {
            ProductId = productId,
            SKU = request.SKU,
            Barcode = request.Barcode,
            VariantName = request.VariantName,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        // إضافة المخزون الأولي
        if (request.InitialStock > 0 && request.BranchId > 0)
        {
            _context.Inventory.Add(new Inventory
            {
                BranchId = request.BranchId,
                VariantId = variant.VariantId,
                Quantity = request.InitialStock,
                AverageCost = request.CostPrice ?? 0
            });
            await _context.SaveChangesAsync();
        }

        // تحديث: المنتج له variants
        if (!product.HasVariants)
        {
            product.HasVariants = true;
            await _context.SaveChangesAsync();
        }

        return variant.VariantId;
    }

    public async Task<bool> UpdateVariantAsync(int variantId, int brandId, CreateVariantRequest request)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.VariantId == variantId && v.Product!.BrandId == brandId);
        if (variant == null) return false;

        variant.SKU = request.SKU;
        variant.Barcode = request.Barcode;
        variant.VariantName = request.VariantName;
        variant.CostPrice = request.CostPrice;
        variant.SellingPrice = request.SellingPrice;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVariantAsync(int variantId, int brandId)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.VariantId == variantId && v.Product!.BrandId == brandId);
        if (variant == null) return false;
        variant.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AdjustStockAsync(int variantId, int branchId, decimal newQuantity, string reason, int userId)
    {
        var inv = await _context.Inventory.FirstOrDefaultAsync(i => i.VariantId == variantId && i.BranchId == branchId);
        decimal oldQuantity = 0;

        if (inv == null)
        {
            inv = new Inventory { BranchId = branchId, VariantId = variantId, Quantity = newQuantity };
            _context.Inventory.Add(inv);
        }
        else
        {
            oldQuantity = inv.Quantity;
            inv.Quantity = newQuantity;
            inv.LastUpdated = DateTime.UtcNow;
        }

        _context.StockMovements.Add(new StockMovement
        {
            BranchId = branchId,
            VariantId = variantId,
            MovementType = StockMovementType.Adjustment,
            Quantity = newQuantity - oldQuantity,
            BalanceAfter = newQuantity,
            Notes = reason,
            UserId = userId
        });

        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// SalesService
// ============================================================
public class SalesService : ISalesService
{
    public async Task<decimal> GetTotalSalesAsync(int brandId, DateTime from, DateTime to)
    {
        return await _context.SalesInvoices
            .Where(i => i.BrandId == brandId && i.CreatedAt >= from && i.CreatedAt <= to && i.Status == InvoiceStatus.Completed)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;
    }

    public async Task<List<SaleSummaryDto>> GetSalesSummaryAsync(int brandId, DateTime from, DateTime to)
    {
        return await _context.SalesInvoices
            .Where(i => i.BrandId == brandId && i.CreatedAt >= from && i.CreatedAt <= to)
            .Include(i => i.Customer)
            .Include(i => i.CashierUser)
            .Include(i => i.Branch)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new SaleSummaryDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.CreatedAt,
                CustomerName = i.Customer != null ? i.Customer.FullName : null,
                CashierName = i.CashierUser != null ? i.CashierUser.FullName : "",
                BranchName = i.Branch.BranchName,
                SubTotal = i.SubTotal,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                GrandTotal = i.GrandTotal,
                PaidAmount = i.PaidAmount,
                PaymentMethod = i.Payments.Any() ? i.Payments.First().PaymentMethod.ToString() : "Cash",
                Status = i.Status.ToString(),
                ItemsCount = i.Items.Count
            }).ToListAsync();
    }

    private readonly AppDbContext _context;
    public SalesService(AppDbContext context) => _context = context;

    public async Task<SaleResponse> CreateSaleAsync(int brandId, int userId, CreateSaleRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var lastInvoice = await _context.SalesInvoices
                .Where(i => i.BrandId == brandId)
                .OrderByDescending(i => i.InvoiceId)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            var invoiceNumber = GenerateInvoiceNumber(lastInvoice);

            decimal subTotal = 0, taxAmount = 0;
            var invoiceItems = new List<SalesInvoiceItem>();

            foreach (var item in request.Items)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .Include(v => v.Inventory)
                    .FirstOrDefaultAsync(v => v.VariantId == item.VariantId);

                if (variant == null) throw new Exception($"المنتج غير موجود");

                var inv = variant.Inventory.FirstOrDefault(i => i.BranchId == request.BranchId);
                if (inv == null || inv.Quantity < item.Quantity)
                    throw new Exception($"المخزون غير كاف للمنتج {variant.VariantName}");

                var lineTotal = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
                var taxRate = variant.Product!.TaxRate;
                var lineTax = lineTotal * (taxRate / 100);

                subTotal += lineTotal;
                taxAmount += lineTax;

                invoiceItems.Add(new SalesInvoiceItem
                {
                    VariantId = variant.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = inv.AverageCost,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    TaxRate = taxRate,
                    TaxAmount = lineTax,
                    LineTotal = lineTotal + lineTax
                });

                inv.Quantity -= item.Quantity;
                inv.LastUpdated = DateTime.UtcNow;

                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = request.BranchId,
                    VariantId = variant.VariantId,
                    MovementType = StockMovementType.Sale,
                    Quantity = -item.Quantity,
                    UnitCost = inv.AverageCost,
                    UnitPrice = item.UnitPrice,
                    BalanceAfter = inv.Quantity,
                    ReferenceType = "SalesInvoice",
                    Notes = $"بيع - فاتورة {invoiceNumber}",
                    UserId = userId
                });
            }

            var grandTotal = subTotal + taxAmount - request.DiscountAmount;
            var paidAmount = request.Payments.Sum(p => p.Amount);
            var changeAmount = paidAmount > grandTotal ? paidAmount - grandTotal : 0;

            var invoice = new SalesInvoice
            {
                BrandId = brandId,
                BranchId = request.BranchId,
                InvoiceNumber = invoiceNumber,
                CustomerId = request.CustomerId,
                CashierUserId = userId,
                SubTotal = subTotal,
                DiscountAmount = request.DiscountAmount,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                PaidAmount = paidAmount,
                ChangeAmount = changeAmount,
                Status = InvoiceStatus.Completed,
                Notes = request.Notes,
                Items = invoiceItems,
                Payments = request.Payments.Select(p => new InvoicePayment
                {
                    PaymentMethod = Enum.Parse<PaymentMethod>(p.PaymentMethod),
                    Amount = p.Amount,
                    ReferenceNumber = p.ReferenceNumber
                }).ToList()
            };

            _context.SalesInvoices.Add(invoice);

            // تحديث بيانات العميل لو موجود
            if (request.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
                if (customer != null)
                {
                    customer.TotalSpent += grandTotal;
                    customer.LastPurchaseAt = DateTime.UtcNow;
                    customer.LoyaltyPoints += (int)(grandTotal / 10);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new SaleResponse
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                SubTotal = invoice.SubTotal,
                DiscountAmount = invoice.DiscountAmount,
                TaxAmount = invoice.TaxAmount,
                GrandTotal = invoice.GrandTotal,
                PaidAmount = invoice.PaidAmount,
                ChangeAmount = invoice.ChangeAmount,
                CreatedAt = invoice.CreatedAt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<SaleResponse>> GetRecentSalesAsync(int brandId, int branchId, int count = 20)
    {
        return await _context.SalesInvoices
            .Where(i => i.BrandId == brandId && i.BranchId == branchId)
            .OrderByDescending(i => i.CreatedAt).Take(count)
            .Select(i => new SaleResponse
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                SubTotal = i.SubTotal,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                GrandTotal = i.GrandTotal,
                PaidAmount = i.PaidAmount,
                ChangeAmount = i.ChangeAmount,
                CreatedAt = i.CreatedAt
            }).ToListAsync();
    }

    public async Task<SaleResponse?> GetSaleByIdAsync(int invoiceId, int brandId)
    {
        return await _context.SalesInvoices
            .Where(i => i.InvoiceId == invoiceId && i.BrandId == brandId)
            .Select(i => new SaleResponse
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                SubTotal = i.SubTotal,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                GrandTotal = i.GrandTotal,
                PaidAmount = i.PaidAmount,
                ChangeAmount = i.ChangeAmount,
                CreatedAt = i.CreatedAt
            }).FirstOrDefaultAsync();
    }

    public async Task<SaleDetailDto?> GetSaleDetailsAsync(int invoiceId, int brandId)
    {
        var invoice = await _context.SalesInvoices
            .Where(i => i.InvoiceId == invoiceId && i.BrandId == brandId)
            .Include(i => i.Branch)
            .Include(i => i.Customer)
            .Include(i => i.CashierUser)
            .Include(i => i.Items).ThenInclude(it => it.Variant).ThenInclude(v => v.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync();

        if (invoice == null) return null;

        return new SaleDetailDto
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerName = invoice.Customer?.FullName,
            CashierName = invoice.CashierUser.FullName,
            BranchName = invoice.Branch.BranchName,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            GrandTotal = invoice.GrandTotal,
            PaidAmount = invoice.PaidAmount,
            ChangeAmount = invoice.ChangeAmount,
            CreatedAt = invoice.CreatedAt,
            Status = invoice.Status.ToString(),
            Items = invoice.Items.Select(it => new SaleItemDetailDto
            {
                VariantId = it.VariantId,
                ProductName = it.Variant.VariantName ?? it.Variant.Product!.ProductName,
                SKU = it.Variant.SKU,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                DiscountAmount = it.DiscountAmount,
                TaxAmount = it.TaxAmount,
                LineTotal = it.LineTotal
            }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentDetailDto
            {
                PaymentMethod = p.PaymentMethod.ToString(),
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber
            }).ToList()
        };
    }

    public async Task<bool> CreateReturnAsync(int brandId, int userId, CreateReturnRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var originalInvoice = await _context.SalesInvoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == request.OriginalInvoiceId && i.BrandId == brandId);
            if (originalInvoice == null) return false;

            decimal totalRefund = 0;

            foreach (var item in request.Items)
            {
                var lineTotal = item.UnitPrice * item.Quantity;
                totalRefund += lineTotal;

                if (item.BackToStock)
                {
                    var inv = await _context.Inventory
                        .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == originalInvoice.BranchId);
                    if (inv != null)
                    {
                        inv.Quantity += item.Quantity;
                        inv.LastUpdated = DateTime.UtcNow;

                        _context.StockMovements.Add(new StockMovement
                        {
                            BranchId = originalInvoice.BranchId,
                            VariantId = item.VariantId,
                            MovementType = StockMovementType.Return,
                            Quantity = item.Quantity,
                            BalanceAfter = inv.Quantity,
                            ReferenceType = "Return",
                            ReferenceId = originalInvoice.InvoiceId,
                            Notes = $"مرتجع: {request.Reason}",
                            UserId = userId
                        });
                    }
                }
            }

            originalInvoice.Status = InvoiceStatus.PartiallyRefunded;
            originalInvoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    private string GenerateInvoiceNumber(string? lastInvoiceNumber)
    {
        var prefix = $"INV-{DateTime.UtcNow:yyyyMM}-";
        if (string.IsNullOrEmpty(lastInvoiceNumber) || !lastInvoiceNumber.StartsWith(prefix))
            return $"{prefix}00001";

        var lastNumber = int.Parse(lastInvoiceNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D5}";
    }
}

// ============================================================
// DashboardService
// ============================================================
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    public DashboardService(AppDbContext context) => _context = context;

    public async Task<DashboardStatsDto> GetStatsAsync(int brandId, int? branchId = null)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        return await GetStatsForPeriodAsync(brandId, monthStart, today.AddDays(1), branchId);
    }

    public async Task<DashboardStatsDto> GetStatsForPeriodAsync(int brandId, DateTime from, DateTime to, int? branchId = null)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var query = _context.SalesInvoices.Where(i => i.BrandId == brandId);
        if (branchId.HasValue) query = query.Where(i => i.BranchId == branchId.Value);

        var todayInvoices = query.Where(i => i.CreatedAt >= today);
        var monthInvoices = query.Where(i => i.CreatedAt >= monthStart);
        var periodInvoices = query.Where(i => i.CreatedAt >= from && i.CreatedAt <= to);

        var stats = new DashboardStatsDto
        {
            TodaySales = await todayInvoices.SumAsync(i => (decimal?)i.GrandTotal) ?? 0,
            TodayInvoiceCount = await todayInvoices.CountAsync(),
            MonthSales = await monthInvoices.SumAsync(i => (decimal?)i.GrandTotal) ?? 0,
            PeriodSales = await periodInvoices.SumAsync(i => (decimal?)i.GrandTotal) ?? 0,
            PeriodInvoiceCount = await periodInvoices.CountAsync(),
            TotalProducts = await _context.Products.CountAsync(p => p.BrandId == brandId && p.IsActive),
            TotalCustomers = await _context.Customers.CountAsync(c => c.BrandId == brandId && c.IsActive)
        };

        stats.LowStockCount = await _context.Inventory
            .Where(i => i.Variant.Product.BrandId == brandId && i.Quantity <= i.Variant.Product.MinStock)
            .CountAsync();

        // أكتر 5 منتجات مبيعاً
        stats.TopProducts = await _context.SalesInvoiceItems
            .Where(it => it.Invoice.BrandId == brandId && it.Invoice.CreatedAt >= from && it.Invoice.CreatedAt <= to)
            .GroupBy(it => new { it.VariantId, ProductName = it.Variant.VariantName ?? it.Variant.Product!.ProductName })
            .Select(g => new TopProductDto
            {
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync();

        return stats;
    }
}

// ============================================================
// BranchService
// ============================================================
public class BranchService : IBranchService
{
    private readonly AppDbContext _context;
    public BranchService(AppDbContext context) => _context = context;

    public async Task<List<BranchDto>> GetBranchesAsync(int brandId)
    {
        return await _context.Branches
            .Where(b => b.BrandId == brandId && b.IsActive)
            .Select(b => new BranchDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                BranchCode = b.BranchCode,
                Phone = b.Phone,
                Address = b.Address,
                City = b.City,
                Governorate = b.Governorate,
                IsMainBranch = b.IsMainBranch,
                IsActive = b.IsActive
            }).ToListAsync();
    }

    public async Task<BranchDto?> GetBranchByIdAsync(int branchId, int brandId)
    {
        return await _context.Branches
            .Where(b => b.BranchId == branchId && b.BrandId == brandId)
            .Select(b => new BranchDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                BranchCode = b.BranchCode,
                Phone = b.Phone,
                Address = b.Address,
                City = b.City,
                Governorate = b.Governorate,
                IsMainBranch = b.IsMainBranch,
                IsActive = b.IsActive
            }).FirstOrDefaultAsync();
    }

    public async Task<int> CreateBranchAsync(int brandId, CreateBranchRequest request)
    {
        var branch = new Branch
        {
            BrandId = brandId,
            BranchName = request.BranchName,
            BranchCode = request.BranchCode,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            Governorate = request.Governorate,
            IsMainBranch = request.IsMainBranch
        };
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return branch.BranchId;
    }

    public async Task<bool> UpdateBranchAsync(int branchId, int brandId, CreateBranchRequest request)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId && b.BrandId == brandId);
        if (branch == null) return false;
        branch.BranchName = request.BranchName;
        branch.BranchCode = request.BranchCode;
        branch.Phone = request.Phone;
        branch.Address = request.Address;
        branch.City = request.City;
        branch.Governorate = request.Governorate;
        branch.IsMainBranch = request.IsMainBranch;
        branch.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBranchAsync(int branchId, int brandId)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId && b.BrandId == brandId);
        if (branch == null) return false;
        branch.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// UserService
// ============================================================
public class UserService : IUserService
{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context) => _context = context;

    public async Task<List<UserDto>> GetUsersAsync(int brandId)
    {
        return await _context.Users
            .Where(u => u.BrandId == brandId)
            .Include(u => u.UserBranches)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                BranchIds = u.UserBranches.Select(ub => ub.BranchId).ToList()
            }).ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, int brandId)
    {
        return await _context.Users
            .Where(u => u.UserId == userId && u.BrandId == brandId)
            .Include(u => u.UserBranches)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                BranchIds = u.UserBranches.Select(ub => ub.BranchId).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<int> CreateUserAsync(int brandId, CreateUserRequest request)
    {
        if (!await CheckUserLimitAsync(brandId))
            throw new Exception("تم الوصول للحد الأقصى من المستخدمين (10 مستخدمين)");

        // التحقق من الـ Username غير مكرر
        if (await _context.Users.AnyAsync(u => u.BrandId == brandId && u.Username == request.Username))
            throw new Exception("اسم المستخدم موجود بالفعل");

        var user = new User
        {
            BrandId = brandId,
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = request.Password,
            Role = Enum.Parse<UserRole>(request.Role),
            PinCode = request.PinCode
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        for (int i = 0; i < request.BranchIds.Count; i++)
        {
            _context.UserBranches.Add(new UserBranch
            {
                UserId = user.UserId,
                BranchId = request.BranchIds[i],
                IsDefault = i == 0
            });
        }
        await _context.SaveChangesAsync();
        return user.UserId;
    }

    public async Task<bool> UpdateUserAsync(int userId, int brandId, UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.BrandId == brandId);
        if (user == null) return false;

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Role = Enum.Parse<UserRole>(request.Role);
        user.PinCode = request.PinCode;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        // لو اتبعت باسورد جديد، حدّثه (وفك أي قفل)
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = request.Password;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
        }

        // تحديث الفروع
        _context.UserBranches.RemoveRange(user.UserBranches);
        for (int i = 0; i < request.BranchIds.Count; i++)
        {
            _context.UserBranches.Add(new UserBranch
            {
                UserId = user.UserId,
                BranchId = request.BranchIds[i],
                IsDefault = i == 0
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId, int brandId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.BrandId == brandId);
        if (user == null) return false;
        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckUserLimitAsync(int brandId)
    {
        var brand = await _context.Brands.Include(b => b.Owner).FirstOrDefaultAsync(b => b.BrandId == brandId);
        if (brand == null) return false;
        var currentCount = await _context.Users.CountAsync(u => u.Brand.OwnerId == brand.OwnerId && u.IsActive);
        return currentCount < brand.Owner.MaxUsers;
    }

    public async Task<bool> ResetPasswordAsync(int userId, int brandId, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.BrandId == brandId);
        if (user == null) return false;
        user.PasswordHash = newPassword;
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// CustomerService
// ============================================================
public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    public CustomerService(AppDbContext context) => _context = context;

    public async Task<List<CustomerDto>> GetCustomersAsync(int brandId)
    {
        return await _context.Customers
            .Where(c => c.BrandId == brandId && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                Gender = c.Gender,
                BirthDate = c.BirthDate,
                CustomerType = c.CustomerType,
                LoyaltyPoints = c.LoyaltyPoints,
                TotalSpent = c.TotalSpent,
                CurrentBalance = c.CurrentBalance,
                LastPurchaseAt = c.LastPurchaseAt,
                IsActive = c.IsActive
            }).ToListAsync();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId, int brandId)
    {
        return await _context.Customers
            .Where(c => c.CustomerId == customerId && c.BrandId == brandId)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                Gender = c.Gender,
                BirthDate = c.BirthDate,
                CustomerType = c.CustomerType,
                LoyaltyPoints = c.LoyaltyPoints,
                TotalSpent = c.TotalSpent,
                CurrentBalance = c.CurrentBalance,
                LastPurchaseAt = c.LastPurchaseAt,
                IsActive = c.IsActive
            }).FirstOrDefaultAsync();
    }

    public async Task<CustomerDto?> SearchByPhoneAsync(string phone, int brandId)
    {
        return await _context.Customers
            .Where(c => c.BrandId == brandId && c.IsActive && c.Phone == phone)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                CustomerType = c.CustomerType,
                LoyaltyPoints = c.LoyaltyPoints,
                TotalSpent = c.TotalSpent,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive
            }).FirstOrDefaultAsync();
    }

    public async Task<int> CreateCustomerAsync(int brandId, CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            BrandId = brandId,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Gender = request.Gender,
            BirthDate = request.BirthDate,
            CustomerType = request.CustomerType,
            TaxNumber = request.TaxNumber,
            CreditLimit = request.CreditLimit,
            Notes = request.Notes,
            CustomerCode = $"CUST-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}"
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer.CustomerId;
    }

    public async Task<bool> UpdateCustomerAsync(int customerId, int brandId, CreateCustomerRequest request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.BrandId == brandId);
        if (customer == null) return false;
        customer.FullName = request.FullName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Gender = request.Gender;
        customer.BirthDate = request.BirthDate;
        customer.CustomerType = request.CustomerType;
        customer.TaxNumber = request.TaxNumber;
        customer.CreditLimit = request.CreditLimit;
        customer.Notes = request.Notes;
        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerAsync(int customerId, int brandId)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.BrandId == brandId);
        if (customer == null) return false;
        customer.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// CategoryService
// ============================================================
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    public CategoryService(AppDbContext context) => _context = context;

    public async Task<List<CategoryDto>> GetCategoriesAsync(int brandId)
    {
        return await _context.Categories
            .Where(c => c.BrandId == brandId && c.IsActive)
            .Include(c => c.ParentCategory)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                ParentCategoryId = c.ParentCategoryId,
                CategoryName = c.CategoryName,
                CategoryNameEn = c.CategoryNameEn,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.CategoryName : null,
                ProductsCount = _context.Products.Count(p => p.CategoryId == c.CategoryId && p.IsActive),
                IsActive = c.IsActive
            }).ToListAsync();
    }

    public async Task<int> CreateCategoryAsync(int brandId, CreateCategoryRequest request)
    {
        var category = new Category
        {
            BrandId = brandId,
            ParentCategoryId = request.ParentCategoryId,
            CategoryName = request.CategoryName,
            CategoryNameEn = request.CategoryNameEn,
            Description = request.Description,
            ShowOnline = request.ShowOnline
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.CategoryId;
    }

    public async Task<bool> UpdateCategoryAsync(int categoryId, int brandId, CreateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.BrandId == brandId);
        if (category == null) return false;
        category.CategoryName = request.CategoryName;
        category.CategoryNameEn = request.CategoryNameEn;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        category.ShowOnline = request.ShowOnline;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, int brandId)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.BrandId == brandId);
        if (category == null) return false;
        category.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// UnitService
// ============================================================
public class UnitService : IUnitService
{
    private readonly AppDbContext _context;
    public UnitService(AppDbContext context) => _context = context;

    public async Task<List<UnitDto>> GetUnitsAsync(int brandId)
    {
        return await _context.Units
            .Where(u => u.BrandId == brandId && u.IsActive)
            .Select(u => new UnitDto
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                UnitNameEn = u.UnitNameEn,
                UnitSymbol = u.UnitSymbol,
                IsActive = u.IsActive
            }).ToListAsync();
    }

    public async Task<int> CreateUnitAsync(int brandId, CreateUnitRequest request)
    {
        var unit = new Unit
        {
            BrandId = brandId,
            UnitName = request.UnitName,
            UnitNameEn = request.UnitNameEn,
            UnitSymbol = request.UnitSymbol
        };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();
        return unit.UnitId;
    }

    public async Task<bool> DeleteUnitAsync(int unitId, int brandId)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.UnitId == unitId && u.BrandId == brandId);
        if (unit == null) return false;
        unit.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

// ============================================================
// StockTransferService
// ============================================================
public class StockTransferService : IStockTransferService
{
    private readonly AppDbContext _context;
    public StockTransferService(AppDbContext context) => _context = context;

    public async Task<List<StockTransferDto>> GetTransfersAsync(int brandId)
    {
        // مفيش جدول للـ transfers في الـ schema الحالي - هنرجع قائمة فاضية
        // (هيتم إضافته في مرحلة لاحقة)
        return await Task.FromResult(new List<StockTransferDto>());
    }

    public async Task<int> CreateTransferAsync(int brandId, int userId, CreateTransferRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // خصم من الفرع المرسل
            foreach (var item in request.Items)
            {
                var fromInv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == request.FromBranchId);
                if (fromInv == null || fromInv.Quantity < item.Quantity)
                    throw new Exception("المخزون غير كاف");

                fromInv.Quantity -= item.Quantity;
                fromInv.LastUpdated = DateTime.UtcNow;

                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = request.FromBranchId,
                    VariantId = item.VariantId,
                    MovementType = StockMovementType.TransferOut,
                    Quantity = -item.Quantity,
                    BalanceAfter = fromInv.Quantity,
                    Notes = $"تحويل لفرع آخر",
                    UserId = userId
                });

                // إضافة للفرع المستلم
                var toInv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == request.ToBranchId);
                if (toInv == null)
                {
                    toInv = new Inventory
                    {
                        BranchId = request.ToBranchId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        AverageCost = fromInv.AverageCost
                    };
                    _context.Inventory.Add(toInv);
                }
                else
                {
                    toInv.Quantity += item.Quantity;
                    toInv.LastUpdated = DateTime.UtcNow;
                }

                _context.StockMovements.Add(new StockMovement
                {
                    BranchId = request.ToBranchId,
                    VariantId = item.VariantId,
                    MovementType = StockMovementType.TransferIn,
                    Quantity = item.Quantity,
                    BalanceAfter = toInv.Quantity,
                    Notes = $"استلام من فرع آخر",
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return 1; // success
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<bool> ConfirmReceiptAsync(int transferId, int brandId, int userId) => Task.FromResult(true);
    public Task<bool> CancelTransferAsync(int transferId, int brandId) => Task.FromResult(true);
}

// ============================================================
// BrandService
// ============================================================
public class BrandService : IBrandService
{
    private readonly AppDbContext _context;
    public BrandService(AppDbContext context) => _context = context;

    public async Task<List<BrandDto>> GetMyBrandsAsync(int ownerId)
    {
        return await _context.Brands
            .Where(b => b.OwnerId == ownerId && b.IsActive)
            .Select(b => new BrandDto
            {
                BrandId = b.BrandId,
                BrandName = b.BrandName,
                BusinessType = b.BusinessType.ToString(),
                Phone = b.Phone,
                Email = b.Email,
                
                IsActive = b.IsActive
            }).ToListAsync();
    }

    public async Task<BrandDto?> GetBrandByIdAsync(int brandId)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == brandId);
        if (brand == null) return null;
        return new BrandDto
        {
            BrandId = brand.BrandId,
            BrandName = brand.BrandName,
            BusinessType = brand.BusinessType.ToString(),
            Phone = brand.Phone,
            Email = brand.Email,
            
            IsActive = brand.IsActive
        };
    }

    public async Task<bool> SwitchBrandAsync(int userId, int brandId)
    {
        // تحقق من إن البراند موجود
        return await _context.Brands.AnyAsync(b => b.BrandId == brandId);
    }
}
