-- =====================================================
-- SECTION 9: MANUFACTURING (التصنيع)
-- =====================================================

USE MAS_DB;
GO

-- قائمة المكونات (Bill of Materials) - الوصفة لكل منتج مصنّع
CREATE TABLE BillOfMaterials (
    BomId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    ProductId INT NOT NULL,                    -- المنتج النهائي
    VariantId INT,                             -- لو variant محدد
    BomName NVARCHAR(200) NOT NULL,
    Version NVARCHAR(20) DEFAULT '1.0',
    EffectiveDate DATE NOT NULL,
    EndDate DATE,                              -- لو في نسخة أحدث
    -- التكاليف الإضافية (مش الخامات)
    DirectLaborCost DECIMAL(18,2) DEFAULT 0,   -- تكلفة العمالة المباشرة
    OverheadCost DECIMAL(18,2) DEFAULT 0,      -- تكاليف غير مباشرة (كهربا، إيجار)
    AdminExpenses DECIMAL(18,2) DEFAULT 0,     -- مصاريف إدارية
    OtherCosts DECIMAL(18,2) DEFAULT 0,        -- مصاريف أخرى
    OtherCostsDescription NVARCHAR(500),
    -- الهالك المتوقع
    ExpectedWastePercent DECIMAL(5,2) DEFAULT 0, -- نسبة الهالك المتوقع
    -- التكلفة الكلية المحسوبة (محسوبة من الترجر)
    TotalMaterialsCost DECIMAL(18,2) DEFAULT 0,
    TotalCost DECIMAL(18,2) DEFAULT 0,         -- إجمالي تكلفة الوحدة
    SuggestedSellingPrice DECIMAL(18,2),       -- السعر المقترح
    TargetMarginPercent DECIMAL(5,2) DEFAULT 30, -- هامش الربح المستهدف
    -- إعدادات
    OutputQuantity DECIMAL(18,3) DEFAULT 1,    -- كم وحدة تطلع من الوصفة (عشان وصفات مجمعة)
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedBy INT,
    CONSTRAINT FK_BOM_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_BOM_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_BOM_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

-- مكونات الوصفة (الخامات)
CREATE TABLE BomItems (
    BomItemId INT IDENTITY(1,1) PRIMARY KEY,
    BomId INT NOT NULL,
    RawMaterialVariantId INT NOT NULL,         -- الخامة (variant من المنتجات نوع RawMaterial)
    Quantity DECIMAL(18,4) NOT NULL,           -- الكمية المطلوبة
    UnitId INT,                                -- وحدة القياس (متر، كيلو)
    UnitCost DECIMAL(18,2) NOT NULL,           -- تكلفة الوحدة وقت إنشاء الـ BOM
    LineCost DECIMAL(18,2) NOT NULL,           -- التكلفة الإجمالية للمكون (Quantity × UnitCost)
    IsCritical BIT DEFAULT 0,                  -- مكون أساسي مش ممكن استبداله
    Notes NVARCHAR(300),
    DisplayOrder INT DEFAULT 0,
    CONSTRAINT FK_BomItems_BOM FOREIGN KEY (BomId) REFERENCES BillOfMaterials(BomId) ON DELETE CASCADE,
    CONSTRAINT FK_BomItems_Variants FOREIGN KEY (RawMaterialVariantId) REFERENCES ProductVariants(VariantId),
    CONSTRAINT FK_BomItems_Units FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
);

-- أوامر الإنتاج
CREATE TABLE ProductionOrders (
    ProductionOrderId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT NOT NULL,                     -- الفرع/المخزن اللي بيتصنع فيه
    OrderNumber NVARCHAR(50) NOT NULL,
    BomId INT NOT NULL,                        -- الوصفة المستخدمة
    ProductId INT NOT NULL,
    VariantId INT,
    PlannedQuantity DECIMAL(18,3) NOT NULL,    -- الكمية المخططة
    ActualQuantity DECIMAL(18,3),              -- الكمية الفعلية اللي طلعت
    WasteQuantity DECIMAL(18,3) DEFAULT 0,     -- الهالك الفعلي
    Status NVARCHAR(20) DEFAULT 'Draft',       -- Draft, Approved, InProgress, Completed, Cancelled
    -- التكاليف
    TotalMaterialsCost DECIMAL(18,2) DEFAULT 0,
    TotalLaborCost DECIMAL(18,2) DEFAULT 0,
    TotalOverhead DECIMAL(18,2) DEFAULT 0,
    OtherCosts DECIMAL(18,2) DEFAULT 0,
    TotalProductionCost DECIMAL(18,2) DEFAULT 0,
    UnitProductionCost DECIMAL(18,2) DEFAULT 0, -- التكلفة الفعلية للوحدة
    -- التواريخ
    PlannedStartDate DATETIME2,
    PlannedEndDate DATETIME2,
    ActualStartDate DATETIME2,
    ActualEndDate DATETIME2,
    -- مسؤولين
    CreatedBy INT,
    StartedBy INT,
    CompletedBy INT,
    QualityCheckedBy INT,
    QualityStatus NVARCHAR(20),                -- NotChecked, Passed, Failed, PartialPass
    QualityNotes NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_PO_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_PO_Branches FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_PO_BOM FOREIGN KEY (BomId) REFERENCES BillOfMaterials(BomId),
    CONSTRAINT FK_PO_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT UQ_PO_Number UNIQUE (BrandId, OrderNumber)
);

-- استهلاك الخامات الفعلي في الإنتاج
CREATE TABLE ProductionMaterialUsage (
    UsageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductionOrderId INT NOT NULL,
    RawMaterialVariantId INT NOT NULL,
    PlannedQuantity DECIMAL(18,4) NOT NULL,    -- اللي كان مفروض حسب الـ BOM
    ActualQuantity DECIMAL(18,4),              -- اللي اتسحب فعلاً
    WasteQuantity DECIMAL(18,4) DEFAULT 0,     -- المهدر من الخامة دي
    UnitCost DECIMAL(18,2),
    TotalCost DECIMAL(18,2),
    BatchId INT,                               -- لو الخامة من باتش معين
    Notes NVARCHAR(300),
    CONSTRAINT FK_PMU_PO FOREIGN KEY (ProductionOrderId) REFERENCES ProductionOrders(ProductionOrderId) ON DELETE CASCADE,
    CONSTRAINT FK_PMU_Variants FOREIGN KEY (RawMaterialVariantId) REFERENCES ProductVariants(VariantId)
);

-- سجل الهالك التفصيلي
CREATE TABLE WasteRecords (
    WasteId INT IDENTITY(1,1) PRIMARY KEY,
    BrandId INT NOT NULL,
    BranchId INT NOT NULL,
    ProductionOrderId INT,                     -- لو الهالك من إنتاج
    VariantId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    UnitCost DECIMAL(18,2),
    TotalCost DECIMAL(18,2),
    WasteReason NVARCHAR(50),                  -- CuttingLoss, Defective, Damaged, Expired, Other
    ReasonDetails NVARCHAR(500),
    RecordedBy INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Waste_Brands FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Waste_PO FOREIGN KEY (ProductionOrderId) REFERENCES ProductionOrders(ProductionOrderId),
    CONSTRAINT FK_Waste_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId)
);

CREATE INDEX IX_BOM_BrandId ON BillOfMaterials(BrandId);
CREATE INDEX IX_BOM_ProductId ON BillOfMaterials(ProductId);
CREATE INDEX IX_PO_BrandId ON ProductionOrders(BrandId);
CREATE INDEX IX_PO_Status ON ProductionOrders(Status);
GO
