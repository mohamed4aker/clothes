-- =====================================================
-- SECTION 7: SALES & POS
-- المبيعات ونقاط البيع
-- =====================================================

USE MAS_DB;
GO

-- جلسات الكاشير (شفت)
CREATE TABLE CashierSessions (
    SessionId INT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL,
    UserId INT NOT NULL,
    OpeningBalance DECIMAL(18,2) NOT NULL,     -- اللي في الدرج وقت الفتح
    ClosingBalance DECIMAL(18,2),              -- اللي في الدرج وقت القفل
    ExpectedBalance DECIMAL(18,2),             -- المتوقع في الدرج
    Difference DECIMAL(18,2),                  -- الفرق
    TotalSales DECIMAL(18,2) DEFAULT 0,
    TotalReturns DECIMAL(18,2) DEFAULT 0,
    TotalCash DECIMAL(18,2) DEFAULT 0,
    TotalCard DECIMAL(18,2) DEFAULT 0,
    TotalWallet DECIMAL(18,2) DEFAULT 0,
    SessionStart DATETIME2 DEFAULT GETDATE(),
    SessionEnd DATETIME2,
    Status NVARCHAR(20) DEFAULT 'Open',        -- Open, Closed
    Notes NVARCHAR(500),
    CONSTRAINT FK_CashierSessions_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_CashierSessions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- فواتير البيع
CREATE TABLE SalesInvoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    InvoiceType NVARCHAR(20) DEFAULT 'POS',    -- POS, Online, B2B, Wholesale
    SaleType NVARCHAR(20) DEFAULT 'Cash',      -- Cash, Credit
    CustomerId INT,
    CashierUserId INT NOT NULL,
    SessionId INT,                             -- جلسة الكاشير
    -- المبالغ
    SubTotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    PromotionDiscount DECIMAL(18,2) DEFAULT 0, -- خصم العروض
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    ShippingCost DECIMAL(18,2) DEFAULT 0,
    GrandTotal DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    ChangeAmount DECIMAL(18,2) DEFAULT 0,      -- الباقي للعميل
    -- بيانات إضافية
    AppliedPromotionId INT,
    AppliedCouponCode NVARCHAR(50),
    LoyaltyPointsEarned INT DEFAULT 0,
    LoyaltyPointsRedeemed INT DEFAULT 0,
    Status NVARCHAR(20) DEFAULT 'Completed',   -- Draft, Held, Completed, Cancelled, Refunded, PartiallyRefunded
    -- الفوترة الإلكترونية
    ETASubmissionStatus NVARCHAR(20),          -- Pending, Submitted, Accepted, Rejected
    ETAUUID NVARCHAR(100),
    ETASubmittedAt DATETIME2,
    -- حقول للأونلاين
    IsOnlineOrder BIT DEFAULT 0,
    OnlineOrderId INT,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_SalesInvoices_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_SalesInvoices_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_SalesInvoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_SalesInvoices_Users FOREIGN KEY (CashierUserId) REFERENCES Users(UserId),
    CONSTRAINT FK_SalesInvoices_Sessions FOREIGN KEY (SessionId) REFERENCES CashierSessions(SessionId),
    CONSTRAINT UQ_SalesInvoices UNIQUE (BrandId, InvoiceNumber)
);

