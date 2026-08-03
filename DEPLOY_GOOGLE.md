# النشر على Google Cloud

> اقرا القسم ده الأول قبل ما تصرف أي فلوس.

## الخلاصة السريعة

| السؤال | الإجابة |
|---|---|
| ينفع على GitHub Pages؟ | ❌ لأ. Pages بتقدّم ملفات ساكنة بس. |
| ينفع على Google Sites / Drive؟ | ❌ لأ. نفس السبب. |
| ينفع على Firebase Hosting؟ | ❌ لأ. استضافة ساكنة. |
| ينفع على Google Cloud Run؟ | ✅ أيوه، بس محتاج قاعدة بيانات SQL Server منفصلة. |
| أرخص حل عملي؟ | **VPS** (Hetzner / Contabo) — شوف `DEPLOYMENT_GUIDE.md` |

## ليه؟

المشروع ده **Blazor Server**. يعني:
- الصفحة بتتبني على السيرفر، والمتصفح بيفضل متوصّل بالسيرفر باتصال **SignalR (WebSocket)** مفتوح طول ما الصفحة مفتوحة.
- محتاج **.NET 8 runtime** شغال.
- محتاج **SQL Server** شغال.

استضافة الملفات الساكنة (GitHub Pages وأخواتها) مبتشغّلش كود على السيرفر أصلاً،
فمفيش طريقة تشغّل بيها Blazor Server عليها.

---

## ⚠️ التكلفة — أهم نقطة

المشكلة مش في تشغيل التطبيق، المشكلة في **الداتابيز**.

النظام مكتوب على **SQL Server** (`UseSqlServer` في `Program.cs`).
و **Cloud SQL for SQL Server** على جوجل **غالي جداً** مقارنة بالبدائل،
لأن رخصة SQL Server نفسها داخلة في السعر. دي أغلى بمراحل من VPS
بيشغّل SQL Server Express ببلاش.

**راجع الأسعار الحالية بنفسك على** https://cloud.google.com/sql/pricing
**قبل ما تفتح حساب.**

### البدائل من الأرخص للأغلى

1. **VPS + SQL Server Express** (~$5/شهر) — Express مجاني لحد 10 GB داتا.
   ده الأنسب لمحل أو محلين. الخطوات كاملة في `DEPLOYMENT_GUIDE.md`.
2. **Azure App Service + Azure SQL** — أسهل نشر لمشاريع .NET، وسعر متوسط.
3. **Cloud Run + Cloud SQL for SQL Server** — الأغلى، ومناسب لو عندك سبب
   إنك تفضل تكون على Google Cloud بالذات.

---

## لو قررت تمشي على Cloud Run

المشروع فيه `Dockerfile` جاهز.

### 1. جهّز المطلوب
- حساب Google Cloud فيه **billing مفعّل**
- ثبّت [gcloud CLI](https://cloud.google.com/sdk/docs/install)
- اعمل مشروع جديد وخد الـ Project ID

### 2. اعمل قاعدة البيانات
من Google Cloud Console: **SQL → Create Instance → SQL Server**.
خد منها الـ Connection String.

### 3. ابنِ الصورة وارفعها
```bash
gcloud auth login
gcloud config set project YOUR_PROJECT_ID

gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/mas-web
```

### 4. انشر
```bash
gcloud run deploy mas-web \
  --image gcr.io/YOUR_PROJECT_ID/mas-web \
  --region europe-west1 \
  --allow-unauthenticated \
  --session-affinity \
  --min-instances 1 \
  --timeout 3600 \
  --set-env-vars "ConnectionStrings__DefaultConnection=Server=...;Database=MAS_DB;User Id=...;Password=...;TrustServerCertificate=True"
```

### إعدادات مهمة جداً لـ Blazor Server على Cloud Run

| الإعداد | ليه |
|---|---|
| `--session-affinity` | **إجباري.** من غيره اتصال SignalR بيتوزّع على instances مختلفة والموقع بيقع كل شوية. |
| `--min-instances 1` | من غيره الـ instance بينام، وأول زائر بيستنى دقيقة. |
| `--timeout 3600` | الافتراضي 5 دقايق وبيقطع اتصال SignalR. |

> ملحوظة: حتى مع الإعدادات دي، Blazor Server مش أنسب حاجة لبيئة serverless
> بتعمل scale تلقائي. لو الموقع هيفضل مفتوح ساعات قدام الكاشير، VPS ثابت
> أريح وأرخص.

### 5. حط الأسرار في مكانها الصح
متكتبش الباسوردات في الأمر مباشرة على المدى الطويل — استخدم
[Secret Manager](https://cloud.google.com/run/docs/configuring/secrets).

---

## قبل أول نشر — تشيك ليست

- [ ] الـ build عدى على GitHub Actions (تاب **Actions** في الريبو)
- [ ] جرّبت النظام محلياً وكل الصفحات بتفتح
- [ ] غيّرت باسورد `admin`
- [ ] ضبطت `ConnectionStrings__DefaultConnection` كمتغير بيئة
- [ ] فعّلت HTTPS
- [ ] عندك خطة Backup للداتابيز
- [ ] `appsettings.Production.json` مترفعش على GitHub (متظبط في `.gitignore`)
