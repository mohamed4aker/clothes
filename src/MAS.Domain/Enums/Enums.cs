namespace MAS.Domain.Enums;

public enum BusinessType
{
    Clothing = 1,           // ملابس
    VeterinaryPharma = 2,   // أدوية بيطرية
    Manufacturing = 3,      // تصنيع
    Generic = 4,            // عام
    Pharmacy = 5,           // صيدلية بشرية
    Restaurant = 6          // مطعم
}

public enum UserRole
{
    Owner = 1,              // المالك
    BrandAdmin = 2,         // مدير البراند
    BranchManager = 3,      // مدير فرع
    Cashier = 4,            // كاشير
    Warehouse = 5,          // مخزن
    Accountant = 6,         // محاسب
    ProductionManager = 7,  // مدير إنتاج
    OnlineSales = 8         // مبيعات أونلاين
}

public enum ProductType
{
    Standard = 1,           // منتج عادي
    Manufactured = 2,       // مصنّع
    RawMaterial = 3,        // خامة
    Service = 4             // خدمة
}

public enum InvoiceStatus
{
    Draft = 1,
    Held = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5,
    PartiallyRefunded = 6
}

public enum PaymentMethod
{
    Cash = 1,
    Visa = 2,
    Mastercard = 3,
    VodafoneCash = 4,
    InstaPay = 5,
    Fawry = 6,
    BankTransfer = 7,
    StoreCredit = 8
}

public enum OnlineOrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,
    ReadyForShipment = 4,
    Shipped = 5,
    OutForDelivery = 6,
    Delivered = 7,
    Cancelled = 8,
    Returned = 9,
    Failed = 10
}

public enum StockMovementType
{
    Purchase = 1,
    Sale = 2,
    Return = 3,
    TransferIn = 4,
    TransferOut = 5,
    ProductionIn = 6,
    ProductionOut = 7,
    Waste = 8,
    Adjustment = 9,
    OnlineReserve = 10,
    OnlineRelease = 11,
    OnlineFulfilled = 12
}

public enum PromotionType
{
    BuyXGetY = 1,           // اشتري X خد Y مجاناً
    BuyXGetYDiscount = 2,   // اشتري X الـ Y التاني خصم
    PercentageDiscount = 3, // خصم نسبة
    FixedDiscount = 4,      // خصم مبلغ
    BundleDiscount = 5,     // خصم مجموعة
    CategoryDiscount = 6,   // خصم تصنيف
    MinPurchaseDiscount = 7,// خصم على فاتورة فوق مبلغ
    SecondItemDiscount = 8, // الصنف التاني بسعر مختلف
    FreeShipping = 9,       // شحن مجاني
    LoyaltyPoints = 10      // نقاط مضاعفة
}
