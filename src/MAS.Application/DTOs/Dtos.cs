namespace MAS.Application.DTOs;

// ========== Auth DTOs ==========
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int OwnerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public List<BranchDto> Branches { get; set; } = new();
}

// ========== Brand ==========
public class BrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? BrandNameEn { get; set; }
    public string BusinessType { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool HasOnlineStore { get; set; }
    public string? OnlineStoreSlug { get; set; }
    public bool IsActive { get; set; }
}

// ========== Branch ==========
public class BranchDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBranchRequest
{
    public string BranchName { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public bool IsMainBranch { get; set; }
}

// ========== User ==========
public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<int> BranchIds { get; set; } = new();
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Cashier";
    public string? PinCode { get; set; }
    public List<int> BranchIds { get; set; } = new();
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = "Cashier";
    public string? PinCode { get; set; }
    public List<int> BranchIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; }   // اختياري: لو اتملت يتغيّر الباسورد
}

// ========== Product ==========
public class ProductDto
{
    public int ProductId { get; set; }
    public int? CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public decimal BaseCostPrice { get; set; }
    public decimal BaseSellingPrice { get; set; }
    public decimal? OnlinePrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal MinStock { get; set; }
    public bool HasVariants { get; set; }
    public bool ShowOnline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public int VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? VariantName { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal TotalStock { get; set; }
}

public class CreateProductRequest
{
    public int? CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseCostPrice { get; set; }
    public decimal BaseSellingPrice { get; set; }
    public decimal? OnlinePrice { get; set; }
    public decimal TaxRate { get; set; } = 14;
    public int? BaseUnitId { get; set; }
    public decimal MinStock { get; set; }
    public bool ShowOnline { get; set; }
}

public class CreateVariantRequest
{
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal InitialStock { get; set; }
    public int BranchId { get; set; }
}

// ========== Category ==========
public class CategoryDto
{
    public int CategoryId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryNameEn { get; set; }
    public string? ParentCategoryName { get; set; }
    public int ProductsCount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCategoryRequest
{
    public int? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryNameEn { get; set; }
    public string? Description { get; set; }
    public bool ShowOnline { get; set; } = true;
}

// ========== Unit ==========
public class UnitDto
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? UnitNameEn { get; set; }
    public string? UnitSymbol { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUnitRequest
{
    public string UnitName { get; set; } = string.Empty;
    public string? UnitNameEn { get; set; }
    public string? UnitSymbol { get; set; }
}

// ========== Customer ==========
public class CustomerDto
{
    public int CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? LastPurchaseAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string CustomerType { get; set; } = "Retail";
    public string? TaxNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public string? Notes { get; set; }
}

// ========== Sales ==========
public class CreateSaleRequest
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public List<SaleItemRequest> Items { get; set; } = new();
    public List<SalePaymentRequest> Payments { get; set; } = new();
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
}

public class SaleItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class SalePaymentRequest
{
    public string PaymentMethod { get; set; } = "Cash";
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class SaleResponse
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SaleDetailDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SaleItemDetailDto> Items { get; set; } = new();
    public List<PaymentDetailDto> Payments { get; set; } = new();
}

public class SaleItemDetailDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class PaymentDetailDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

// ========== Returns ==========
public class CreateReturnRequest
{
    public int OriginalInvoiceId { get; set; }
    public List<ReturnItemRequest> Items { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public string RefundMethod { get; set; } = "Cash";
}

public class ReturnItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool BackToStock { get; set; } = true;
}

// ========== Stock Transfer ==========
public class StockTransferDto
{
    public int TransferId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public int FromBranchId { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public int ToBranchId { get; set; }
    public string ToBranchName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int ItemsCount { get; set; }
}

public class CreateTransferRequest
{
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public List<TransferItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class TransferItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
}

// ========== Dashboard ==========
public class DashboardStatsDto
{
    public decimal TodaySales { get; set; }
    public int TodayInvoiceCount { get; set; }
    public decimal MonthSales { get; set; }
    public decimal PeriodSales { get; set; }
    public int PeriodInvoiceCount { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int TotalCustomers { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

// ========== Manufacturing DTOs ==========

public class BomDto
{
    public int BomId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BomName { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public decimal DirectLaborCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal AdminExpenses { get; set; }
    public decimal OtherCosts { get; set; }
    public string? OtherCostsDescription { get; set; }
    public decimal ExpectedWastePercent { get; set; }
    public decimal TotalMaterialsCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal SuggestedSellingPrice { get; set; }
    public decimal TargetMarginPercent { get; set; }
    public decimal OutputQuantity { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BomItemDto> Items { get; set; } = new();
}

public class BomItemDto
{
    public int BomItemId { get; set; }
    public int RawMaterialVariantId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineCost { get; set; }
    public decimal AvailableStock { get; set; }
    public string? Notes { get; set; }
}

public class CreateBomRequest
{
    public int ProductId { get; set; }
    public string BomName { get; set; } = string.Empty;
    public decimal DirectLaborCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal AdminExpenses { get; set; }
    public decimal OtherCosts { get; set; }
    public string? OtherCostsDescription { get; set; }
    public decimal ExpectedWastePercent { get; set; }
    public decimal TargetMarginPercent { get; set; } = 30;
    public decimal OutputQuantity { get; set; } = 1;
    public string? Notes { get; set; }
    public List<BomItemRequest> Items { get; set; } = new();
}

public class BomItemRequest
{
    public int RawMaterialVariantId { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitId { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}

// حاسبة الإنتاج العكسية
public class ProductionCapacityRequest
{
    public int BomId { get; set; }
    public int BranchId { get; set; }
}

public class ProductionCapacityResponse
{
    public int BomId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int MaxPossibleUnits { get; set; }
    public string? BottleneckMaterial { get; set; }
    public List<MaterialAvailability> Materials { get; set; } = new();
}

public class MaterialAvailability
{
    public string MaterialName { get; set; } = string.Empty;
    public decimal RequiredPerUnit { get; set; }
    public decimal AvailableStock { get; set; }
    public int PossibleUnits { get; set; }
    public decimal NeededForTarget { get; set; }
    public decimal Shortage { get; set; }
    public bool IsBottleneck { get; set; }
}

// أمر إنتاج
public class ProductionOrderDto
{
    public int ProductionOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
    public decimal WasteQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalProductionCost { get; set; }
    public decimal UnitProductionCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateProductionOrderRequest
{
    public int BomId { get; set; }
    public int BranchId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CompleteProductionRequest
{
    public int ProductionOrderId { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal WasteQuantity { get; set; }
    public List<MaterialUsageRequest> MaterialsUsed { get; set; } = new();
    public string? Notes { get; set; }
}

public class MaterialUsageRequest
{
    public int RawMaterialVariantId { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal WasteQuantity { get; set; }
}

// ========== Expenses ==========
public class ExpenseDto
{
    public int ExpenseId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public DateTime ExpenseDate { get; set; }
    public string? BranchName { get; set; }
    public string? Notes { get; set; }
}

public class CreateExpenseRequest
{
    public int? BranchId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? ReceiptNumber { get; set; }
    public string? Notes { get; set; }
}

// ========== Purchases ==========
public class PurchaseInvoiceDto
{
    public int PurchaseInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public string PurchaseType { get; set; } = "Finished";
    public string? WarehouseName { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierPhone { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
}

public class CreatePurchaseRequest
{
    public int BranchId { get; set; }
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public string PurchaseType { get; set; } = "Finished";
    public string? SupplierName { get; set; }
    public string? SupplierPhone { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal ShippingCost { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public List<PurchaseItemRequest> Items { get; set; } = new();
}

public class PurchaseItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

// ========== Brand Settings ==========
public class BrandSettingsDto
{
    public bool ShowTaxInPOS { get; set; }
    public bool ShowProfitInPOS { get; set; }
    public bool ShowProductImages { get; set; }
    public bool HideZeroStockProducts { get; set; }
    public string PrimaryColor { get; set; } = "#0d9488";
    public string ReceiptHeader { get; set; } = string.Empty;
    public string ReceiptFooter { get; set; } = string.Empty;
}

// ========== Advanced Dashboard ==========
public class AdvancedDashboardDto
{
    // الفترات
    public PeriodStatsDto Today { get; set; } = new();
    public PeriodStatsDto Week { get; set; } = new();
    public PeriodStatsDto Month { get; set; } = new();
    public PeriodStatsDto Year { get; set; } = new();
    public PeriodStatsDto AllTime { get; set; } = new();
    public PeriodStatsDto CustomPeriod { get; set; } = new();
    
    // إحصائيات عامة
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int TotalCustomers { get; set; }
    public decimal InventoryValue { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class PeriodStatsDto
{
    public string PeriodName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int InvoicesCount { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal CostOfGoodsSold { get; set; }    // تكلفة البضاعة المباعة
    public decimal GrossProfit { get; set; }         // الربح الإجمالي = المبيعات - تكلفة البضاعة
    public decimal NetProfit { get; set; }           // صافي الربح = الإجمالي - المصاريف
    public decimal AverageProfitPerSale { get; set; } // متوسط الربح في الفاتورة
    public bool ExpensesCovered { get; set; }        // المصروفات مغطاة ولا لا
}

// ========== Sales Returns ==========
public class SalesReturnDto
{
    public int ReturnId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string OriginalInvoiceNumber { get; set; } = string.Empty;
    public int OriginalInvoiceId { get; set; }
    public string? CustomerName { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundMethod { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public int ItemsCount { get; set; }
}

public class CreateSalesReturnRequest
{
    public int OriginalInvoiceId { get; set; }
    public List<ReturnItemRequest2> Items { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public string RefundMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
}

public class ReturnItemRequest2
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool BackToStock { get; set; } = true;
    public string? ItemReason { get; set; }
}

// ========== Stock Take ==========
public class StockTakeDto
{
    public int StockTakeId { get; set; }
    public string StockTakeNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime StockTakeDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalDifferenceValue { get; set; }
    public int ItemsCount { get; set; }
    public string? Notes { get; set; }
}

public class StockTakeDetailDto
{
    public int StockTakeId { get; set; }
    public string StockTakeNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public DateTime StockTakeDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalDifferenceValue { get; set; }
    public List<StockTakeItemDto> Items { get; set; } = new();
}

public class StockTakeItemDto
{
    public int ItemId { get; set; }
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Difference { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DifferenceValue { get; set; }
    public string? Notes { get; set; }
}

public class CreateStockTakeRequest
{
    public int BranchId { get; set; }
    public string? Notes { get; set; }
    public List<StockTakeItemRequest> Items { get; set; } = new();
}

public class StockTakeItemRequest
{
    public int VariantId { get; set; }
    public decimal CountedQuantity { get; set; }
    public string? Notes { get; set; }
}

// ========== Cashier Session (تسليم درج) ==========
public class CashierSessionDto
{
    public int SessionId { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ExpectedBalance { get; set; }
    public decimal? Difference { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal TotalReturns { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OpenSessionRequest
{
    public int BranchId { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? Notes { get; set; }
}

public class CloseSessionRequest
{
    public int SessionId { get; set; }
    public decimal CountedCash { get; set; }
    public string? Notes { get; set; }
}

// ========== Promotions ==========
public class PromotionDto
{
    public int PromotionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool ApplyAutomatically { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => EndDate < DateTime.UtcNow;
    public string Status => !IsActive ? "متوقف" : IsExpired ? "منتهي" : DateTime.UtcNow < StartDate ? "لم يبدأ" : "نشط";
}

public class CreatePromotionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CouponCode { get; set; }
    public string DiscountType { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public bool ApplyAutomatically { get; set; }
}

// ========== Reports ==========
public class SalesReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrossProfit { get; set; }
    public int InvoicesCount { get; set; }
    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Sales { get; set; }
    public int InvoicesCount { get; set; }
}

public class TopCustomerDto
{
    public string CustomerName { get; set; } = string.Empty;
    public int InvoicesCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class InventoryReportDto
{
    public List<InventoryItemDto> Items { get; set; } = new();
    public decimal TotalValue { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class InventoryItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal MinStock { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ========== E-Commerce ==========
public class OnlineOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal SellerShippingShare { get; set; }
    public decimal CustomerShippingShare { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Notes { get; set; }
    public int ItemsCount { get; set; }
    public List<OnlineOrderItemDto> Items { get; set; } = new();
}

public class OnlineOrderItemDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateOnlineOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public decimal CustomerShippingShare { get; set; }
    public string? Notes { get; set; }
    public List<OnlineOrderItemRequest> Items { get; set; } = new();
}

public class OnlineOrderItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
}

public class OnlineProductDto
{
    public int ProductId { get; set; }
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Stock { get; set; }
}

// ========== Supplier (المورد) ==========
public class SupplierDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalPurchases { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierRequest
{
    public string SupplierName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
}

// ========== Warehouse (المخزن) ==========
public class WarehouseDto
{
    public int WarehouseId { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public bool IsMainWarehouse { get; set; }
    public bool IsActive { get; set; }
}

public class CreateWarehouseRequest
{
    public int? BranchId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseType { get; set; } = "Finished";
    public string? Address { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public bool IsMainWarehouse { get; set; }
}

// ========== Initial Stock (مخزن أول المدة) ==========
public class InitialStockDto
{
    public int InitialStockId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? WarehouseName { get; set; }
    public DateTime EntryDate { get; set; }
    public int ItemsCount { get; set; }
    public decimal TotalValue { get; set; }
    public string? Notes { get; set; }
}

public class CreateInitialStockRequest
{
    public int BranchId { get; set; }
    public int? WarehouseId { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public List<InitialStockItemRequest> Items { get; set; } = new();
}

public class InitialStockItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

// ========== Sales Summary ==========
public class SaleSummaryDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string? CustomerName { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
}

// ========== Purchase Return ==========
public class PurchaseReturnDto
{
    public int PurchaseReturnId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string? PurchaseInvoiceNumber { get; set; }
    public string? SupplierName { get; set; }
    public string? WarehouseName { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundMethod { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseReturnRequest
{
    public int BranchId { get; set; }
    public int? PurchaseInvoiceId { get; set; }
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public string? Reason { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public List<PurchaseReturnItemRequest> Items { get; set; } = new();
}

public class PurchaseReturnItemRequest
{
    public int VariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

// ========== Financial Operations (سندات) ==========
public class FinancialVoucherDto
{
    public int VoucherId { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string VoucherType { get; set; } = string.Empty;  // Receipt, Payment
    public DateTime VoucherDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PartyName { get; set; }     // المستلم/الدافع
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class CreateVoucherRequest
{
    public string VoucherType { get; set; } = "Receipt";  // Receipt, Payment
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? PartyName { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }
}
