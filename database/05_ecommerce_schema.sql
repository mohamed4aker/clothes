-- =====================================================
-- SECTION 10: E-COMMERCE & ONLINE ORDERS
-- البيع الإلكتروني والطلبات الأونلاين
-- =====================================================

USE MAS_DB;
GO

-- إعدادات المتجر الإلكتروني لكل براند
CREATE TABLE OnlineStoreSettings (
    SettingsId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL UNIQUE,
    StoreName NVARCHAR(200) NOT NULL,
    StoreNameEn NVARCHAR(200),
    Slogan NVARCHAR(500),
    SloganEn NVARCHAR(500),
    LogoUrl NVARCHAR(500),
    FaviconUrl NVARCHAR(500),
    BannerUrl NVARCHAR(500),
    PrimaryColor NVARCHAR(10) DEFAULT '#0d9488',  -- لون أساسي
    SecondaryColor NVARCHAR(10),
    ContactEmail NVARCHAR(200),
    ContactPhone NVARCHAR(20),
    WhatsAppNumber NVARCHAR(20),
    -- روابط السوشيال ميديا
    FacebookUrl NVARCHAR(500),
    InstagramUrl NVARCHAR(500),
    TwitterUrl NVARCHAR(500),
    TikTokUrl NVARCHAR(500),
    -- صفحات
    AboutUsContent NVARCHAR(MAX),
    PrivacyPolicyContent NVARCHAR(MAX),
    TermsContent NVARCHAR(MAX),
    -- إعدادات الطلب
    EnableGuestCheckout BIT DEFAULT 1,
    RequirePhoneVerification BIT DEFAULT 1,
    AutoCancelHours INT DEFAULT 24,            -- إلغاء الطلب تلقائياً بعد كم ساعة لو ما تأكدش
    MinOrderAmount DECIMAL(18,2) DEFAULT 0,
    -- العملة والضريبة
    Currency NVARCHAR(10) DEFAULT 'EGP',
    DisplayPricesIncludeTax BIT DEFAULT 1,
    -- إعدادات SEO
    MetaTitle NVARCHAR(200),
    MetaDescription NVARCHAR(500),
    MetaKeywords NVARCHAR(500),
    GoogleAnalyticsId NVARCHAR(50),
    FacebookPixelId NVARCHAR(50),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_OSS_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- شركات الشحن
CREATE TABLE ShippingCompanies (
    ShippingCompanyId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,        -- Bosta, Aramex, مكتب الفرع نفسه
    ApiEndpoint NVARCHAR(500),                 -- لو فيه API
    ApiKey NVARCHAR(500),
    ApiSecret NVARCHAR(500),
    SupportsCOD BIT DEFAULT 1,                 -- بيدعم الدفع عند الاستلام
    AverageDeliveryDays INT,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_SC_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- مناطق الشحن وأسعارها
CREATE TABLE ShippingZones (
    ZoneId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    ZoneName NVARCHAR(200) NOT NULL,           -- القاهرة، الإسكندرية، الدلتا
    Governorates NVARCHAR(MAX),                -- "القاهرة,الجيزة" مفصول بفاصلة
    StandardShippingCost DECIMAL(18,2) NOT NULL,
    ExpressShippingCost DECIMAL(18,2),
    EstimatedDays INT,
    -- تقسيم الشحن بين البائع والمشتري
    CustomerPaysPercent DECIMAL(5,2) DEFAULT 100, -- % اللي العميل يدفعه
    SellerPaysPercent DECIMAL(5,2) DEFAULT 0,     -- % اللي يخصم من ربح البائع
    -- شحن مجاني فوق مبلغ
    FreeShippingThreshold DECIMAL(18,2),       -- لو الفاتورة فوق المبلغ ده، شحن مجاني
    FreeShippingSellerCost DECIMAL(18,2),      -- وقتها كم يتخصم من البائع
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_SZ_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- الطلبات الأونلاين
CREATE TABLE OnlineOrders (
    OnlineOrderId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    AssignedBranchId INT,                      -- الفرع اللي بيجهز الطلب
    OrderNumber NVARCHAR(50) NOT NULL,
    CustomerId INT,                            -- لو مسجل
    -- بيانات الزائر (لو guest checkout)
    GuestName NVARCHAR(200),
    GuestPhone NVARCHAR(20),
    GuestEmail NVARCHAR(200),
    -- العنوان
    DeliveryAddressId INT,
    DeliveryFullAddress NVARCHAR(500) NOT NULL,
    DeliveryCity NVARCHAR(100),
    DeliveryGovernorate NVARCHAR(100),
    DeliveryPhone NVARCHAR(20) NOT NULL,
    DeliveryNotes NVARCHAR(500),
    -- المبالغ
    SubTotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    PromotionDiscount DECIMAL(18,2) DEFAULT 0,
    CouponDiscount DECIMAL(18,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    ShippingCost DECIMAL(18,2) DEFAULT 0,      -- تكلفة الشحن الكلية
    CustomerShippingShare DECIMAL(18,2) DEFAULT 0,  -- اللي العميل بيدفعه
    SellerShippingShare DECIMAL(18,2) DEFAULT 0,    -- اللي يتخصم من البائع
    GrandTotal DECIMAL(18,2) NOT NULL,
    -- الدفع
    PaymentMethod NVARCHAR(50) NOT NULL,       -- COD, OnlineCard, BankTransfer, Wallet
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending', -- Pending, Paid, Failed, Refunded
    PaymentReference NVARCHAR(200),
    PaymentCompletedAt DATETIME2,
    -- الشحن
    ShippingCompanyId INT,
    ShippingMethod NVARCHAR(50),               -- Standard, Express, Pickup
    ShippingZoneId INT,
    TrackingNumber NVARCHAR(100),
    EstimatedDeliveryDate DATE,
    -- الحالة
    OrderStatus NVARCHAR(30) DEFAULT 'Pending', 
    -- Pending, Confirmed, Preparing, ReadyForShipment, Shipped, OutForDelivery, Delivered, Cancelled, Returned, Failed
    -- تواريخ تتبع الحالة
    ConfirmedAt DATETIME2,
    PreparedAt DATETIME2,
    ShippedAt DATETIME2,
    DeliveredAt DATETIME2,
    CancelledAt DATETIME2,
    CancellationReason NVARCHAR(500),
    -- تفاعلات
    AppliedCouponCode NVARCHAR(50),
    AppliedPromotionId INT,
    LoyaltyPointsUsed INT DEFAULT 0,
    LoyaltyPointsEarned INT DEFAULT 0,
    -- الفاتورة المرتبطة (لما الطلب يكتمل)
    SalesInvoiceId INT,
    -- بيانات إضافية
    CustomerNotes NVARCHAR(500),
    InternalNotes NVARCHAR(MAX),
    Source NVARCHAR(50) DEFAULT 'Website',     -- Website, Mobile, WhatsApp, Phone
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_OO_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_OO_Branches FOREIGN KEY (AssignedBranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_OO_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_OO_ShippingCompanies FOREIGN KEY (ShippingCompanyId) REFERENCES ShippingCompanies(ShippingCompanyId),
    CONSTRAINT FK_OO_Zones FOREIGN KEY (ShippingZoneId) REFERENCES ShippingZones(ZoneId),
    CONSTRAINT FK_OO_Invoices FOREIGN KEY (SalesInvoiceId) REFERENCES SalesInvoices(InvoiceId),
    CONSTRAINT UQ_OO_Number UNIQUE (BrandId, OrderNumber)
);

-- بنود الطلب الأونلاين
CREATE TABLE OnlineOrderItems (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    OnlineOrderId INT NOT NULL,
    VariantId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    PromotionDiscount DECIMAL(18,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL,
    AppliedPromotionId INT,
    -- المخزون المحجوز
    ReservedFromBranchId INT,                  -- محجوز من الفرع ده
    IsReserved BIT DEFAULT 1,
    ProductSnapshot NVARCHAR(MAX),             -- صورة بيانات المنتج وقت الطلب (JSON)
    CONSTRAINT FK_OOI_Orders FOREIGN KEY (OnlineOrderId) REFERENCES OnlineOrders(OnlineOrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OOI_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT FK_OOI_Branches FOREIGN KEY (ReservedFromBranchId) REFERENCES Branches(BranchId)
);

-- سجل تتبع حالة الطلب
CREATE TABLE OnlineOrderStatusHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    OnlineOrderId INT NOT NULL,
    OldStatus NVARCHAR(30),
    NewStatus NVARCHAR(30) NOT NULL,
    ChangedBy INT,                             -- لو موظف غيرها
    Notes NVARCHAR(500),
    ChangedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_OOSH_Orders FOREIGN KEY (OnlineOrderId) REFERENCES OnlineOrders(OnlineOrderId) ON DELETE CASCADE
);

-- سلة المتسوق (مؤقتة)
CREATE TABLE ShoppingCarts (
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CustomerId INT,                            -- لو مسجل دخول
    SessionId NVARCHAR(100),                   -- لو زائر
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    ExpiresAt DATETIME2,                       -- تنتهي بعد فترة
    CONSTRAINT FK_Cart_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Cart_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);

CREATE TABLE ShoppingCartItems (
    CartItemId INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL,
    VariantId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    AddedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_CartItems_Cart FOREIGN KEY (CartId) REFERENCES ShoppingCarts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

-- المفضلة
CREATE TABLE Wishlists (
    WishlistId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    VariantId INT NOT NULL,
    AddedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Wishlist_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlist_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT UQ_Wishlist UNIQUE (CustomerId, VariantId)
);

-- التقييمات والمراجعات
CREATE TABLE ProductReviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    CustomerId INT NOT NULL,
    OrderItemId INT,                           -- التحقق إن العميل اشترى المنتج
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Title NVARCHAR(200),
    Comment NVARCHAR(MAX),
    IsVerifiedPurchase BIT DEFAULT 0,
    IsApproved BIT DEFAULT 0,                  -- مراجعة الأدمن
    HelpfulCount INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Reviews_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);

CREATE INDEX IX_OO_BrandId_Status ON OnlineOrders(BrandId, OrderStatus);
CREATE INDEX IX_OO_CreatedAt ON OnlineOrders(CreatedAt);
CREATE INDEX IX_OO_Customer ON OnlineOrders(CustomerId);
CREATE INDEX IX_OOI_Order ON OnlineOrderItems(OnlineOrderId);
GO
