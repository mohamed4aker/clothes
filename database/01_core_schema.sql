-- =====================================================
-- MAS - نظام إدارة الأعمال المتكامل
-- Multi-Brand POS, Manufacturing & E-Commerce System
-- Database Schema - SQL Server 2019+
-- Created by: MAS Software
-- =====================================================

-- إنشاء قاعدة البيانات
IF DB_ID('MAS_DB') IS NULL
BEGIN
    CREATE DATABASE MAS_DB
    COLLATE Arabic_CI_AS;
END
GO

USE MAS_DB;
GO

-- =====================================================
-- SECTION 1: OWNERSHIP & MULTI-TENANCY
-- ملاك النظام والكيانات الأساسية
-- =====================================================

-- ملاك النظام (لو بتأجر النظام كـ SaaS)
CREATE TABLE Owners (
    OwnerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    PasswordHash NVARCHAR(500) NOT NULL,
    CompanyName NVARCHAR(200),
    SubscriptionPlan NVARCHAR(50) DEFAULT 'Trial', -- Trial, Basic, Pro, Enterprise
    SubscriptionStartDate DATETIME2,
    SubscriptionExpiry DATETIME2,
    MaxBrands INT DEFAULT 1,
    MaxBranches INT DEFAULT 1,
    MaxUsers INT DEFAULT 5,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    LastLoginAt DATETIME2
);

-- البراندات (كل عميل ممكن يكون له أكتر من براند)
CREATE TABLE Brands (
    BrandId INT IDENTITY(1,1) PRIMARY KEY,
    OwnerId INT NOT NULL,
    BrandName NVARCHAR(200) NOT NULL,
    BrandNameEn NVARCHAR(200),
    BusinessType NVARCHAR(50) NOT NULL, -- Clothing, VeterinaryPharma, Manufacturing, Generic, Restaurant, Pharmacy
    LogoUrl NVARCHAR(500),
    Phone NVARCHAR(20),
    Email NVARCHAR(200),
    Address NVARCHAR(500),
    TaxNumber NVARCHAR(50),                    -- الرقم الضريبي
    CommercialRegister NVARCHAR(50),           -- السجل التجاري
    Currency NVARCHAR(10) DEFAULT 'EGP',
    DefaultTaxRate DECIMAL(5,2) DEFAULT 14.00, -- ضريبة القيمة المضافة
    -- إعدادات البيع الأونلاين
    HasOnlineStore BIT DEFAULT 0,
    OnlineStoreSlug NVARCHAR(100) UNIQUE,      -- e.g., "zaid" → zaid.maspos.com
    OnlineStoreDomain NVARCHAR(200),           -- custom domain
    -- إعدادات عامة
    AllowNegativeStock BIT DEFAULT 0,
    DefaultLanguage NVARCHAR(10) DEFAULT 'ar',
    FiscalYearStartMonth INT DEFAULT 1,        -- بداية السنة المالية (1=يناير)
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_Brands_Owners FOREIGN KEY (OwnerId) REFERENCES Owners(OwnerId)
);

-- الفروع (كل براند ممكن يكون له فروع متعددة)
CREATE TABLE Branches (
    BranchId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchName NVARCHAR(200) NOT NULL,
    BranchCode NVARCHAR(20),                   -- كود الفرع (مثلاً: ALX01)
    Phone NVARCHAR(20),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    Governorate NVARCHAR(100),
    Latitude DECIMAL(10,7),
    Longitude DECIMAL(10,7),
    OpenTime TIME,
    CloseTime TIME,
    IsMainBranch BIT DEFAULT 0,                -- الفرع الرئيسي
    IsWarehouse BIT DEFAULT 0,                 -- مخزن (مش فرع بيع)
    AllowOnlineFulfillment BIT DEFAULT 1,      -- يقدر يجهز طلبات أونلاين
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_Branches_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

CREATE INDEX IX_Brands_OwnerId ON Brands(OwnerId);
CREATE INDEX IX_Branches_BrandId ON Branches(BrandId);
GO

-- =====================================================
-- SECTION 2: USERS & PERMISSIONS
-- المستخدمين والصلاحيات
-- =====================================================

-- المستخدمين
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200),
    Phone NVARCHAR(20),
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(50) NOT NULL,                -- Owner, BrandAdmin, BranchManager, Cashier, Warehouse, Accountant, ProductionManager, OnlineSales
    PinCode NVARCHAR(10),                      -- كود سريع للـ POS
    AvatarUrl NVARCHAR(500),
    Has2FA BIT DEFAULT 0,
    TwoFactorSecret NVARCHAR(200),
    LastLoginAt DATETIME2,
    LastLoginIp NVARCHAR(50),
    FailedLoginAttempts INT DEFAULT 0,
    LockedUntil DATETIME2,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_Users_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT UQ_Users_BrandId_Username UNIQUE (BrandId, Username)
);

