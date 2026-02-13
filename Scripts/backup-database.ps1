# Script de Respaldo de Base de Datos - FacturacionV2
# Ejecutar antes de cada fase de migración ERP → FacturacionV2

param(
    [string]$Server = "www.smarttechcr.com",
    [string]$Database = "ATFE",
    [string]$BackupPath = "D:\Backups\FacturacionV2",
    [string]$Username = "sanchez",
    [switch]$FullBackup = $true
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "$BackupPath\${Database}_${timestamp}.bak"

# Crear directorio si no existe
if (!(Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force
    Write-Host "Directorio de backups creado: $BackupPath" -ForegroundColor Green
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BACKUP DE BASE DE DATOS - FACTURACIONV2" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servidor: $Server" -ForegroundColor Yellow
Write-Host "Base de datos: $Database" -ForegroundColor Yellow
Write-Host "Archivo: $backupFile" -ForegroundColor Yellow
Write-Host ""

# Solicitar contraseña de forma segura
$SecurePassword = Read-Host "Ingrese la contraseña para $Username" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
$Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Query de backup
$backupQuery = @"
BACKUP DATABASE [$Database]
TO DISK = N'$backupFile'
WITH NOFORMAT, NOINIT,
NAME = N'$Database-Full Backup $timestamp',
SKIP, NOREWIND, NOUNLOAD, STATS = 10,
COMPRESSION
"@

try {
    Write-Host "Iniciando backup..." -ForegroundColor Cyan

    # Ejecutar backup usando sqlcmd
    sqlcmd -S $Server -U $Username -P $Password -Q $backupQuery -o "$BackupPath\backup_log_$timestamp.txt"

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "BACKUP COMPLETADO EXITOSAMENTE" -ForegroundColor Green
        Write-Host "Archivo: $backupFile" -ForegroundColor Green

        # Mostrar tamaño del archivo
        if (Test-Path $backupFile) {
            $size = (Get-Item $backupFile).Length / 1MB
            Write-Host "Tamaño: $([math]::Round($size, 2)) MB" -ForegroundColor Green
        }
    } else {
        Write-Host "ERROR: El backup falló. Revise el log." -ForegroundColor Red
    }
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    # Limpiar contraseña de memoria
    $Password = $null
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
