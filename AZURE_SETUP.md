# النشر على Azure — خطوة بخطوة

الهدف: لينك شغال على النت، والنشر يبقى تلقائي مع كل push.

**اللي أنا عملته:** ملف النشر `.github/workflows/deploy-azure.yml` جاهز في الريبو.
**اللي انت هتعمله:** الخطوات اللي تحت — محتاجة حسابك انت، مقدرش أعملها بدالك.

الوقت المتوقع: **~20 دقيقة**.

---

## ⚠️ اقرا ده الأول — قيود الباقة المجانية

باقة **F1 (المجانية)** كويسة للتجربة والعرض على حد، لكن **مش مناسبة لمحل شغال**:

| القيد | القيمة | يعني إيه لنظامك |
|---|---|---|
| **اتصالات WebSocket** | **5 في نفس الوقت** | Blazor Server بياخد اتصال واحد لكل تاب مفتوح. **يعني 5 مستخدمين بالكتير في نفس اللحظة** |
| النوم | التطبيق بينام بعد ~20 دقيقة سكون | أول زائر بعد النوم بيستنى شوية لحد ما يقوم |
| وقت المعالجة | محدود يومياً | لو اتعدى، الموقع بيقف لحد بكرة |
| Always On | مش متاح في F1 | مفيش طريقة تمنع النوم |

للاستخدام الحقيقي قدام كاشير طول اليوم: **B1 (~$13/شهر)** أو **VPS (~$5/شهر)**.
الترقية بتبقى بضغطة زرار من نفس المكان من غير ما تغيّر أي حاجة في الكود.

---

## 1. اعمل حساب Azure

https://azure.microsoft.com/free

هيطلب منك بطاقة للتأكد من الهوية. **باقة F1 نفسها مجانية** ومش بتتحاسب،
بس خلي بالك ما تختارش باقة مدفوعة بالغلط.

---

## 2. اعمل قاعدة البيانات (Azure SQL)

من [portal.azure.com](https://portal.azure.com):

1. **Create a resource → SQL Database**
2. اعمل **Resource group** جديدة، سمّيها مثلاً `mas-rg`
3. **Database name**: `MAS_DB`
4. **Server**: اعمل سيرفر جديد — اختار منطقة قريبة (مثلاً West Europe)،
   وسجّل **اسم المستخدم والباسورد** في مكان آمن، هتحتاجهم بعدين
5. في **Compute + storage** دوّر على العرض المجاني
   (بيظهر باسم *Free offer* أو *General Purpose – Serverless*).
   **راجع الشروط الحالية بنفسك — العروض دي بتتغير.**
6. اعمل **Create** واستنى لحد ما يخلص

### افتح الجدار الناري
بعد ما السيرفر يجهز: **SQL server → Networking**
- علّم على **Allow Azure services and resources to access this server** ✅
- ضيف الـ IP بتاعك لو عايز تتصل من جهازك

### خد الـ Connection String
**SQL Database → Connection strings → ADO.NET**
انسخه، وحط الباسورد الحقيقي بدل `{your_password}`.

---

## 3. اعمل الـ App Service

1. **Create a resource → Web App**
2. **Resource group**: نفس `mas-rg`
3. **Name**: اسم فريد، مثلاً `mas-shop-2026` (ده هيبقى `mas-shop-2026.azurewebsites.net`)
4. **Publish**: `Code`
5. **Runtime stack**: `.NET 8 (LTS)`
6. **Operating System**: `Windows`
7. **Pricing plan**: دوس **Change size** واختار **Dev/Test → F1 (Free)**
8. **Create**

---

## 4. اضبط إعدادات التطبيق

### أ) الـ Connection String
**App Service → Settings → Environment variables → Connection strings → Add**

| الحقل | القيمة |
|---|---|
| Name | `DefaultConnection` |
| Value | الـ connection string من خطوة 2 |
| Type | `SQLAzure` |

> الاسم لازم يكون `DefaultConnection` بالظبط — ده اللي الكود بيدوّر عليه.

### ب) بيئة الإنتاج
في نفس الصفحة، تاب **App settings → Add**:

| Name | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### ج) فعّل WebSockets — **مهمة جداً**
**Settings → Configuration → General settings**
- **Web sockets**: `On` ✅
- **ARR affinity**: `On` ✅

> من غير الخطوة دي **الموقع مش هيشتغل خالص**. Blazor Server كله قايم على WebSockets.

دوس **Save**.

---

## 5. اربط GitHub بـ Azure

### أ) فعّل الدخول بالـ publish profile
**App Service → Settings → Configuration → General settings**
- **SCM Basic Auth Publishing Credentials**: `On`

(في الحسابات الجديدة دي بتبقى مقفولة افتراضياً، ومن غيرها النشر بيفشل.)

### ب) نزّل الـ publish profile
من الصفحة الرئيسية للـ App Service فوق: **Download publish profile**
هينزل ملف `.PublishSettings` — **افتحه بالنوتباد وانسخ محتواه كله**.

### ج) حط البيانات في GitHub

روح على الريبو → **Settings → Secrets and variables → Actions**

**تاب Secrets** → `New repository secret`:
| Name | Value |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | محتوى الملف اللي نسخته |

**تاب Variables** → `New repository variable`:
| Name | Value |
|---|---|
| `AZURE_WEBAPP_NAME` | اسم الـ App Service (مثلاً `mas-shop-2026`) |

---

## 6. شغّل النشر

روح على تاب **Actions** في الريبو → **Deploy to Azure** → **Run workflow**.

استنى 2-3 دقايق. لما يخلص، اللينك بتاعك:

```
https://<اسم-التطبيق>.azurewebsites.net
```

أول مرة تفتحه هياخد وقت أطول شوية — الكود بيعمل الداتابيز والجداول تلقائياً
(`EnsureCreated` + `DbSeeder`).

### الدخول
```
Username: admin
Password: admin
```

## 🔴 أول حاجة تعملها بعد أول دخول

**غيّر باسورد admin** من صفحة الموظفين.

الموقع بقى على النت ومفتوح لأي حد يعرف اللينك، و`admin`/`admin`
باسورد أي حد ممكن يخمّنه.

---

## بعد كده

النشر بقى تلقائي — أي push على البرانش، الموقع بيتحدّث لوحده.

---

## لو حصلت مشكلة

| العرض | السبب غالباً |
|---|---|
| صفحة بيضا أو الموقع بيقطع كل شوية | WebSockets مقفولة (خطوة 4-ج) |
| `Application Error` | الـ Connection String غلط أو الجدار الناري قافل (خطوة 2) |
| النشر بيفشل بخطأ 401 / 403 | SCM Basic Auth مقفولة (خطوة 5-أ) |
| النشر بيفشل بخطأ في الاسم | `AZURE_WEBAPP_NAME` مش مطابق لاسم الـ App Service |
| الموقع بطيء أول مرة | التطبيق كان نايم — طبيعي في F1 |
| بيقع لما أكتر من 5 ناس يفتحوه | حد الـ 5 اتصالات في F1 — لازم ترقية |

**اللوجات:** `App Service → Monitoring → Log stream`،
وكمان المشروع بيكتب أخطاءه في ملف `logs/errors.log` جنب ملفات التطبيق.