-- ربط المستخدم بالفروع (يوزر واحد ممكن يدخل أكتر من فرع)
CREATE TABLE UserBranches (
    UserBranchId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    BranchId INT NOT NULL,
    IsDefault BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_UserBranches_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserBranches_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT UQ_UserBranches UNIQUE (UserId, BranchId)
);

-- صلاحيات دقيقة لكل مستخدم
CREATE TABLE UserPermissions (
    PermissionId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Module NVARCHAR(50) NOT NULL,              -- Products, Sales, Inventory, Reports, etc.
    Action NVARCHAR(50) NOT NULL,              -- View, Create, Edit, Delete, Approve
    IsAllowed BIT DEFAULT 1,
    CONSTRAINT FK_UserPermissions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_UserPermissions UNIQUE (UserId, Module, Action)
);

CREATE INDEX IX_Users_BrandId ON Users(BrandId);
CREATE INDEX IX_UserBranches_UserId ON UserBranches(UserId);
GO

-- =====================================================
-- SECTION 3: PRODUCTS & CATEGORIES
-- المنتجات والتصنيفات
-- =====================================================

-- التصنيفات (هرمية - تصنيف داخل تصنيف)
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    ParentCategoryId INT NULL,                 -- للتصنيفات الفرعية
    CategoryName NVARCHAR(200) NOT NULL,
    CategoryNameEn NVARCHAR(200),
    Description NVARCHAR(500),
    ImageUrl NVARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    ShowOnline BIT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_Categories_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES Categories(CategoryId)
);

-- وحدات القياس
CREATE TABLE Units (
    UnitId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    UnitName NVARCHAR(50) NOT NULL,            -- متر، كيلو، قطعة، علبة
    UnitNameEn NVARCHAR(50),
    UnitSymbol NVARCHAR(10),                   -- m, kg, pc
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Units_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- المنتجات (الأساسية)
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    CategoryId INT,
    SKU NVARCHAR(50) NOT NULL,
    Barcode NVARCHAR(50),
    ProductName NVARCHAR(300) NOT NULL,
    ProductNameEn NVARCHAR(300),
    Description NVARCHAR(MAX),
    DescriptionEn NVARCHAR(MAX),
    BaseCostPrice DECIMAL(18,2) DEFAULT 0,     -- سعر التكلفة
    BaseSellingPrice DECIMAL(18,2) DEFAULT 0,  -- سعر البيع
    WholesalePrice DECIMAL(18,2),              -- سعر الجملة
    OnlinePrice DECIMAL(18,2),                 -- سعر مختلف للأونلاين
    TaxRate DECIMAL(5,2) DEFAULT 14.00,
    BaseUnitId INT,
    MinStock DECIMAL(18,3) DEFAULT 0,          -- حد إنذار المخزون
    MaxStock DECIMAL(18,3),
    -- نوع المنتج
    ProductType NVARCHAR(50) DEFAULT 'Standard', -- Standard, Manufactured, RawMaterial, Service
    HasVariants BIT DEFAULT 0,                 -- له variants ولا منتج بسيط
    HasExpiry BIT DEFAULT 0,                   -- له تاريخ صلاحية
    HasBatchTracking BIT DEFAULT 0,            -- يتتبع بـ batch
    -- بيانات الأونلاين
    ShowOnline BIT DEFAULT 0,
    OnlineFeatured BIT DEFAULT 0,              -- منتج مميز
    OnlineSlug NVARCHAR(200),                  -- URL friendly
    -- صور
    MainImageUrl NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'Active',      -- Active, Inactive, Discontinued
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedBy INT,
    CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT FK_Products_Units FOREIGN KEY (BaseUnitId) REFERENCES Units(UnitId),
    CONSTRAINT UQ_Products_BrandId_SKU UNIQUE (BrandId, SKU)
);

