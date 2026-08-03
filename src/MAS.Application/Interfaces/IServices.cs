using MAS.Application.DTOs;

namespace MAS.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> ValidateUserAsync(int userId);
}

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync(int brandId);
    Task<ProductDto?> GetProductByIdAsync(int productId, int brandId);
    Task<ProductDto?> GetByBarcodeAsync(string barcode, int brandId);
    Task<int> CreateProductAsync(int brandId, int userId, CreateProductRequest request);
    Task<bool> UpdateProductAsync(int productId, int brandId, CreateProductRequest request);
    Task<bool> DeleteProductAsync(int productId, int brandId);
    
    // Variants
    Task<int> AddVariantAsync(int productId, int brandId, CreateVariantRequest request);
    Task<bool> UpdateVariantAsync(int variantId, int brandId, CreateVariantRequest request);
    Task<bool> DeleteVariantAsync(int variantId, int brandId);
    
    // Stock adjustment
    Task<bool> AdjustStockAsync(int variantId, int branchId, decimal newQuantity, string reason, int userId);
}

public interface ISalesService
{
    Task<decimal> GetTotalSalesAsync(int brandId, DateTime from, DateTime to);
    Task<List<SaleSummaryDto>> GetSalesSummaryAsync(int brandId, DateTime from, DateTime to);
    Task<SaleResponse> CreateSaleAsync(int brandId, int userId, CreateSaleRequest request);
    Task<List<SaleResponse>> GetRecentSalesAsync(int brandId, int branchId, int count = 20);
    Task<SaleResponse?> GetSaleByIdAsync(int invoiceId, int brandId);
    Task<SaleDetailDto?> GetSaleDetailsAsync(int invoiceId, int brandId);
    Task<bool> CreateReturnAsync(int brandId, int userId, CreateReturnRequest request);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int brandId, int? branchId = null);
    Task<DashboardStatsDto> GetStatsForPeriodAsync(int brandId, DateTime from, DateTime to, int? branchId = null);
}

public interface IBranchService
{
    Task<List<BranchDto>> GetBranchesAsync(int brandId);
    Task<BranchDto?> GetBranchByIdAsync(int branchId, int brandId);
    Task<int> CreateBranchAsync(int brandId, CreateBranchRequest request);
    Task<bool> UpdateBranchAsync(int branchId, int brandId, CreateBranchRequest request);
    Task<bool> DeleteBranchAsync(int branchId, int brandId);
}

public interface IUserService
{
    Task<List<UserDto>> GetUsersAsync(int brandId);
    Task<UserDto?> GetUserByIdAsync(int userId, int brandId);
    Task<int> CreateUserAsync(int brandId, CreateUserRequest request);
    Task<bool> UpdateUserAsync(int userId, int brandId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int userId, int brandId);
    Task<bool> CheckUserLimitAsync(int brandId);
    Task<bool> ResetPasswordAsync(int userId, int brandId, string newPassword);
}

public interface ICustomerService
{
    Task<List<CustomerDto>> GetCustomersAsync(int brandId);
    Task<CustomerDto?> GetCustomerByIdAsync(int customerId, int brandId);
    Task<CustomerDto?> SearchByPhoneAsync(string phone, int brandId);
    Task<int> CreateCustomerAsync(int brandId, CreateCustomerRequest request);
    Task<bool> UpdateCustomerAsync(int customerId, int brandId, CreateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(int customerId, int brandId);
}

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync(int brandId);
    Task<int> CreateCategoryAsync(int brandId, CreateCategoryRequest request);
    Task<bool> UpdateCategoryAsync(int categoryId, int brandId, CreateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int categoryId, int brandId);
}

public interface IUnitService
{
    Task<List<UnitDto>> GetUnitsAsync(int brandId);
    Task<int> CreateUnitAsync(int brandId, CreateUnitRequest request);
    Task<bool> DeleteUnitAsync(int unitId, int brandId);
}

public interface IStockTransferService
{
    Task<List<StockTransferDto>> GetTransfersAsync(int brandId);
    Task<int> CreateTransferAsync(int brandId, int userId, CreateTransferRequest request);
    Task<bool> ConfirmReceiptAsync(int transferId, int brandId, int userId);
    Task<bool> CancelTransferAsync(int transferId, int brandId);
}

// ============================================================
// Manufacturing Service
// ============================================================
public interface IManufacturingService
{
    // BOM
    Task<List<BomDto>> GetBomsAsync(int brandId);
    Task<BomDto?> GetBomByIdAsync(int bomId, int brandId);
    Task<int> CreateBomAsync(int brandId, int userId, CreateBomRequest request);
    Task<bool> UpdateBomAsync(int bomId, int brandId, CreateBomRequest request);
    Task<bool> DeleteBomAsync(int bomId, int brandId);
    
    // الحاسبة العكسية - عندي خامات كذا، أعمل كام منتج؟
    Task<ProductionCapacityResponse> CalculateCapacityAsync(int bomId, int branchId, int brandId);
    
