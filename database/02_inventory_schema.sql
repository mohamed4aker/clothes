-- =====================================================
-- SECTION 4: INVENTORY & STOCK MOVEMENTS
-- المخزون وحركاته
-- =====================================================

USE MAS_DB;
GO

-- المخزون لكل variant في كل فرع
CREATE TABLE Inventory (
    InventoryId INT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL,
    VariantId INT NOT NULL,                    -- لكل variant مخزون منفصل
    Quantity DECIMAL(18,3) DEFAULT 0,          -- الكمية المتاحة
    ReservedQuantity DECIMAL(18,3) DEFAULT 0,  -- محجوز لطلبات أونلاين
    AverageCost DECIMAL(18,2) DEFAULT 0,       -- متوسط التكلفة
    LastUpdated DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Inventory_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_Inventory_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT UQ_Inventory UNIQUE (BranchId, VariantId)
);

-- الباتشات (للأدوية والمنتجات اللي ليها تاريخ صلاحية)
CREATE TABLE Batches (
    BatchId INT IDENTITY(1,1) PRIMARY KEY,
    VariantId INT NOT NULL,
    BranchId INT NOT NULL,
    BatchNumber NVARCHAR(50) NOT NULL,
    ManufactureDate DATE,
    ExpiryDate DATE NOT NULL,
    InitialQuantity DECIMAL(18,3) NOT NULL,
    CurrentQuantity DECIMAL(18,3) NOT NULL,
    CostPrice DECIMAL(18,2),
    SupplierId INT,
    PurchaseInvoiceId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Batches_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT FK_Batches_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT UQ_Batches UNIQUE (VariantId, BranchId, BatchNumber)
);

