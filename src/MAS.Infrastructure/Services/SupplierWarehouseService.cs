using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using MAS.Domain.Entities;
using MAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAS.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;
    public SupplierService(AppDbContext context) => _context = context;

    public async Task<List<SupplierDto>> GetSuppliersAsync(int brandId)
    {
        return await _context.Suppliers
            .Where(s => s.BrandId == brandId && s.IsActive)
            .OrderBy(s => s.SupplierName)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                CompanyName = s.CompanyName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                TaxNumber = s.TaxNumber,
                CurrentBalance = s.CurrentBalance,
                TotalPurchases = s.TotalPurchases,
                PaymentTerms = s.PaymentTerms,
                Notes = s.Notes,
                IsActive = s.IsActive
            }).ToListAsync();
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int supplierId, int brandId)
    {
        var s = await _context.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.BrandId == brandId);
        if (s == null) return null;
        return new SupplierDto
        {
            SupplierId = s.SupplierId,
            SupplierName = s.SupplierName,
            CompanyName = s.CompanyName,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            TaxNumber = s.TaxNumber,
            CurrentBalance = s.CurrentBalance,
            TotalPurchases = s.TotalPurchases,
            PaymentTerms = s.PaymentTerms,
            Notes = s.Notes,
            IsActive = s.IsActive
        };
    }

    public async Task<int> CreateSupplierAsync(int brandId, CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            BrandId = brandId,
            SupplierName = request.SupplierName,
            CompanyName = request.CompanyName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            PaymentTerms = request.PaymentTerms,
            Notes = request.Notes
        };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return supplier.SupplierId;
    }

    public async Task<bool> UpdateSupplierAsync(int supplierId, int brandId, CreateSupplierRequest request)
    {
        var s = await _context.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.BrandId == brandId);
        if (s == null) return false;
        s.SupplierName = request.SupplierName;
        s.CompanyName = request.CompanyName;
        s.Phone = request.Phone;
        s.Email = request.Email;
        s.Address = request.Address;
        s.TaxNumber = request.TaxNumber;
        s.PaymentTerms = request.PaymentTerms;
        s.Notes = request.Notes;
        s.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSupplierAsync(int supplierId, int brandId)
    {
        var s = await _context.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.BrandId == brandId);
        if (s == null) return false;
        s.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;
    public WarehouseService(AppDbContext context) => _context = context;

    public async Task<List<WarehouseDto>> GetWarehousesAsync(int brandId)
    {
        return await _context.Warehouses
            .Where(w => w.BrandId == brandId && w.IsActive)
            .Include(w => w.Branch)
            .OrderBy(w => w.WarehouseName)
            .Select(w => new WarehouseDto
            {
                WarehouseId = w.WarehouseId,
                BranchId = w.BranchId,
                BranchName = w.Branch != null ? w.Branch.BranchName : null,
                WarehouseName = w.WarehouseName,
                WarehouseCode = w.WarehouseCode,
                WarehouseType = w.WarehouseType,
                Address = w.Address,
                ManagerName = w.ManagerName,
                Phone = w.Phone,
                IsMainWarehouse = w.IsMainWarehouse,
                IsActive = w.IsActive
            }).ToListAsync();
    }

    public async Task<int> CreateWarehouseAsync(int brandId, CreateWarehouseRequest request)
    {
        var w = new Warehouse
        {
            BrandId = brandId,
            BranchId = request.BranchId,
            WarehouseName = request.WarehouseName,
            WarehouseCode = request.WarehouseCode,
            WarehouseType = request.WarehouseType,
            Address = request.Address,
            ManagerName = request.ManagerName,
            Phone = request.Phone,
            IsMainWarehouse = request.IsMainWarehouse
        };
        _context.Warehouses.Add(w);
        await _context.SaveChangesAsync();
        return w.WarehouseId;
    }

    public async Task<bool> UpdateWarehouseAsync(int warehouseId, int brandId, CreateWarehouseRequest request)
    {
        var w = await _context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.BrandId == brandId);
        if (w == null) return false;
        w.BranchId = request.BranchId;
        w.WarehouseName = request.WarehouseName;
        w.WarehouseCode = request.WarehouseCode;
        w.WarehouseType = request.WarehouseType;
        w.Address = request.Address;
        w.ManagerName = request.ManagerName;
        w.Phone = request.Phone;
        w.IsMainWarehouse = request.IsMainWarehouse;
        w.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWarehouseAsync(int warehouseId, int brandId)
    {
        var w = await _context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.BrandId == brandId);
        if (w == null) return false;
        w.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
