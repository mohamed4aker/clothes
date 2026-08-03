-- =====================================================
-- شغّل ده لو الـ seeder كان اشتغل قبل التعديل (كان بيكتب أرقام كنص زي "1")
-- ده بيمسح الداتا المزروعة بس عشان الـ seeder يعيد إنشاءها بالنص الصح
-- (لو الداتابيز لسه جديدة ومحصلش login نهائي، ممكن تتجاهله)
-- البديل الأنضف: DROP DATABASE MAS_DB ثم إعادة تشغيل 01..06
-- =====================================================
USE MAS_DB;
GO

DELETE FROM dbo.UserBranches;
DELETE FROM dbo.Users;
DELETE FROM dbo.Branches;
DELETE FROM dbo.Brands;
DELETE FROM dbo.Owners;
GO
-- بعد كده شغّل المشروع، والـ seeder هيعمل admin/owner من جديد بقيم نصية صح