    // أوامر الإنتاج
    Task<List<ProductionOrderDto>> GetProductionOrdersAsync(int brandId);
    Task<int> CreateProductionOrderAsync(int brandId, int userId, CreateProductionOrderRequest request);
    Task<bool> CompleteProductionAsync(int brandId, int userId, CompleteProductionRequest request);
    Task<bool> CancelProductionOrderAsync(int orderId, int brandId);
}

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetExpensesAsync(int brandId, DateTime? from = null, DateTime? to = null);
    Task<int> CreateExpenseAsync(int brandId, int userId, CreateExpenseRequest request);
    Task<bool> DeleteExpenseAsync(int expenseId, int brandId);
    Task<decimal> GetTotalExpensesAsync(int brandId, DateTime from, DateTime to);
}

public interface IPurchaseService
{
    Task<List<PurchaseInvoiceDto>> GetPurchasesAsync(int brandId);
    Task<int> CreatePurchaseAsync(int brandId, int userId, CreatePurchaseRequest request);
    Task<decimal> GetTotalPurchasesAsync(int brandId, DateTime from, DateTime to);
}

public interface IBrandSettingsService
{
    Task<BrandSettingsDto> GetSettingsAsync(int brandId);
    Task<bool> UpdateSettingsAsync(int brandId, BrandSettingsDto settings);
}

public interface IAdvancedDashboardService
{
    Task<AdvancedDashboardDto> GetDashboardAsync(int brandId, int? branchId = null, DateTime? customFrom = null, DateTime? customTo = null);
}

public interface ISalesReturnService
{
    Task<List<SalesReturnDto>> GetReturnsAsync(int brandId);
    Task<int> CreateReturnAsync(int brandId, int userId, CreateSalesReturnRequest request);
    Task<SaleDetailDto?> GetInvoiceForReturnAsync(int invoiceId, int brandId);
}

public interface IStockTakeService
{
    Task<List<StockTakeDto>> GetStockTakesAsync(int brandId);
    Task<StockTakeDetailDto?> GetStockTakeByIdAsync(int stockTakeId, int brandId);
    Task<List<StockTakeItemDto>> PrepareStockTakeAsync(int branchId, int brandId);
    Task<int> CreateStockTakeAsync(int brandId, int userId, CreateStockTakeRequest request);
}

public interface ICashierSessionService
{
    Task<CashierSessionDto?> GetActiveSessionAsync(int userId, int branchId);
    Task<List<CashierSessionDto>> GetSessionsAsync(int brandId, int? userId = null);
    Task<int> OpenSessionAsync(int userId, OpenSessionRequest request);
    Task<bool> CloseSessionAsync(int userId, CloseSessionRequest request);
}

public interface IPromotionService
{
    Task<List<PromotionDto>> GetPromotionsAsync(int brandId);
    Task<int> CreatePromotionAsync(int brandId, CreatePromotionRequest request);
    Task<bool> UpdatePromotionAsync(int promotionId, int brandId, CreatePromotionRequest request);
    Task<bool> DeletePromotionAsync(int promotionId, int brandId);
    Task<PromotionDto?> ValidateCouponAsync(string code, int brandId, decimal cartTotal);
}

public interface IReportsService
{
    Task<SalesReportDto> GetSalesReportAsync(int brandId, DateTime from, DateTime to, int? branchId = null);
    Task<InventoryReportDto> GetInventoryReportAsync(int brandId, int? branchId = null);
}

public interface IOnlineOrderService
{
    Task<List<OnlineOrderDto>> GetOrdersAsync(int brandId);
    Task<OnlineOrderDto?> GetOrderByIdAsync(int orderId, int brandId);
    Task<int> CreateOrderAsync(int brandId, CreateOnlineOrderRequest request);
    Task<bool> UpdateOrderStatusAsync(int orderId, int brandId, string newStatus, int userId);
    Task<bool> ConvertToInvoiceAsync(int orderId, int brandId, int branchId, int userId);
    Task<List<OnlineProductDto>> GetOnlineProductsAsync(int brandId);
}

public interface IBrandService
{
    Task<List<BrandDto>> GetMyBrandsAsync(int ownerId);
    Task<BrandDto?> GetBrandByIdAsync(int brandId);
    Task<bool> SwitchBrandAsync(int userId, int brandId);
}

public interface ISupplierService
{
    Task<List<SupplierDto>> GetSuppliersAsync(int brandId);
    Task<SupplierDto?> GetSupplierByIdAsync(int supplierId, int brandId);
    Task<int> CreateSupplierAsync(int brandId, CreateSupplierRequest request);
    Task<bool> UpdateSupplierAsync(int supplierId, int brandId, CreateSupplierRequest request);
    Task<bool> DeleteSupplierAsync(int supplierId, int brandId);
}

public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetWarehousesAsync(int brandId);
    Task<int> CreateWarehouseAsync(int brandId, CreateWarehouseRequest request);
    Task<bool> UpdateWarehouseAsync(int warehouseId, int brandId, CreateWarehouseRequest request);
    Task<bool> DeleteWarehouseAsync(int warehouseId, int brandId);
}

public interface IInitialStockService
{
    Task<List<InitialStockDto>> GetInitialStocksAsync(int brandId);
    Task<int> CreateInitialStockAsync(int brandId, int userId, CreateInitialStockRequest request);
}

public interface IPurchaseReturnService
{
    Task<List<PurchaseReturnDto>> GetReturnsAsync(int brandId);
    Task<int> CreateReturnAsync(int brandId, int userId, CreatePurchaseReturnRequest request);
}
