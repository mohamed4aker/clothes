-- =====================================================
-- SECTION 11: ACCOUNTING & TREASURY
-- المحاسبة والخزن
-- =====================================================

USE MAS_DB;
GO

-- الخزن (لكل فرع ممكن يكون له أكتر من خزنة)
CREATE TABLE Treasuries (
    TreasuryId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT,                              -- ممكن خزنة عامة للبراند
    TreasuryName NVARCHAR(200) NOT NULL,       -- خزنة الفرع، خزنة المالية
    TreasuryType NVARCHAR(20) DEFAULT 'Cash',  -- Cash, Bank, EWallet
    AccountNumber NVARCHAR(100),               -- لو حساب بنكي
    BankName NVARCHAR(200),
    IBAN NVARCHAR(50),
    CurrentBalance DECIMAL(18,2) DEFAULT 0,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Treasuries_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Treasuries_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId)
);

-- حركات الخزن
CREATE TABLE TreasuryTransactions (
    TransactionId BIGINT IDENTITY(1,1) PRIMARY KEY,
    TreasuryId INT NOT NULL,
    TransactionType NVARCHAR(50) NOT NULL,     -- Sale, Purchase, Expense, Income, Transfer, Refund, Salary
    Direction NVARCHAR(10) NOT NULL,           -- In, Out
    Amount DECIMAL(18,2) NOT NULL,
    BalanceAfter DECIMAL(18,2),
    Description NVARCHAR(500),
    -- المرجع
    ReferenceType NVARCHAR(50),                -- SalesInvoice, PurchaseInvoice, Expense, etc.
    ReferenceId INT,
    -- لو تحويل بين خزن
    ToTreasuryId INT,
    UserId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_TT_Treasuries FOREIGN KEY (TreasuryId) REFERENCES Treasuries(TreasuryId),
    CONSTRAINT FK_TT_ToTreasuries FOREIGN KEY (ToTreasuryId) REFERENCES Treasuries(TreasuryId),
    CONSTRAINT FK_TT_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- تصنيفات المصاريف
CREATE TABLE ExpenseCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CategoryName NVARCHAR(200) NOT NULL,       -- إيجار، كهربا، رواتب، نقل
    ParentCategoryId INT,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_EC_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_EC_Parent FOREIGN KEY (ParentCategoryId) REFERENCES ExpenseCategories(CategoryId)
);

-- المصاريف
CREATE TABLE Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT,
    ExpenseNumber NVARCHAR(50),
    CategoryId INT NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    TreasuryId INT,                            -- اتصرف من أنهي خزنة
    ReceiptNumber NVARCHAR(100),
    AttachmentUrl NVARCHAR(500),
    ExpenseDate DATE NOT NULL,
    CreatedBy INT,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Expenses_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Expenses_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_Expenses_Categories FOREIGN KEY (CategoryId) REFERENCES ExpenseCategories(CategoryId),
    CONSTRAINT FK_Expenses_Treasuries FOREIGN KEY (TreasuryId) REFERENCES Treasuries(TreasuryId)
);

-- مدفوعات الموردين
CREATE TABLE SupplierPayments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    SupplierId INT NOT NULL,
    PurchaseInvoiceId INT,                     -- لو مرتبط بفاتورة معينة
    PaymentNumber NVARCHAR(50),
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    TreasuryId INT,
    PaymentDate DATE NOT NULL,
    ReferenceNumber NVARCHAR(100),
    Notes NVARCHAR(500),
    CreatedBy INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_SP_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_SP_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_SP_Invoices FOREIGN KEY (PurchaseInvoiceId) REFERENCES PurchaseInvoices(InvoiceId),
    CONSTRAINT FK_SP_Treasuries FOREIGN KEY (TreasuryId) REFERENCES Treasuries(TreasuryId)
);

-- مقبوضات العملاء (للعملاء الآجلين)
CREATE TABLE CustomerPayments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CustomerId INT NOT NULL,
    SalesInvoiceId INT,
    PaymentNumber NVARCHAR(50),
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    TreasuryId INT,
    PaymentDate DATE NOT NULL,
    ReferenceNumber NVARCHAR(100),
    Notes NVARCHAR(500),
    CreatedBy INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_CP_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_CP_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_CP_Invoices FOREIGN KEY (SalesInvoiceId) REFERENCES SalesInvoices(InvoiceId),
    CONSTRAINT FK_CP_Treasuries FOREIGN KEY (TreasuryId) REFERENCES Treasuries(TreasuryId)
);