-- صور المنتج (متعددة)
CREATE TABLE ProductImages (
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    IsMain BIT DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

-- خصائص الـ Variants (لون، مقاس، إلخ)
CREATE TABLE VariantAttributes (
    AttributeId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    AttributeName NVARCHAR(100) NOT NULL,      -- اللون، المقاس، الحجم
    AttributeNameEn NVARCHAR(100),
    DisplayOrder INT DEFAULT 0,
    CONSTRAINT FK_VariantAttributes_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- قيم الخصائص (أحمر، أزرق، S، M، L)
CREATE TABLE VariantAttributeValues (
    ValueId INT IDENTITY(1,1) PRIMARY KEY,
    AttributeId INT NOT NULL,
    ValueName NVARCHAR(100) NOT NULL,
    ValueNameEn NVARCHAR(100),
    ColorHex NVARCHAR(10),                     -- لو القيمة لون، يتخزن كـ #FF0000
    DisplayOrder INT DEFAULT 0,
    CONSTRAINT FK_VAV_Attributes FOREIGN KEY (AttributeId) REFERENCES VariantAttributes(AttributeId) ON DELETE CASCADE
);

-- المنتجات المتنوعة (Variants) - مثلاً: تيشيرت أحمر مقاس L
CREATE TABLE ProductVariants (
    VariantId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    SKU NVARCHAR(50) NOT NULL,
    Barcode NVARCHAR(50),
    VariantName NVARCHAR(300),                 -- مثلاً: تيشيرت أحمر-L
    CostPrice DECIMAL(18,2),                   -- ممكن يكون مختلف عن السعر الأساسي
    SellingPrice DECIMAL(18,2),
    OnlinePrice DECIMAL(18,2),
    WeightGrams DECIMAL(10,2),                 -- وزن للشحن
    ImageUrl NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT UQ_ProductVariants_SKU UNIQUE (SKU)
);

-- ربط الـ Variant بقيم الخصائص
CREATE TABLE VariantValueMappings (
    MappingId INT IDENTITY(1,1) PRIMARY KEY,
    VariantId INT NOT NULL,
    ValueId INT NOT NULL,
    CONSTRAINT FK_VVM_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId) ON DELETE CASCADE,
    CONSTRAINT FK_VVM_Values FOREIGN KEY (ValueId) REFERENCES VariantAttributeValues(ValueId)
);

-- وحدات بديلة للبيع (مثلاً: المنتج الأساسي بالمتر، والبيع كمان بنص متر)
CREATE TABLE ProductSellingUnits (
    SellingUnitId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UnitId INT NOT NULL,
    ConversionFactor DECIMAL(18,4) NOT NULL,   -- 1 متر = 100 سم → factor = 100
    PriceMultiplier DECIMAL(18,4) DEFAULT 1,   -- لو السعر مختلف
    IsDefault BIT DEFAULT 0,
    CONSTRAINT FK_PSU_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_PSU_Units FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
);

CREATE INDEX IX_Products_BrandId ON Products(BrandId);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_Barcode ON Products(Barcode);
CREATE INDEX IX_ProductVariants_ProductId ON ProductVariants(ProductId);
CREATE INDEX IX_ProductVariants_Barcode ON ProductVariants(Barcode);
GO
