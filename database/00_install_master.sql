-- =====================================================
-- MAS Database - Master Installation Script
-- شغل الملف ده عشان يثبت كل قاعدة البيانات
-- =====================================================

-- الترتيب مهم - شغل الملفات بالترتيب ده:

-- 1. شغل ملف 01_core_schema.sql
--    (الجداول الأساسية: Owners, Brands, Branches, Users, Products, Categories)

-- 2. شغل ملف 02_inventory_schema.sql  
--    (المخزون والباتشات والموردين والعملاء)

-- 3. شغل ملف 03_sales_promotions_schema.sql
--    (المبيعات، الكاشير، المرتجعات، العروض، الكوبونات)

-- 4. شغل ملف 04_manufacturing_schema.sql
--    (التصنيع، BOM، أوامر الإنتاج، الهالك)

-- 5. شغل ملف 05_ecommerce_schema.sql
--    (المتجر الإلكتروني، الطلبات الأونلاين، الشحن)

-- 6. شغل ملف 06_accounting_audit_schema.sql
--    (المحاسبة، الخزن، المصاريف، الإشعارات، Audit Log)

-- =====================================================
-- بعد التثبيت، شغل السكريبت ده عشان تتأكد إن كل حاجة تمام
-- =====================================================

USE MAS_DB;
GO

-- عدد الجداول
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    COUNT(*) AS TableCount
FROM sys.tables
WHERE is_ms_shipped = 0
GROUP BY schema_id;

-- قائمة كل الجداول
SELECT 
    t.name AS TableName,
    p.rows AS [RowCount],
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.is_ms_shipped = 0
GROUP BY t.name, p.rows
ORDER BY t.name;

-- التحقق من العلاقات (Foreign Keys)
SELECT 
    f.name AS ForeignKeyName,
    OBJECT_NAME(f.parent_object_id) AS TableName,
    OBJECT_NAME(f.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys f
ORDER BY TableName;
