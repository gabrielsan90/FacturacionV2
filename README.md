# Sistema de Facturacion

Sistema de facturacion desarrollado con .NET 9 siguiendo la arquitectura MJL de 3 capas.

## Estructura de la Solucion

```
Facturacion/
├── Facturacion.Shared/          # Biblioteca de clases compartida
│   ├── Entities/                # Entidades de base de datos
│   │   └── User.cs
│   ├── DTOs/                    # Data Transfer Objects
│   │   ├── LoginDto.cs
│   │   └── TokenDto.cs
│   ├── Enums/                   # Enumeraciones
│   └── Responses/               # Clases de respuesta
│       └── ActionResponse.cs
│
├── Facturacion.Backend/         # API Web (.NET 9)
│   ├── Controllers/             # Controladores API
│   │   └── AccountsController.cs
│   ├── Data/                    # Contexto y Seed
│   │   ├── DataContext.cs
│   │   └── SeedDb.cs
│   ├── Repositories/            # Repositorios
│   │   ├── IGenericRepository.cs
│   │   └── GenericRepository.cs
│   ├── UnitsOfWork/             # Unidades de Trabajo
│   │   ├── IGenericUnitOfWork.cs
│   │   └── GenericUnitOfWork.cs
│   ├── Helpers/                 # Servicios auxiliares
│   │   ├── IUserHelper.cs
│   │   └── UserHelper.cs
│   ├── Program.cs               # Configuracion del Backend
│   └── appsettings.json         # Configuracion (Connection String, JWT)
│
└── Facturacion.Frontend/        # Aplicacion Web Razor Pages (.NET 9)
    ├── Pages/                   # Paginas Razor
    │   ├── Index.cshtml
    │   ├── Privacy.cshtml
    │   └── Error.cshtml
    ├── Helpers/                 # Servicios auxiliares
    │   ├── IApiService.cs
    │   └── ApiService.cs
    ├── Services/                # Servicios de negocio
    ├── wwwroot/                 # Archivos estaticos (CSS, JS, imagenes)
    ├── Program.cs               # Configuracion del Frontend
    └── appsettings.json         # Configuracion (ApiBaseUrl)
```

## Tecnologias Utilizadas

### Backend
- .NET 9 Web API
- Entity Framework Core 9.0
- SQL Server (LocalDB)
- ASP.NET Core Identity
- JWT Bearer Authentication
- Swagger/OpenAPI

### Frontend
- .NET 9 Razor Pages
- Cookie Authentication
- HttpClient para consumo de API
- Bootstrap (incluido por defecto)

### Shared
- .NET 9 Class Library
- ASP.NET Core Identity EntityFrameworkCore

## Configuracion Inicial

### 1. Requisitos Previos
- .NET 9 SDK instalado
- SQL Server o SQL Server LocalDB
- Visual Studio 2022 o Visual Studio Code

### 2. Configurar Cadena de Conexion

Editar `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/appsettings.json`:

```json
"ConnectionStrings": {
  "LocalConnection": "Server=(localdb)\\mssqllocaldb;Database=Facturacion;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Crear Base de Datos

Desde el directorio del proyecto Backend:

```bash
cd Facturacion.Backend
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Ejecutar los Proyectos

#### Backend (Puerto 7030):
```bash
cd Facturacion.Backend
dotnet run
```

#### Frontend (Puerto 5000):
```bash
cd Facturacion.Frontend
dotnet run
```

## Usuarios Iniciales

El sistema crea automaticamente un usuario administrador al ejecutar por primera vez:

- **Email:** admin@facturacion.com
- **Password:** Admin123!
- **Rol:** Admin

## Arquitectura MJL

### Principios Fundamentales

1. **Separacion de Responsabilidades**: Cada proyecto tiene una responsabilidad especifica.
2. **Repository Pattern**: Abstraccion del acceso a datos mediante repositorios genericos y especificos.
3. **Unit of Work Pattern**: Coordinacion de operaciones de base de datos.
4. **Dependency Injection**: Todas las dependencias se inyectan a traves del contenedor IoC.
5. **ActionResponse Pattern**: Todas las respuestas de API usan el tipo ActionResponse<T>.

### Flujo de Datos

```
Usuario → Frontend (Razor Page)
       ↓ AJAX/Handler
       → HttpClient (IApiService)
       ↓ HTTP Request
       → Backend (Controller)
       ↓ Business Logic
       → Unit of Work
       ↓ Data Access
       → Repository
       ↓ ORM
       → Entity Framework Core
       ↓ SQL Query
       → SQL Server Database
```

## Patron de Desarrollo

### Para Crear una Nueva Entidad

1. **Crear entidad en Shared/Entities/**
2. **Crear DTOs en Shared/DTOs/** (si es necesario)
3. **Agregar DbSet en Backend/Data/DataContext.cs**
4. **Crear Repository especifico** (si requiere queries complejas)
5. **Crear UnitOfWork especifico** (si requiere Repository especifico)
6. **Crear Controller en Backend/Controllers/**
7. **Crear migracion**: `dotnet ef migrations add NombreMigracion`
8. **Aplicar migracion**: `dotnet ef database update`
9. **Crear Razor Page en Frontend/Pages/**
10. **Implementar handlers en PageModel**

## Seguridad

### Backend
- Autenticacion: JWT Bearer Token
- Autorizacion: Por roles usando `[Authorize(Roles = "Admin")]`
- Validacion de datos en DTOs y entidades

### Frontend
- Autenticacion: Cookies
- Autorizacion: Por roles usando `[Authorize(Roles = "Admin,User")]`
- Validacion de formularios del lado del cliente y servidor

## Compilacion de la Solucion

```bash
# Restaurar paquetes
dotnet restore

# Compilar solucion
dotnet build

# Ejecutar tests (cuando se implementen)
dotnet test
```

## Proximos Pasos

1. Implementar autenticacion completa en Frontend
2. Crear paginas de Login y Registro
3. Implementar manejo de tokens JWT en Frontend
4. Crear entidades de negocio (Facturas, Clientes, Productos, etc.)
5. Implementar CRUD completo para cada entidad
6. Agregar validaciones personalizadas
7. Implementar manejo de errores global
8. Agregar logging
9. Implementar tests unitarios y de integracion
10. Configurar CI/CD

## Documentacion de Referencia

Ver archivos de guia en el directorio raiz:
- `ARCHITECTURE_GUIDE.md` - Guia completa de arquitectura
- `BACKEND_PATTERNS.md` - Patrones del Backend
- `FRONTEND_PATTERNS.md` - Patrones del Frontend
- `SHARED_PATTERNS.md` - Patrones de codigo compartido
- `SECURITY_CONFIG.md` - Configuracion de seguridad
- `NAMING_CONVENTIONS.md` - Convenciones de nombres

## Licencia

Este proyecto es privado y confidencial.
