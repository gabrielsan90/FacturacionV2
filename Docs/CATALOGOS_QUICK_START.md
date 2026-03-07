# Catálogos Geográficos - Quick Start Guide

## Quick Setup (5 minutes)

### Step 1: Seed the Database
```sql
-- Execute this in SQL Server Management Studio (SSMS)
-- File: SeedData_CatalogosGeograficos.sql
-- Or copy-paste the SQL from the file
```

### Step 2: Verify Services are Registered
Check `Facturacion.Backend/Program.cs` contains:
```csharp
// Lines 183-187
builder.Services.AddScoped<IProvinciaRepository, ProvinciaRepository>();
builder.Services.AddScoped<ICantonRepository, CantonRepository>();
builder.Services.AddScoped<IDistritoRepository, DistritoRepository>();
builder.Services.AddScoped<ICatalogoUnitOfWork, CatalogoUnitOfWork>();
```

### Step 3: Build and Run
```bash
# Terminal 1 - Backend
cd Facturacion.Backend
dotnet run

# Terminal 2 - Frontend
cd Facturacion.Frontend
dotnet run
```

### Step 4: Test the API
Use Swagger UI at `https://localhost:7000/swagger` or Postman:

```bash
# Get all provinces
GET https://localhost:7000/api/catalogos/provincias
Headers: Authorization: Bearer {your-jwt-token}

# Get cantones for San José
GET https://localhost:7000/api/catalogos/cantones/1
Headers: Authorization: Bearer {your-jwt-token}

# Get distritos for San José/Escazú
GET https://localhost:7000/api/catalogos/distritos/1/2
Headers: Authorization: Bearer {your-jwt-token}
```

### Step 5: Test the Frontend
1. Navigate to `https://localhost:7031/Empresas`
2. Click "Nueva Empresa"
3. Select "San José" from Provincia dropdown
4. Watch Cantones dropdown populate automatically
5. Select a canton
6. Watch Distritos dropdown populate automatically

---

## Expected Results

### API Response for Provincias:
```json
{
  "success": true,
  "data": [
    { "codigo": 1, "nombre": "San José" },
    { "codigo": 2, "nombre": "Alajuela" },
    { "codigo": 3, "nombre": "Cartago" },
    { "codigo": 4, "nombre": "Heredia" },
    { "codigo": 5, "nombre": "Guanacaste" },
    { "codigo": 6, "nombre": "Puntarenas" },
    { "codigo": 7, "nombre": "Limón" }
  ]
}
```

### Frontend Cascading Dropdowns:
```
Provincia: [Select...] ▼
           ↓ (user selects San José)
Cantón:    [Select...] ▼
           Options: San José, Escazú, Desamparados, etc.
           ↓ (user selects Escazú)
Distrito:  [Select...] ▼
           Options: Escazú, San Antonio, San Rafael
```

---

## Troubleshooting

### Problem: Dropdowns are empty
**Solution**:
1. Check browser console for errors
2. Verify backend is running
3. Check you're logged in (JWT cookie exists)
4. Run seed data SQL script

### Problem: "Unauthorized" error
**Solution**:
1. Login to the system first
2. Check JWT token in browser cookies (F12 → Application → Cookies)
3. Verify token is being sent in API requests (F12 → Network → Headers)

### Problem: Canton/Distrito dropdowns don't populate
**Solution**:
1. Open browser DevTools (F12)
2. Go to Network tab
3. Select a province
4. Check if API call succeeds (Status 200)
5. Verify response has data

### Problem: Database errors
**Solution**:
1. Ensure seed data was executed
2. Check connection string in `appsettings.json`
3. Verify database tables exist: `Provincias`, `Cantones`, `Distritos`
4. Run query: `SELECT COUNT(*) FROM Provincias` (should return 7)

---

## Quick Verification Commands