CREATE TABLE SalesInvoiceItems (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    VariantId INT NOT NULL,
    BatchId INT,
    Quantity DECIMAL(18,3) NOT NULL,
    UnitId INT,                                -- وحدة البيع (متر، نص متر، إلخ)
    ConversionFactor DECIMAL(18,4) DEFAULT 1,
    UnitCost DECIMAL(18,2),                    -- سعر التكلفة وقت البيع (للتقارير)
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    PromotionDiscount DECIMAL(18,2) DEFAULT 0, -- خصم العرض على الصنف
    TaxRate DECIMAL(5,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL,
    AppliedPromotionId INT,                    -- لو الصنف ده تطبق عليه عرض
    Notes NVARCHAR(300),
    CONSTRAINT FK_SII_Invoices FOREIGN KEY (InvoiceId) REFERENCES SalesInvoices(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_SII_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT FK_SII_Batches FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_SII_Units FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
);

-- مدفوعات الفاتورة (ممكن دفع متعدد: كاش + فيزا)
CREATE TABLE InvoicePayments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,       -- Cash, Visa, Mastercard, VodafoneCash, InstaPay, Fawry, BankTransfer, StoreCredit
    Amount DECIMAL(18,2) NOT NULL,
    ReferenceNumber NVARCHAR(100),             -- رقم العملية للفيزا أو المحفظة
    Notes NVARCHAR(300),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_InvoicePayments_Invoices FOREIGN KEY (InvoiceId) REFERENCES SalesInvoices(InvoiceId) ON DELETE CASCADE
);

-- المرتجعات
CREATE TABLE SalesReturns (
    ReturnId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT NOT NULL,
    ReturnNumber NVARCHAR(50) NOT NULL,
    OriginalInvoiceId INT NOT NULL,            -- الفاتورة الأصلية
    CustomerId INT,
    ReturnType NVARCHAR(20) DEFAULT 'Refund',  -- Refund, Exchange
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    GrandTotal DECIMAL(18,2) NOT NULL,
    RefundMethod NVARCHAR(50),                 -- Cash, StoreCredit, OriginalPayment
    Reason NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'Pending',     -- Pending, Approved, Rejected, Completed
    ApprovedBy INT,
    ApprovedAt DATETIME2,
    ProcessedBy INT NOT NULL,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_SalesReturns_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_SalesReturns_OriginalInvoice FOREIGN KEY (OriginalInvoiceId) REFERENCES SalesInvoices(InvoiceId),
    CONSTRAINT UQ_SalesReturns UNIQUE (BrandId, ReturnNumber)
);

CREATE TABLE SalesReturnItems (
    ReturnItemId INT IDENTITY(1,1) PRIMARY KEY,
    ReturnId INT NOT NULL,
    OriginalItemId INT,                        -- الصنف في الفاتورة الأصلية
    VariantId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(300),
    BackToStock BIT DEFAULT 1,                 -- يرجع للمخزون ولا تالف
    CONSTRAINT FK_SRI_Returns FOREIGN KEY (ReturnId) REFERENCES SalesReturns(ReturnId) ON DELETE CASCADE,
    CONSTRAINT FK_SRI_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

-- الفواتير المعلقة (Hold/Park)
CREATE TABLE HeldSales (
    HeldSaleId INT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL,
    CashierUserId INT NOT NULL,
    HoldName NVARCHAR(100),                    -- اسم اختياري (مثلاً: "العميل اللي لابس أزرق")
    CustomerId INT,
    ItemsJson NVARCHAR(MAX) NOT NULL,          -- بيانات السلة كـ JSON
    SubTotal DECIMAL(18,2),
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_HeldSales_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_HeldSales_Users FOREIGN KEY (CashierUserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_SalesInvoices_BrandId ON SalesInvoices(BrandId);
CREATE INDEX IX_SalesInvoices_BranchId ON SalesInvoices(BranchId);
CREATE INDEX IX_SalesInvoices_Date ON SalesInvoices(CreatedAt);
CREATE INDEX IX_SalesInvoices_CustomerId ON SalesInvoices(CustomerId);
CREATE INDEX IX_SalesInvoiceItems_InvoiceId ON SalesInvoiceItems(InvoiceId);
GO

-- =====================================================
-- SECTION 8: PROMOTIONS & DISCOUNTS (نظام العروض المرن)
-- نظام العروض والخصومات
-- =====================================================

-- العروض الرئيسية
CREATE TABLE Promotions (
    PromotionId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    PromotionName NVARCHAR(200) NOT NULL,      -- مثلاً: "اشتري 2 خد 1 مجاناً"
    Description NVARCHAR(500),
    -- نوع العرض
    PromotionType NVARCHAR(50) NOT NULL,       
    -- الأنواع المدعومة:
    -- 'BuyXGetY'              : اشتري X خد Y مجاناً
    -- 'BuyXGetYDiscount'      : اشتري X الـ Y التاني خصم %
    -- 'PercentageDiscount'    : خصم نسبة على المنتج
    -- 'FixedDiscount'         : خصم مبلغ ثابت على المنتج
    -- 'BundleDiscount'        : خصم على مجموعة منتجات
    -- 'CategoryDiscount'      : خصم على تصنيف كامل
    -- 'MinPurchaseDiscount'   : خصم لو الفاتورة فوق مبلغ
    -- 'SecondItemDiscount'    : التيشيرت التاني بسعر مختلف
    -- 'FreeShipping'          : شحن مجاني
    -- 'LoyaltyPoints'         : نقاط مضاعفة
    -- 'CustomCustomerType'    : خصم لنوع عميل (VIP)
    
    -- إعدادات العرض
    BuyQuantity INT,                           -- كم منتج يشتري
    GetQuantity INT,                           -- كم منتج ياخد
    DiscountValue DECIMAL(18,2),               -- قيمة الخصم (نسبة أو مبلغ)
    DiscountType NVARCHAR(20),                 -- Percentage, Fixed
    MaxDiscountAmount DECIMAL(18,2),           -- حد أقصى للخصم
    MinPurchaseAmount DECIMAL(18,2),           -- أقل مبلغ للفاتورة
    -- نطاق التطبيق
    AppliesTo NVARCHAR(20) DEFAULT 'Specific', -- Specific, Category, AllProducts
    -- الفترة
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    -- الأيام والساعات
    DaysOfWeek NVARCHAR(20),                   -- "1,2,3,4,5,6,7" (1=الأحد)
    StartTime TIME,
    EndTime TIME,
    -- حدود الاستخدام
    MaxUsageTotal INT,                         -- الحد الأقصى لاستخدام العرض
    MaxUsagePerCustomer INT,                   -- الحد الأقصى للعميل الواحد
    UsageCount INT DEFAULT 0,                  -- عداد الاستخدام
    -- قنوات التطبيق
    ApplyToPOS BIT DEFAULT 1,
    ApplyToOnline BIT DEFAULT 1,
    -- اشتراك مع عروض تانية
    CombineWithOthers BIT DEFAULT 0,
    Priority INT DEFAULT 0,                    -- لو اتنين عرض ينفعوا، الأعلى يطبق
    -- كوبون
    RequiresCoupon BIT DEFAULT 0,
    CouponCode NVARCHAR(50),
    -- نوع العملاء
    CustomerType NVARCHAR(20),                 -- All, Retail, Wholesale, VIP
    NewCustomersOnly BIT DEFAULT 0,
    -- حالة
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedBy INT,
    CONSTRAINT FK_Promotions_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- المنتجات المشمولة في العرض (لو "Specific")
CREATE TABLE PromotionProducts (
    PromotionProductId INT IDENTITY(1,1) PRIMARY KEY,
    PromotionId INT NOT NULL,
    ProductId INT,                             -- لو العرض على منتج معين
    VariantId INT,                             -- لو العرض على variant معين
    CategoryId INT,                            -- لو العرض على تصنيف
    ProductRole NVARCHAR(20) DEFAULT 'Both',   -- Buy, Get, Both
    CONSTRAINT FK_PromotionProducts_Promotions FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId) ON DELETE CASCADE,
    CONSTRAINT FK_PromotionProducts_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_PromotionProducts_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT FK_PromotionProducts_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);

-- الفروع المشمولة (ممكن عرض في فروع معينة بس)
CREATE TABLE PromotionBranches (
    PromotionBranchId INT IDENTITY(1,1) PRIMARY KEY,
    PromotionId INT NOT NULL,
    BranchId INT NOT NULL,
    CONSTRAINT FK_PB_Promotions FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId) ON DELETE CASCADE,
    CONSTRAINT FK_PB_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId)
);

-- الكوبونات
CREATE TABLE Coupons (
    CouponId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CouponCode NVARCHAR(50) NOT NULL,
    PromotionId INT,                           -- مرتبط بعرض
    DiscountType NVARCHAR(20),                 -- Percentage, Fixed, FreeShipping
    DiscountValue DECIMAL(18,2),
    MaxDiscountAmount DECIMAL(18,2),
    MinPurchaseAmount DECIMAL(18,2),
    UsageLimit INT,
    UsageCount INT DEFAULT 0,
    PerCustomerLimit INT DEFAULT 1,
    StartDate DATETIME2,
    EndDate DATETIME2,
    AssignedToCustomerId INT,                  -- كوبون شخصي لعميل معين
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Coupons_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Coupons_Promotions FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId),
    CONSTRAINT UQ_Coupons UNIQUE (BrandId, CouponCode)
);

-- استخدامات الكوبون
CREATE TABLE CouponUsages (
    UsageId INT IDENTITY(1,1) PRIMARY KEY,
    CouponId INT NOT NULL,
    InvoiceId INT,
    OnlineOrderId INT,
    CustomerId INT,
    DiscountApplied DECIMAL(18,2),
    UsedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_CouponUsages_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(CouponId)
);

CREATE INDEX IX_Promotions_BrandId_Active ON Promotions(BrandId, IsActive);
CREATE INDEX IX_Promotions_Dates ON Promotions(StartDate, EndDate);
CREATE INDEX IX_Coupons_Code ON Coupons(CouponCode);
GO