-- حركة المخزون (سجل كل عملية)
CREATE TABLE StockMovements (
    MovementId BIGINT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL,
    VariantId INT NOT NULL,
    BatchId INT,                               -- لو في batch tracking
    MovementType NVARCHAR(50) NOT NULL,        -- Purchase, Sale, Return, TransferIn, TransferOut, ProductionIn, ProductionOut, Waste, Adjustment, OnlineReserve, OnlineRelease, OnlineFulfilled
    Quantity DECIMAL(18,3) NOT NULL,           -- موجب: داخل، سالب: خارج
    UnitCost DECIMAL(18,2),
    UnitPrice DECIMAL(18,2),
    BalanceAfter DECIMAL(18,3),                -- الرصيد بعد الحركة
    -- المرجع (مثلاً: رقم فاتورة البيع، رقم تحويل المخزون)
    ReferenceType NVARCHAR(50),
    ReferenceId INT,
    Notes NVARCHAR(500),
    UserId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_StockMovements_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_StockMovements_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT FK_StockMovements_Batches FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_StockMovements_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- تحويلات المخزون بين الفروع
CREATE TABLE StockTransfers (
    TransferId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    TransferNumber NVARCHAR(50) NOT NULL,
    FromBranchId INT NOT NULL,
    ToBranchId INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',     -- Pending, InTransit, Received, Cancelled
    SentAt DATETIME2,
    ReceivedAt DATETIME2,
    SentBy INT,
    ReceivedBy INT,
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_StockTransfers_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_StockTransfers_FromBranch FOREIGN KEY (FromBranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_StockTransfers_ToBranch FOREIGN KEY (ToBranchId) REFERENCES Branches(BranchId),
    CONSTRAINT UQ_StockTransfers_Number UNIQUE (BrandId, TransferNumber)
);

CREATE TABLE StockTransferItems (
    TransferItemId INT IDENTITY(1,1) PRIMARY KEY,
    TransferId INT NOT NULL,
    VariantId INT NOT NULL,
    BatchId INT,
    QuantitySent DECIMAL(18,3) NOT NULL,
    QuantityReceived DECIMAL(18,3),            -- ممكن تكون أقل لو في فرق
    UnitCost DECIMAL(18,2),
    Notes NVARCHAR(300),
    CONSTRAINT FK_STI_Transfers FOREIGN KEY (TransferId) REFERENCES StockTransfers(TransferId) ON DELETE CASCADE,
    CONSTRAINT FK_STI_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

-- جرد المخزون
CREATE TABLE StockTakes (
    StockTakeId INT IDENTITY(1,1) PRIMARY KEY,
    BranchId INT NOT NULL,
    StockTakeNumber NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'InProgress',  -- InProgress, Completed, Cancelled
    StartedAt DATETIME2 DEFAULT GETDATE(),
    CompletedAt DATETIME2,
    CreatedBy INT,
    Notes NVARCHAR(500),
    CONSTRAINT FK_StockTakes_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId)
);

CREATE TABLE StockTakeItems (
    StockTakeItemId INT IDENTITY(1,1) PRIMARY KEY,
    StockTakeId INT NOT NULL,
    VariantId INT NOT NULL,
    SystemQuantity DECIMAL(18,3) NOT NULL,     -- اللي في النظام
    ActualQuantity DECIMAL(18,3),              -- اللي اتعد فعلاً
    Difference AS (ActualQuantity - SystemQuantity), -- الفرق محسوب
    UnitCost DECIMAL(18,2),
    Notes NVARCHAR(300),
    CONSTRAINT FK_STI2_StockTakes FOREIGN KEY (StockTakeId) REFERENCES StockTakes(StockTakeId) ON DELETE CASCADE,
    CONSTRAINT FK_STI2_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

CREATE INDEX IX_Inventory_Branch_Variant ON Inventory(BranchId, VariantId);
CREATE INDEX IX_StockMovements_Branch_Variant ON StockMovements(BranchId, VariantId);
CREATE INDEX IX_StockMovements_CreatedAt ON StockMovements(CreatedAt);
CREATE INDEX IX_Batches_ExpiryDate ON Batches(ExpiryDate);
GO

-- =====================================================
-- SECTION 5: SUPPLIERS & PURCHASES
-- الموردين والمشتريات
-- =====================================================

CREATE TABLE Suppliers (
    SupplierId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    SupplierName NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(200),
    Phone NVARCHAR(20),
    Email NVARCHAR(200),
    Address NVARCHAR(500),
    TaxNumber NVARCHAR(50),
    OpeningBalance DECIMAL(18,2) DEFAULT 0,    -- الرصيد الافتتاحي
    CurrentBalance DECIMAL(18,2) DEFAULT 0,    -- الرصيد الحالي (موجب = ليه عندنا)
    PaymentTerms NVARCHAR(100),                -- شروط الدفع (نقدي، 30 يوم، إلخ)
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_Suppliers_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

CREATE TABLE PurchaseInvoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT NOT NULL,
    SupplierId INT NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    SupplierInvoiceNumber NVARCHAR(50),        -- رقم فاتورة المورد الأصلية
    InvoiceDate DATE NOT NULL,
    DueDate DATE,
    SubTotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    ShippingCost DECIMAL(18,2) DEFAULT 0,
    OtherCosts DECIMAL(18,2) DEFAULT 0,         -- جمارك، نقل، إلخ
    GrandTotal DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    Status NVARCHAR(20) DEFAULT 'Pending',     -- Pending, PartiallyPaid, Paid, Cancelled
    Notes NVARCHAR(MAX),
    CreatedBy INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_PurchaseInvoices_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_PurchaseInvoices_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_PurchaseInvoices_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT UQ_PurchaseInvoices UNIQUE (BrandId, InvoiceNumber)
);

CREATE TABLE PurchaseInvoiceItems (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    VariantId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    UnitCost DECIMAL(18,2) NOT NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    TaxRate DECIMAL(5,2) DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL,
    BatchNumber NVARCHAR(50),
    ExpiryDate DATE,
    CONSTRAINT FK_PII_Invoices FOREIGN KEY (InvoiceId) REFERENCES PurchaseInvoices(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_PII_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

CREATE INDEX IX_Suppliers_BrandId ON Suppliers(BrandId);
CREATE INDEX IX_PurchaseInvoices_BrandId ON PurchaseInvoices(BrandId);
CREATE INDEX IX_PurchaseInvoices_Date ON PurchaseInvoices(InvoiceDate);
GO

-- =====================================================
-- SECTION 6: CUSTOMERS (CRM)
-- العملاء
-- =====================================================

CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CustomerCode NVARCHAR(50),
    FullName NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20),
    AlternatePhone NVARCHAR(20),
    Email NVARCHAR(200),
    Gender NVARCHAR(10),
    BirthDate DATE,
    TaxNumber NVARCHAR(50),                    -- للعملاء الـ B2B
    CustomerType NVARCHAR(20) DEFAULT 'Retail', -- Retail, Wholesale, VIP, B2B
    LoyaltyPoints INT DEFAULT 0,
    TotalSpent DECIMAL(18,2) DEFAULT 0,
    OpeningBalance DECIMAL(18,2) DEFAULT 0,
    CurrentBalance DECIMAL(18,2) DEFAULT 0,    -- موجب: ليه علينا، سالب: علينا له
    CreditLimit DECIMAL(18,2) DEFAULT 0,
    -- الأونلاين
    HasOnlineAccount BIT DEFAULT 0,
    PasswordHash NVARCHAR(500),                -- لو عنده حساب أونلاين
    EmailVerified BIT DEFAULT 0,
    PhoneVerified BIT DEFAULT 0,
    -- مفضلات
    PreferredLanguage NVARCHAR(10) DEFAULT 'ar',
    AcceptMarketing BIT DEFAULT 1,
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    LastPurchaseAt DATETIME2,
    CONSTRAINT FK_Customers_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

CREATE TABLE CustomerAddresses (
    AddressId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    AddressType NVARCHAR(20) DEFAULT 'Home',   -- Home, Work, Other
    AddressTitle NVARCHAR(100),                -- مثلاً: "بيت الأهل"
    FullAddress NVARCHAR(500) NOT NULL,
    City NVARCHAR(100),
    Governorate NVARCHAR(100),
    PostalCode NVARCHAR(20),
    Latitude DECIMAL(10,7),
    Longitude DECIMAL(10,7),
    IsDefault BIT DEFAULT 0,
    DeliveryNotes NVARCHAR(500),
    CONSTRAINT FK_CustomerAddresses_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId) ON DELETE CASCADE
);

CREATE INDEX IX_Customers_BrandId ON Customers(BrandId);
CREATE INDEX IX_Customers_Phone ON Customers(Phone);
CREATE INDEX IX_Customers_Email ON Customers(Email);
GO
