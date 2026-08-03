# دليل النشر والتسليم للعميل

هذا الدليل يشرح كيفية:
1. تصفير البيانات وتجهيز النظام للتسليم
2. عمل Publish (Build للإصدار النهائي)
3. حماية الكود (Obfuscation)
4. النشر على VPS (سيرفر على النت)

---

## الخطوة 1: تجهيز المشروع للتسليم

### قبل ما تعمل Publish:

1. **اقفل المشروع تماماً**

2. **امسح الداتابيز القديمة** (لو موجودة):
   - SQL Server Object Explorer → MAS_DB → Delete (Close existing connections)

3. **شغل المشروع مرة واحدة (F5)** عشان يعمل الداتابيز الجديدة الفاضية

4. **اقفل المشروع** بعد ما يتأكد إنه شغال

---

## الخطوة 2: عمل Publish

### الطريقة من Visual Studio:

1. **Right-Click على مشروع MAS.Web** في Solution Explorer
2. اختار **Publish**
3. اختار **Folder** كـ target
4. **Folder Location**: مثلاً `C:\MAS-Published`
5. **Configuration**: Release
6. **Target Framework**: net8.0
7. **Deployment Mode**: 
   - **Self-Contained** = يشتغل بدون .NET (حجم أكبر)
   - **Framework-Dependent** = لازم العميل ينزل .NET 8 (حجم أصغر)
8. **Target Runtime**: win-x64
9. **اضغط Publish**

### الطريقة من Command Line (الأسرع):

افتح PowerShell في مجلد المشروع واكتب:

```powershell
# Framework-Dependent (موصى به)
dotnet publish src/MAS.Web/MAS.Web.csproj -c Release -o C:\MAS-Published

# أو Self-Contained
dotnet publish src/MAS.Web/MAS.Web.csproj -c Release -r win-x64 --self-contained true -o C:\MAS-Published
```

---

## الخطوة 3: حماية الكود (Obfuscation)

عشان العميل ما يقدرش يفتح الـ DLLs ويشوف الكود:

### الأداة: ConfuserEx (مجاني)

1. **حمل من**: https://github.com/mkaring/ConfuserEx/releases
2. **فك الضغط** في مكان زي `C:\ConfuserEx`
3. **افتح ConfuserEx.exe**

### الإعدادات:

1. **Project Settings**:
   - **Base Directory**: `C:\MAS-Published`
   - **Output Directory**: `C:\MAS-Protected`

2. **Add Module**: ضيف الـ DLLs التالية فقط:
   - `MAS.Web.dll`
   - `MAS.Application.dll`
   - `MAS.Domain.dll`
   - `MAS.Infrastructure.dll`
   - `MAS.API.dll`

   ⚠️ **مش تضيف الـ DLLs التانية** (Microsoft.*, System.*, MudBlazor.*)

3. **Settings لكل Module**:
   - Rules: Add Rule → اختار **Preset = Normal**
   - يكفي للحماية الأساسية

4. **Protect!** اضغط الزرار

5. **بعد ما يخلص**، انسخ الـ DLLs المحمية من `C:\MAS-Protected` لـ `C:\MAS-Published` (استبدلهم)

---

## الخطوة 4: النشر على VPS

### اختار VPS مناسب:

| الشركة | السعر | المواصفات |
|--------|-------|-----------|
| **Hetzner CX22** | 4.59€/شهر (~150 ج) | 4GB RAM, 40GB SSD |
| **Contabo VPS S** | 4.99€/شهر | 8GB RAM, 200GB SSD |
| **DigitalOcean** | $6/شهر | 1GB RAM, 25GB SSD |

### بعد ما تشتري VPS Ubuntu 22.04:

#### 1. اتصل بالسيرفر:
```bash
ssh root@your-server-ip
```

#### 2. ثبت .NET 8:
```bash
apt update && apt upgrade -y
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y aspnetcore-runtime-8.0
```

#### 3. ثبت SQL Server:
```bash
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
apt update
apt install -y mssql-server
/opt/mssql/bin/mssql-conf setup
```

اختار **Express edition** (مجاني) وحط كلمة سر قوية.

#### 4. ارفع الملفات للسيرفر:

من جهازك (PowerShell):
```powershell
scp -r C:\MAS-Published\* root@your-server-ip:/var/www/mas/
```

#### 5. عدل connection string:

على السيرفر:
```bash
nano /var/www/mas/appsettings.json
```

غير الـ connection string لـ:
```
"DefaultConnection": "Server=localhost;Database=MAS_DB;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True"
```

#### 6. اعمل service للنظام:

```bash
nano /etc/systemd/system/mas.service
```

```ini
[Unit]
Description=MAS POS System

[Service]
WorkingDirectory=/var/www/mas
ExecStart=/usr/bin/dotnet /var/www/mas/MAS.Web.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5002

[Install]
WantedBy=multi-user.target
```

```bash
systemctl enable mas
systemctl start mas
systemctl status mas
```

#### 7. ثبت Nginx + SSL:

```bash
apt install -y nginx certbot python3-certbot-nginx
nano /etc/nginx/sites-available/mas
```

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    location / {
        proxy_pass http://localhost:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
ln -s /etc/nginx/sites-available/mas /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx
certbot --nginx -d yourdomain.com
```

#### 8. النظام شغال!

افتح في المتصفح: `https://yourdomain.com`

---

## الخطوة 5: التسليم للعميل

### بيانات الدخول الأولى:

أعطي العميل:
- **الرابط**: https://yourdomain.com
- **Email**: admin@maspos.com
- **Password**: Admin@123

### نصائح للعميل:

1. **غير الـ Password فوراً** بعد أول تسجيل دخول
2. **عدل بيانات البراند** (الاسم، العنوان، الهاتف) من الإعدادات
3. **ضيف منتجاتك وعملاءك**
4. **اعمل backup كل أسبوع** (دي مسؤوليتك انت)

### Backup يومي تلقائي:

على السيرفر:
```bash
crontab -e
```

ضيف السطر:
```
0 2 * * * /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword" -Q "BACKUP DATABASE MAS_DB TO DISK='/var/backups/mas_$(date +\%Y\%m\%d).bak'"
```

---

## التكلفة الإجمالية الشهرية للعميل

- VPS: ~150 ج
- Domain: ~12 ج (محسوبة شهرياً)
- SSL: مجاني
- **الإجمالي: ~162 ج/شهر**

---

## مشاكل شائعة وحلولها

### المشروع مش بيشتغل على السيرفر:
```bash
systemctl status mas
journalctl -u mas -n 50
```

### الداتابيز مش بتتعمل:
- تأكد إن SQL Server شغال: `systemctl status mssql-server`
- تأكد إن الـ connection string صح
- جرب تتصل يدوي: `/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Password"`

### بطء في الأداء:
- زود الـ RAM للـ VPS
- اعمل index على الجداول الكبيرة
- استخدم Output Caching في ASP.NET Core
