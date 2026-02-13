#!/bin/bash
# Script de Respaldo de Base de Datos - FacturacionV2
# Para Linux/WSL - Ejecutar antes de cada fase de migración

SERVER="www.smarttechcr.com"
DATABASE="ATFE"
USERNAME="sanchez"
BACKUP_PATH="/mnt/d/Backups/FacturacionV2"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

echo "========================================"
echo "BACKUP DE BASE DE DATOS - FACTURACIONV2"
echo "========================================"
echo ""
echo "Servidor: $SERVER"
echo "Base de datos: $DATABASE"
echo ""

# Crear directorio si no existe
mkdir -p "$BACKUP_PATH"

# Solicitar contraseña
read -sp "Ingrese la contraseña para $USERNAME: " PASSWORD
echo ""

# Backup usando sqlcmd (si está instalado)
if command -v sqlcmd &> /dev/null; then
    echo "Iniciando backup con sqlcmd..."
    sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "
    BACKUP DATABASE [$DATABASE]
    TO DISK = N'$BACKUP_PATH/${DATABASE}_${TIMESTAMP}.bak'
    WITH COMPRESSION, STATS = 10"
else
    echo "sqlcmd no encontrado. Instalando mssql-tools..."
    echo "Ejecute: curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -"
    echo "         sudo apt-get update && sudo apt-get install mssql-tools"
fi

# Alternativa: Exportar schema con dotnet ef
echo ""
echo "Alternativa: Exportar schema con EF Core migrations..."
cd /mnt/d/Proyectos/ERPFactu/FacturacionV2/Facturacion.Backend

# Generar script SQL de todas las migraciones
dotnet ef migrations script --output "$BACKUP_PATH/migrations_${TIMESTAMP}.sql" 2>/dev/null

if [ $? -eq 0 ]; then
    echo "Script de migraciones exportado: $BACKUP_PATH/migrations_${TIMESTAMP}.sql"
fi

echo ""
echo "========================================"
echo "Backup completado"
echo "========================================"
