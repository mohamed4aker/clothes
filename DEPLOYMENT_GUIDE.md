# دليل النشر على VPS - MAS System

## الخيارات المتاحة

### 1. Hetzner Cloud (أرخص - موصى به)
- **VPS CX22**: $4.59/شهر = ~140 ج/شهر
- 4GB RAM, 40GB SSD
- مناسب لـ 10-20 عميل

### 2. DigitalOcean
- **Basic Droplet**: $6/شهر
- 1GB RAM, 25GB SSD

### 3. Contabo (أرخص بالذاكرة)
- **VPS S**: €4.99/شهر
- 8GB RAM, 200GB SSD

## الـ Domain
- من Namecheap أو GoDaddy: ~$10-15/سنة
- أو من نطاقات.مصر: ~150 ج/سنة (للنطاقات .com.eg)

## خطوات النشر (Ubuntu 22.04)

### 1. إعداد السيرفر
```bash
# الاتصال بالسيرفر
ssh root@your-server-ip

# تحديث النظام
apt update && apt upgrade -y

# تثبيت .NET 8
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y dotnet-sdk-8.0

# تثبيت SQL Server
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | tee /etc/apt/trusted.gpg.d/microsoft.asc
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
apt update
apt install -y mssql-server
/opt/mssql/bin/mssql-conf setup

# تثبيت Nginx
apt install -y nginx

# تثبيت Certbot للـ SSL
apt install -y certbot python3-certbot-nginx
```

### 2. نشر التطبيق
```bash
# على جهازك المحلي:
dotnet publish src/MAS.Web/MAS.Web.csproj -c Release -o ./publish

# نقل الملفات للسيرفر:
scp -r ./publish root@your-server-ip:/var/www/mas

# على السيرفر:
cd /var/www/mas
chmod +x MAS.Web
```

### 3. إنشاء Service لـ systemd
```bash
nano /etc/systemd/system/mas.service
```

محتوى الملف:
```ini
[Unit]
Description=MAS Application

[Service]
WorkingDirectory=/var/www/mas
ExecStart=/usr/bin/dotnet /var/www/mas/MAS.Web.dll
Restart=always
RestartSec=10
SyslogIdentifier=mas
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

### 4. إعداد Nginx
```bash
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
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
ln -s /etc/nginx/sites-available/mas /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx
```

### 5. SSL بـ Let's Encrypt (مجاني)
```bash
certbot --nginx -d yourdomain.com
```

### 6. تعديل Connection String
في `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MAS_DB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
}
```

## التكلفة الإجمالية الشهرية:
- **VPS Hetzner**: 140 ج
- **Domain سنوي**: 12 ج (لو مقسم على 12 شهر)
- **SSL**: مجاني (Let's Encrypt)
- **الإجمالي**: ~150 ج/شهر

## النسخ الاحتياطي اليومي
```bash
# crontab -e
0 2 * * * /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword" -Q "BACKUP DATABASE MAS_DB TO DISK='/var/backups/mas_$(date +\%Y\%m\%d).bak'"
```

## الأمان
1. غيّر JWT Key في `appsettings.json` لقيمة عشوائية قوية
2. غيّر كلمة سر admin@zaid.com بعد أول تسجيل دخول
3. عمل firewall:
```bash
ufw allow 22
ufw allow 80
ufw allow 443
ufw enable
```
