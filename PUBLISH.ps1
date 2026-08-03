# سكريبت تلقائي للنشر
# شغله من فولدر المشروع: .\PUBLISH.ps1

$ErrorActionPreference = "Stop"

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "  MAS POS System - Publish Script" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

# 1. مسح المخرجات السابقة
$outputPath = "C:\MAS-ToDeliver"
if (Test-Path $outputPath) {
    Write-Host "حذف المخرجات السابقة..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $outputPath
}

# 2. Publish
Write-Host "" 
Write-Host "[1/3] جاري عمل Publish..." -ForegroundColor Green
dotnet publish src/MAS.Web/MAS.Web.csproj `
    -c Release `
    -o $outputPath `
    --self-contained false `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "فشل الـ Publish!" -ForegroundColor Red
    exit 1
}

# 3. مسح الملفات الزائدة
Write-Host "" 
Write-Host "[2/3] تنظيف الملفات الزائدة..." -ForegroundColor Green
Get-ChildItem $outputPath -Filter "*.pdb" | Remove-Item -Force
Get-ChildItem $outputPath -Filter "*.xml" -Exclude "appsettings*.xml" | Remove-Item -Force

# 4. عمل ZIP
Write-Host ""
Write-Host "[3/3] جاري عمل ZIP للتسليم..." -ForegroundColor Green
$zipPath = "C:\MAS-Final-Delivery.zip"
if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
Compress-Archive -Path "$outputPath\*" -DestinationPath $zipPath

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "  انتهى!" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "الملفات المنشورة: $outputPath" -ForegroundColor White
Write-Host "ZIP جاهز للتسليم: $zipPath" -ForegroundColor White
Write-Host ""
Write-Host "الخطوة الجاية: شفر الـ DLLs بـ ConfuserEx" -ForegroundColor Yellow
Write-Host "  1. افتح ConfuserEx" -ForegroundColor Gray
Write-Host "  2. Base Directory: $outputPath" -ForegroundColor Gray
Write-Host "  3. ضيف: MAS.Web.dll, MAS.Application.dll, MAS.Domain.dll, MAS.Infrastructure.dll" -ForegroundColor Gray
Write-Host "  4. لكل DLL: Settings -> Add Rule -> Preset: Normal" -ForegroundColor Gray
Write-Host "  5. اضغط Protect!" -ForegroundColor Gray
Write-Host "  6. انسخ الـ DLLs المشفرة من المجلد Confused بتاع ConfuserEx لـ $outputPath" -ForegroundColor Gray
Write-Host "  7. اعمل Compress-Archive تاني للحصول على نسخة محمية" -ForegroundColor Gray