### Check Data in Database:
```sql
-- Should return 7 rows
SELECT COUNT(*) FROM Provincias;

-- Should return 82 rows
SELECT COUNT(*) FROM Cantones;

-- Should return 41+ rows
SELECT COUNT(*) FROM Distritos;

-- View all provincias
SELECT * FROM Provincias ORDER BY Codigo;

-- View cantones for San José
SELECT * FROM Cantones WHERE ProvinciaId = 1 ORDER BY Codigo;

-- View distritos for Escazú
SELECT d.* FROM Distritos d
INNER JOIN Cantones c ON d.CantonId = c.Id
WHERE c.Codigo = '0102'
ORDER BY d.Codigo;
```

### Check API Endpoints:
```bash
# Quick test with curl (replace TOKEN)
curl -H "Authorization: Bearer {TOKEN}" \
     https://localhost:7000/api/catalogos/provincias

curl -H "Authorization: Bearer {TOKEN}" \
     https://localhost:7000/api/catalogos/cantones/1

curl -H "Authorization: Bearer {TOKEN}" \
     https://localhost:7000/api/catalogos/distritos/1/1
```

---

## Files Reference

### Backend Files (Created):
- `Repositories/Interfaces/IProvinciaRepository.cs`
- `Repositories/Implementations/ProvinciaRepository.cs`
- `Repositories/Interfaces/ICantonRepository.cs`
- `Repositories/Implementations/CantonRepository.cs`
- `Repositories/Interfaces/IDistritoRepository.cs`
- `Repositories/Implementations/DistritoRepository.cs`
- `UnitsOfWork/Interfaces/ICatalogoUnitOfWork.cs`
- `UnitsOfWork/Implementations/CatalogoUnitOfWork.cs`
- `Controllers/CatalogosController.cs`

### Backend Files (Modified):
- `Program.cs` (lines 183-187)

### Frontend Files (Modified):
- `Pages/Empresas.cshtml.cs` (lines 272-293)

### Database Files (Created):
- `SeedData_CatalogosGeograficos.sql`

### Documentation Files (Created):
- `CATALOGOS_GEOGRAFICOS_IMPLEMENTATION.md`
- `CATALOGOS_QUICK_START.md` (this file)

---

## What Changed?

### BEFORE:
```csharp
// Empresas.cshtml.cs - Hardcoded fallback
catch {
    Provincias = new List<SelectListItem> {
        new SelectListItem { Value = "1", Text = "San José" },
        new SelectListItem { Value = "2", Text = "Alajuela" },
        ...
    };
}
```

### AFTER:
```csharp
// Empresas.cshtml.cs - Direct API call
var response = await client.GetAsync("/api/catalogos/provincias");
if (response.IsSuccessStatusCode) {
    // Parse and display provincias from database
}
```

---

## Next Steps

1. ✅ Seed database with geographic data
2. ✅ Test API endpoints
3. ✅ Test frontend cascading dropdowns
4. ✅ Verify empresa creation with location
5. 📝 Optional: Add remaining 447 distritos to seed data
6. 📝 Optional: Implement caching for catalog data
7. 📝 Optional: Add barrios (neighborhoods) if needed

---

## Success Criteria

✅ Backend compiles without errors
✅ Frontend compiles without errors
✅ Database contains 7 provincias
✅ Database contains 82 cantones
✅ Database contains 41+ distritos
✅ GET /api/catalogos/provincias returns 7 items
✅ GET /api/catalogos/cantones/1 returns 20 cantones
✅ Empresas page loads provincias dropdown
✅ Selecting provincia populates cantones
✅ Selecting canton populates distritos
✅ Can create/edit empresa with location

---

## Support

For detailed documentation, see:
- `CATALOGOS_GEOGRAFICOS_IMPLEMENTATION.md` - Complete implementation details
- `BACKEND_PATTERNS.md` - Repository/UnitOfWork patterns
- `FRONTEND_PATTERNS.md` - PageModel patterns
- `ARCHITECTURE_GUIDE.md` - Overall architecture

If you encounter issues:
1. Check browser console (F12)
2. Check backend logs
3. Verify database connection
4. Ensure seed data is loaded
5. Confirm JWT authentication is working
