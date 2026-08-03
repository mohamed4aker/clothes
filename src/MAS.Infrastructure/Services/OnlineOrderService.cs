using MAS.Application.Common;
using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Domain.Enums;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

public class OnlineOrderService : IOnlineOrderService
{
    private readonly AppDbContext _context;
    public OnlineOrderService(AppDbContext context) => _context = context;

    public async Task<List<OnlineOrderDto>> GetOrdersAsync(int brandId)
    {
        return await _context.OnlineOrders
            .Where(o => o.BrandId == brandId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OnlineOrderDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                CustomerEmail = o.CustomerEmail,
                ShippingAddress = o.ShippingAddress,
                City = o.City,
                SubTotal = o.SubTotal,
                ShippingCost = o.ShippingCost,
                SellerShippingShare = o.SellerShippingShare,
                CustomerShippingShare = o.CustomerShippingShare,
                GrandTotal = o.GrandTotal,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                IsPaid = o.IsPaid,
                OrderDate = o.OrderDate,
                Notes = o.Notes,
                ItemsCount = o.Items.Count
            }).ToListAsync();
    }

    public async Task<OnlineOrderDto?> GetOrderByIdAsync(int orderId, int brandId)
    {
        var order = await _context.OnlineOrders
            .Where(o => o.OrderId == orderId && o.BrandId == brandId)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();
        if (order == null) return null;

        return new OnlineOrderDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            ShippingAddress = order.ShippingAddress,
            City = order.City,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            SellerShippingShare = order.SellerShippingShare,
            CustomerShippingShare = order.CustomerShippingShare,
            GrandTotal = order.GrandTotal,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            IsPaid = order.IsPaid,
            OrderDate = order.OrderDate,
            Notes = order.Notes,
            ItemsCount = order.Items.Count,
            Items = order.Items.Select(i => new OnlineOrderItemDto
            {
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }

    public async Task<int> CreateOrderAsync(int brandId, CreateOnlineOrderRequest request)
    {
        var lastOrder = await _context.OnlineOrders
            .Where(o => o.BrandId == brandId)
            .OrderByDescending(o => o.OrderId)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync();
        var prefix = $"ORD-{DateTime.UtcNow:yyyyMM}-";
        var orderNumber = DocumentNumber.Next(prefix, lastOrder, 3);

        decimal subTotal = 0;
        var order = new OnlineOrder
        {
            BrandId = brandId,
            OrderNumber = orderNumber,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerEmail = request.CustomerEmail,
            ShippingAddress = request.ShippingAddress,
            City = request.City,
            ShippingCost = request.ShippingCost,
            CustomerShippingShare = request.CustomerShippingShare,
            SellerShippingShare = request.ShippingCost - request.CustomerShippingShare,
            Notes = request.Notes,
            Status = "Pending",
            PaymentMethod = "COD"
        };

        foreach (var item in request.Items)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == item.VariantId);
            if (variant == null) continue;

            var price = variant.SellingPrice ?? variant.Product!.BaseSellingPrice;
            var lineTotal = price * item.Quantity;
            subTotal += lineTotal;

            order.Items.Add(new OnlineOrderItem
            {
                VariantId = variant.VariantId,
                ProductName = variant.VariantName ?? variant.Product!.ProductName,
                Quantity = item.Quantity,
                UnitPrice = price,
                LineTotal = lineTotal
            });
        }

        order.SubTotal = subTotal;
        order.GrandTotal = subTotal + request.CustomerShippingShare;

        _context.OnlineOrders.Add(order);
        await _context.SaveChangesAsync();
        return order.OrderId;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int brandId, string newStatus, int userId)
    {
        var order = await _context.OnlineOrders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BrandId == brandId);
        if (order == null) return false;
        order.Status = newStatus;
        order.ProcessedByUserId = userId;
        if (newStatus == "Delivered") { order.DeliveryDate = DateTime.UtcNow; order.IsPaid = true; }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConvertToInvoiceAsync(int orderId, int brandId, int branchId, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.OnlineOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BrandId == brandId);
            if (order == null) return false;

            var lastInvoice = await _context.SalesInvoices
                .Where(i => i.BrandId == brandId)
                .OrderByDescending(i => i.InvoiceId)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();
            var prefix = $"INV-{DateTime.UtcNow:yyyyMM}-";
            var invoiceNumber = DocumentNumber.Next(prefix, lastInvoice, 5);

            var invoice = new SalesInvoice
            {
                BrandId = brandId,
                BranchId = branchId,
                InvoiceNumber = invoiceNumber,
                InvoiceType = "Online",
                CashierUserId = userId,
                SubTotal = order.SubTotal,
                ShippingCost = order.CustomerShippingShare,
                GrandTotal = order.GrandTotal,
                PaidAmount = order.GrandTotal,
                Status = InvoiceStatus.Completed,
                Notes = $"طلب أونلاين رقم {order.OrderNumber}"
            };

            foreach (var item in order.Items)
            {
                var inv = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.VariantId == item.VariantId && i.BranchId == branchId);
                
                invoice.Items.Add(new SalesInvoiceItem
                {
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitCost = inv?.AverageCost ?? 0,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                });

                if (inv != null)
                {
                    inv.Quantity -= item.Quantity;
                    inv.LastUpdated = DateTime.UtcNow;
                    
                    _context.StockMovements.Add(new StockMovement
                    {
                        BranchId = branchId,
                        VariantId = item.VariantId,
                        MovementType = StockMovementType.Sale,
                        Quantity = -item.Quantity,
                        BalanceAfter = inv.Quantity,
                        ReferenceType = "OnlineOrder",
                        Notes = $"بيع أونلاين - {order.OrderNumber}",
                        UserId = userId
                    });
                }
            }

            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            order.LinkedInvoiceId = invoice.InvoiceId;
            order.Status = "Delivered";
            order.IsPaid = true;
            order.DeliveryDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OnlineProductDto>> GetOnlineProductsAsync(int brandId)
    {
        return await _context.Products
            .Where(p => p.BrandId == brandId && p.IsActive && p.ShowOnline)
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Inventory)
            .SelectMany(p => p.Variants.Select(v => new OnlineProductDto
            {
                ProductId = p.ProductId,
                VariantId = v.VariantId,
                ProductName = v.VariantName ?? p.ProductName,
                Description = p.Description,
                Price = p.OnlinePrice ?? v.SellingPrice ?? p.BaseSellingPrice,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                ImageUrl = p.MainImageUrl,
                Stock = v.Inventory.Sum(i => i.Quantity)
            }))
            .Where(x => x.Stock > 0)
            .ToListAsync();
    }

    public async Task<OnlineStoreDto?> GetStoreAsync(string? slug)
    {
        var query = _context.Brands.Where(b => b.IsActive && b.HasOnlineStore);

        // لو الرابط فيه slug نجيب البراند بتاعه بالظبط،
        // لو مفيش slug نفتح أول متجر مفعّل (حالة البراند الواحد).
        var brand = string.IsNullOrWhiteSpace(slug)
            ? await query.OrderBy(b => b.BrandId).FirstOrDefaultAsync()
            : await query.FirstOrDefaultAsync(b => b.OnlineStoreSlug == slug);

        if (brand == null) return null;

        return new OnlineStoreDto
        {
            BrandId = brand.BrandId,
            BrandName = brand.BrandName,
            Slug = brand.OnlineStoreSlug
        };
    }
}