-- السنة المالية
CREATE TABLE FiscalYears (
    FiscalYearId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    YearName NVARCHAR(50) NOT NULL,            -- 2025، 2025-2026
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Open',        -- Open, Closed
    ClosedAt DATETIME2,
    ClosedBy INT,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_FY_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

CREATE INDEX IX_TT_Treasury ON TreasuryTransactions(TreasuryId);
CREATE INDEX IX_TT_Date ON TreasuryTransactions(CreatedAt);
CREATE INDEX IX_Expenses_Brand_Date ON Expenses(BrandId, ExpenseDate);
GO

-- =====================================================
-- SECTION 12: NOTIFICATIONS & AUDIT LOG
-- الإشعارات وسجل التدقيق
-- =====================================================

CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT,
    UserId INT,                                -- لو موجه لمستخدم معين
    NotificationType NVARCHAR(50) NOT NULL,    -- LowStock, NewOrder, ExpiryAlert, etc.
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX),
    Severity NVARCHAR(20) DEFAULT 'Info',      -- Info, Warning, Error, Success
    -- المرجع
    ReferenceType NVARCHAR(50),                -- Product, Order, Invoice
    ReferenceId INT,
    -- الحالة
    IsRead BIT DEFAULT 0,
    ReadAt DATETIME2,
    -- القنوات اللي اتبعت عليها
    SentInApp BIT DEFAULT 1,
    SentEmail BIT DEFAULT 0,
    SentWhatsApp BIT DEFAULT 0,
    SentSMS BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Notifications_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- إعدادات الإشعارات لكل مستخدم
CREATE TABLE NotificationSettings (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL,
    EnableInApp BIT DEFAULT 1,
    EnableEmail BIT DEFAULT 0,
    EnableWhatsApp BIT DEFAULT 0,
    EnableSMS BIT DEFAULT 0,
    CONSTRAINT FK_NS_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_NS UNIQUE (UserId, NotificationType)
);

-- سجل التدقيق (Audit Log)
CREATE TABLE AuditLogs (
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT,
    UserId INT,
    Action NVARCHAR(100) NOT NULL,             -- Login, Create, Update, Delete, Export, etc.
    Module NVARCHAR(50),                       -- Products, Sales, Users, etc.
    EntityType NVARCHAR(100),
    EntityId INT,
    Description NVARCHAR(500),
    OldValues NVARCHAR(MAX),                   -- JSON - القيم القديمة
    NewValues NVARCHAR(MAX),                   -- JSON - القيم الجديدة
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_AL_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_AL_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_Notifications_User_Read ON Notifications(UserId, IsRead);
CREATE INDEX IX_AuditLogs_Brand_Date ON AuditLogs(BrandId, CreatedAt);
CREATE INDEX IX_AuditLogs_User ON AuditLogs(UserId);
GO

-- =====================================================
-- SECTION 13: USER LIMITS & SEED DATA
-- حدود الاستخدام وبيانات أولية
-- =====================================================

-- إضافة Constraint على عدد المستخدمين (الحد الأقصى 10 يوزر لكل عميل)
-- ده هيتطبق في الكود وكمان في الـ SQL
GO

CREATE OR ALTER TRIGGER trg_LimitUsersPerOwner
ON Users
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OwnerId INT, @MaxUsers INT, @CurrentCount INT;
    
    SELECT @OwnerId = b.OwnerId, @MaxUsers = ISNULL(o.MaxUsers, 10)
    FROM inserted i
    INNER JOIN Brands b ON i.BrandId = b.BrandId
    INNER JOIN Owners o ON b.OwnerId = o.OwnerId;
    
    SELECT @CurrentCount = COUNT(*)
    FROM Users u
    INNER JOIN Brands b ON u.BrandId = b.BrandId
    WHERE b.OwnerId = @OwnerId AND u.IsActive = 1;
    
    IF @CurrentCount > @MaxUsers
    BEGIN
        RAISERROR ('تم الوصول للحد الأقصى من المستخدمين (10 مستخدمين)', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- =====================================================
-- بيانات أولية (Seed Data)
-- =====================================================

-- وحدات قياس افتراضية (يتم نسخها لكل براند جديد)
-- هتحط البيانات الافتراضية عبر الكود C# عند إنشاء براند جديد

-- أنواع الإشعارات الأساسية
-- LowStock, OutOfStock, ExpiryAlert, NewOnlineOrder, OrderPaid, 
-- DailySummary, ProductionCompleted, ReturnRequest, LargeDiscount, SyncFailed

PRINT 'تم إنشاء قاعدة بيانات MAS بنجاح';
PRINT 'إجمالي الجداول المنشأة: 50+';
GO
