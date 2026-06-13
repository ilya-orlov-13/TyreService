param(
    [string]$DbPassword
)

$ProjectDir = Join-Path $PSScriptRoot "TyreServiceApp\TyreServiceApp"
$FrontendDir = Join-Path $PSScriptRoot "web"
$OcrDir = Join-Path $PSScriptRoot "ocr-service"
$OcrLog = Join-Path $PSScriptRoot "ocr-service\log.txt"
$ScriptsDir = Join-Path $PSScriptRoot "scripts"
$MinioDir = Join-Path $PSScriptRoot "minio"
$MinioExe = Join-Path $MinioDir "minio.exe"
$MinioData = Join-Path $MinioDir "data"
$MinioLog = Join-Path $MinioDir "server.log"
$FrontendUrl = "http://localhost:5173"
$BackendUrl = "http://localhost:5000"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 1. PostgreSQL
$pgService = Get-Service -Name "postgresql-x64-16" -ErrorAction SilentlyContinue
if ($pgService -and $pgService.Status -ne 'Running') {
    Write-Host "Запускаю PostgreSQL..." -ForegroundColor Green
    Start-Service -Name $pgService.Name
}

# 2. Minio
if (-not (Test-Path $MinioExe)) {
    Write-Host "Скачиваю MinIO сервер..." -ForegroundColor Yellow
    $null = New-Item -ItemType Directory -Path $MinioDir -Force
    $minioUrl = "https://dl.min.io/server/minio/release/windows-amd64/minio.exe"
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest -Uri $minioUrl -OutFile $MinioExe -UseBasicParsing -TimeoutSec 120
    $ProgressPreference = 'Continue'
    Write-Host "  MinIO скачан" -ForegroundColor Green
}
$null = New-Item -ItemType Directory -Path $MinioData -Force
Write-Host "Запускаю MinIO (localhost:9000)..." -ForegroundColor Green
$minioJob = Start-Job -ScriptBlock {
    param($exe, $data, $log)
    $env:MINIO_ROOT_USER = "minioadmin"
    $env:MINIO_ROOT_PASSWORD = "minioadmin"
    & $exe server $data --console-address ":9001" 2>&1 | Out-File -FilePath $log -Encoding utf8
} -ArgumentList $MinioExe, $MinioData, $MinioLog
Start-Sleep -Seconds 3

# 3. Применяем миграции
Write-Host "Применяю миграции..." -ForegroundColor Green
if ($DbPassword) {
    $env:DbPassword = $DbPassword
}
Push-Location $ProjectDir
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при применении миграций" -ForegroundColor Red
    Pop-Location
    Stop-Job $minioJob -ErrorAction SilentlyContinue
    Remove-Job $minioJob -ErrorAction SilentlyContinue
    exit $LASTEXITCODE
}
Pop-Location

# 4. EasyOCR
Write-Host "Запускаю EasyOCR сервис (порт 5003)..." -ForegroundColor Green
$ocrJob = Start-Job -ScriptBlock {
    param($dir, $log)
    Set-Location $dir
    uvicorn app:app --host 0.0.0.0 --port 5003 2>&1 | Out-File -FilePath $log -Encoding utf8
} -ArgumentList $OcrDir, $OcrLog
Start-Sleep -Seconds 5

# 5. Фронтенд (Vite)
Write-Host "Запускаю фронтенд (порт 5173)..." -ForegroundColor Green
$frontendJob = Start-Job -ScriptBlock {
    param($dir)
    Set-Location $dir
    npm run dev 2>&1
} -ArgumentList $FrontendDir
Start-Sleep -Seconds 2

# 6. Бэкенд (главный процесс)
Write-Host ""
Write-Host "Сервисы запущены:" -ForegroundColor Cyan
Write-Host "  Фронтенд: $FrontendUrl" -ForegroundColor Cyan
Write-Host "  Бэкенд:   $BackendUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "Запускаю бэкенд ($BackendUrl)..." -ForegroundColor Green
try {
    Push-Location $ProjectDir
    dotnet run --urls $BackendUrl
} finally {
    Pop-Location
    Write-Host "Останавливаю сервисы..." -ForegroundColor Yellow
    Stop-Job $frontendJob -ErrorAction SilentlyContinue
    Remove-Job $frontendJob -ErrorAction SilentlyContinue
    Stop-Job $ocrJob -ErrorAction SilentlyContinue
    Remove-Job $ocrJob -ErrorAction SilentlyContinue
    Stop-Job $minioJob -ErrorAction SilentlyContinue
    Remove-Job $minioJob -ErrorAction SilentlyContinue
}
