# Sistema de Facturación Electrónica - Guía de Implementación Frontend

## Estado Actual de la Implementación

### ✅ COMPLETADO - Infraestructura Base (100%)

#### 1. _Layout.cshtml - CoreUI Admin Template
**Ubicación:** `/Pages/Shared/_Layout.cshtml`

**Características Implementadas:**
- Sidebar con navegación completa de todos los módulos
- Header con breadcrumbs, selector de empresa, notificaciones y perfil de usuario
- Menú colapsable responsive para móviles
- Integración completa de todas las librerías necesarias:
  - CoreUI 4.3.0
  - FontAwesome 6.5.1
  - DataTables 1.13.8
  - Select2 4.1.0
  - SweetAlert2 11
  - Chart.js 4.4.1
  - Moment.js 2.30.1

**Secciones del Menú:**
- Dashboard
- Administración (Empresas, Usuarios, Roles)
- Contactos (Clientes, Proveedores)
- Productos (Productos, Categorías)
- Sucursales (Sucursales, Terminales)
- Inventario (Stock, Movimientos)
- Documentos (Crear, Listar, Recibidos)
- Gastos
- Reportes

**Perfil de Usuario:**
- Muestra nombre completo y email del usuario autenticado
- Avatar con inicial del nombre
- Botón de cerrar sesión con confirmación

#### 2. site.js - Biblioteca de Utilidades JavaScript
**Ubicación:** `/wwwroot/js/site.js`

**Funciones Principales:**

**API Calls:**
```javascript
apiGet(url, params)           // GET request
apiPost(url, data)            // POST request
apiPut(url, data)             // PUT request
apiDelete(url)                // DELETE request
```

**Notificaciones:**
```javascript
showSuccess(message, title)   // Notificación de éxito
showError(message, title)     // Notificación de error
showWarning(message, title)   // Notificación de advertencia
showInfo(message, title)      // Notificación informativa
confirmDialog(msg, title, fn) // Diálogo de confirmación
confirmDelete(msg, fn)        // Confirmación de eliminación
showLoading(message)          // Mostrar spinner de carga
hideLoading()                 // Ocultar spinner
```

**Formateo:**
```javascript
formatCurrency(amount)        // Formato: ₡1,234.56
formatDollars(amount)         // Formato: $1,234.56
formatDate(date)              // Formato: DD/MM/YYYY
formatDateTime(date)          // Formato: DD/MM/YYYY HH:mm
parseCostaRicanDate(string)   // Convierte DD/MM/YYYY a ISO
formatCostaRicanId(id)        // Formato: X-XXXX-XXXX
```

**Validaciones:**
```javascript
validateCostaRicanId(id)      // Valida cédula costarricense
validateEmail(email)          // Valida formato de email
```

**DataTables y Select2:**
```javascript
initDataTable(selector, opts) // Inicializa DataTable en español
initSelect2(selector, opts)   // Inicializa Select2
initSelect2Ajax(sel, url, fn) // Select2 con búsqueda AJAX
```

**Utilidades:**
```javascript
debounce(func, wait)          // Debounce para búsquedas
downloadFile(blob, filename)  // Descarga archivos
handleAjaxError(xhr)          // Manejo global de errores AJAX
```

#### 3. Dashboard (Index.cshtml)
**Ubicación:** `/Pages/Index.cshtml` y `/Pages/Index.cshtml.cs`

**Características:**

**Tarjetas de Métricas (4):**
1. **Ventas Hoy** (Verde) - Total de ventas del día actual
2. **Documentos Pendientes** (Amarillo) - Docs pendientes de Hacienda
3. **Stock Bajo** (Rojo) - Productos bajo stock mínimo
4. **Pagos Pendientes** (Azul) - Total de pagos pendientes

**Gráficos (2):**
1. **Ventas Últimos 7 Días** - Gráfico de línea con Chart.js
2. **Documentos por Tipo** - Gráfico de dona (FE, TE, NC, ND, etc.)

**Tabla de Documentos Recientes:**
- DataTable con últimos 10 documentos
- Columnas: Fecha, Tipo, Consecutivo, Cliente, Total, Estado
- Acciones: Ver Detalle, Descargar PDF

**API Endpoint Esperado:**
```
GET /api/dashboard/resumen
Response: {
    ventasHoy: 1500000.50,
    documentosPendientes: 5,
    productosBajoStock: 12,
    pagosPendientes: 2500000.00,
    ventasUltimos7Dias: [
        { fecha: "2025-11-17", total: 150000 },
        { fecha: "2025-11-18", total: 200000 },
        ...
    ],
    documentosPorTipo: [
        { tipo: "FE", cantidad: 45 },
        { tipo: "TE", cantidad: 12 },
        ...
    ]
}
```

#### 4. Sistema de Autenticación
**Ya implementado:**
- Login.cshtml con diseño moderno
- Logout.cshtml.cs con manejo de sesión
- JWT almacenado en Claims
- Cookie-based authentication

---

## 📋 TEMPLATES PARA PÁGINAS RESTANTES

### Template 1: Página CRUD Básica (Ejemplo: Categorías)

#### Categorias.cshtml
```cshtml
@page
@model CategoriasModel
@{
    ViewData["Title"] = "Categorías";
    ViewData["Breadcrumb"] = "Categorías";
}

<meta name="jwt-token" content="@Model.JwtToken" />

<div class="card">
    <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="mb-0">
            <i class="fas fa-tags me-2"></i>Categorías
        </h5>
        <button class="btn btn-primary btn-sm" onclick="openCreateModal()">
            <i class="fas fa-plus me-1"></i> Nueva Categoría
        </button>
    </div>
    <div class="card-body">
        <table id="tableCategorias" class="table table-hover">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Nombre</th>
                    <th>Descripción</th>
                    <th>Activa</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>
</div>

<!-- Modal Create/Edit -->
<div class="modal fade" id="modalCategoria" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modalTitle">Nueva Categoría</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="formCategoria">
                <div class="modal-body">
                    <input type="hidden" id="Categoria_Id" name="Id" value="0" />

                    <div class="mb-3">
                        <label class="form-label">Nombre <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="Categoria_Nombre" name="Nombre" required />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Descripción</label>
                        <textarea class="form-control" id="Categoria_Descripcion" name="Descripcion" rows="3"></textarea>
                    </div>

                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="Categoria_Activa" name="Activa" checked />
                        <label class="form-check-label" for="Categoria_Activa">Activa</label>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-save me-1"></i> Guardar
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
<script>
    let table;

    $(document).ready(function() {
        loadDataTable();
        setupFormSubmit();
    });

    function loadDataTable() {
        table = initDataTable('#tableCategorias', {
            ajax: {
                url: API_BASE_URL + '/api/categorias',
                headers: {
                    'Authorization': 'Bearer ' + getJwtToken()
                },
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'nombre' },
                { data: 'descripcion' },
                {
                    data: 'activa',
                    render: function(data) {
                        return data
                            ? '<span class="badge bg-success">Activa</span>'
                            : '<span class="badge bg-secondary">Inactiva</span>';
                    }
                },
                {
                    data: 'id',
                    orderable: false,
                    render: function(data) {
                        return `
                            <button class="btn btn-sm btn-info" onclick="edit(${data})">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="deleteRecord(${data})">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                }
            ]
        });
    }

    function setupFormSubmit() {
        $('#formCategoria').on('submit', function(e) {
            e.preventDefault();
            save();
        });
    }

    function openCreateModal() {
        $('#formCategoria')[0].reset();
        $('#Categoria_Id').val('0');
        $('#modalTitle').text('Nueva Categoría');
        $('#modalCategoria').modal('show');
    }

    function edit(id) {
        apiGet('/api/categorias/' + id)
            .done(function(data) {
                $('#Categoria_Id').val(data.id);
                $('#Categoria_Nombre').val(data.nombre);
                $('#Categoria_Descripcion').val(data.descripcion);
                $('#Categoria_Activa').prop('checked', data.activa);
                $('#modalTitle').text('Editar Categoría');
                $('#modalCategoria').modal('show');
            })
            .fail(function(xhr) {
                showError('Error al cargar la categoría');
            });
    }

    function save() {
        const data = {
            id: parseInt($('#Categoria_Id').val()),
            nombre: $('#Categoria_Nombre').val(),
            descripcion: $('#Categoria_Descripcion').val(),
            activa: $('#Categoria_Activa').is(':checked')
        };

        const isNew = data.id === 0;
        const request = isNew
            ? apiPost('/api/categorias', data)
            : apiPut('/api/categorias', data);

        request
            .done(function(response) {
                showSuccess(isNew ? 'Categoría creada exitosamente' : 'Categoría actualizada exitosamente');
                $('#modalCategoria').modal('hide');
                table.ajax.reload();
            })
            .fail(function(xhr) {
                showError('Error al guardar la categoría');
            });
    }

    function deleteRecord(id) {
        confirmDelete('Esta acción no se puede revertir', function() {
            apiDelete('/api/categorias/' + id)
                .done(function() {
                    showSuccess('Categoría eliminada exitosamente');
                    table.ajax.reload();
                })
                .fail(function(xhr) {
                    showError('Error al eliminar la categoría');
                });
        });
    }
</script>
}
```

#### Categorias.cshtml.cs
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "Admin,Employee")]
public class CategoriasModel : PageModel
{
    public string JwtToken { get; set; } = string.Empty;

    public void OnGet()
    {
        JwtToken = User.FindFirst("Token")?.Value ?? string.Empty;
    }
}
```

---

### Template 2: Página CRUD con Dropdowns (Ejemplo: Productos)

Esta página es más compleja porque incluye:
- Dropdown de Categorías
- Select2 para búsquedas
- Validación de campos numéricos
- Control de inventario

**Características adicionales en el modal:**
```cshtml
<div class="mb-3">
    <label class="form-label">Categoría <span class="text-danger">*</span></label>
    <select class="form-select" id="Producto_CategoriaId" name="CategoriaId" required>
        <option value="">Seleccione...</option>
    </select>
</div>

<div class="mb-3">
    <label class="form-label">Precio Venta <span class="text-danger">*</span></label>
    <div class="input-group">
        <span class="input-group-text">₡</span>
        <input type="number" step="0.01" class="form-control"
               id="Producto_PrecioVenta" name="PrecioVenta" required />
    </div>
</div>
```

**JavaScript adicional para cargar dropdowns:**
```javascript
function loadCategorias() {
    apiGet('/api/categorias')
        .done(function(data) {
            const select = $('#Producto_CategoriaId');
            select.empty().append('<option value="">Seleccione...</option>');
            data.forEach(cat => {
                select.append(`<option value="${cat.id}">${cat.nombre}</option>`);
            });
        });
}

$(document).ready(function() {
    loadDataTable();
    setupFormSubmit();
    loadCategorias(); // Cargar categorías al iniciar
});
```

---

### Template 3: Página CRUD Compleja (Ejemplo: Clientes/Proveedores)

Para páginas con direcciones de Costa Rica (Provincia → Cantón → Distrito):

```javascript
// Cascading dropdowns para Costa Rica
function loadProvincias() {
    apiGet('/api/catalogos/provincias')
        .done(function(data) {
            const select = $('#Cliente_ProvinciaId');
            select.empty().append('<option value="">Seleccione...</option>');
            data.forEach(prov => {
                select.append(`<option value="${prov.id}">${prov.nombre}</option>`);
            });
        });
}

$('#Cliente_ProvinciaId').on('change', function() {
    const provinciaId = $(this).val();
    if (provinciaId) {
        loadCantones(provinciaId);
    } else {
        $('#Cliente_CantonId').empty().append('<option value="">Seleccione...</option>');
        $('#Cliente_DistritoId').empty().append('<option value="">Seleccione...</option>');
    }
});

$('#Cliente_CantonId').on('change', function() {
    const cantonId = $(this).val();
    if (cantonId) {
        loadDistritos(cantonId);
    } else {
        $('#Cliente_DistritoId').empty().append('<option value="">Seleccione...</option>');
    }
});
```

---

## 🚀 PÁGINAS PRIORITARIAS A IMPLEMENTAR

### 1. CrearDocumento.cshtml (MÁXIMA PRIORIDAD)

Esta es la página MÁS IMPORTANTE del sistema. Permite crear facturas electrónicas.

**Estructura de la Página:**

```
┌─────────────────────────────────────────┐
│ Header Section (Card)                   │
│ - Tipo Documento (FE, TE, NC, ND, etc.) │
│ - Sucursal dropdown                     │
│ - Terminal dropdown                     │
│ - Cliente (Select2 AJAX)                │
│ - Condición Venta dropdown              │
│ - Medio Pago dropdown                   │
│ - Moneda (CRC/USD)                      │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Detalle Section (Card)                  │
│ [+ Agregar Línea]                       │
│                                         │
│ Tabla Dinámica:                         │
│ | Producto | Desc | Qty | Precio |     │
│ | Desc% | IVA | Subtotal | [X] |       │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Totals Section (Card - Right Aligned)   │
│ Subtotal:          ₡ 100,000.00        │
│ Descuentos:        ₡  10,000.00        │
│ IVA 13%:           ₡  11,700.00        │
│ ─────────────────────────────────       │
│ TOTAL:             ₡ 101,700.00        │
└─────────────────────────────────────────┘

[Guardar Borrador] [Firmar y Enviar] [Cancelar]
```

**Funciones JavaScript Críticas:**
```javascript
let detalleRows = [];
let rowCounter = 0;

function agregarLinea() {
    rowCounter++;
    const row = {
        id: rowCounter,
        productoId: 0,
        descripcion: '',
        cantidad: 1,
        precioUnitario: 0,
        descuentoPorcentaje: 0,
        impuestos: [],
        subtotal: 0
    };
    detalleRows.push(row);
    renderDetalleTable();
}

function calculateRow(rowId) {
    const row = detalleRows.find(r => r.id === rowId);
    const qty = parseFloat($('#qty_' + rowId).val()) || 0;
    const price = parseFloat($('#price_' + rowId).val()) || 0;
    const discount = parseFloat($('#discount_' + rowId).val()) || 0;

    const subtotalSinDesc = qty * price;
    const descuentoMonto = subtotalSinDesc * (discount / 100);
    const subtotal = subtotalSinDesc - descuentoMonto;

    row.subtotal = subtotal;
    $('#subtotal_' + rowId).text(formatCurrency(subtotal));

    calculateTotals();
}

function calculateTotals() {
    let subtotal = 0;
    let totalImpuestos = 0;
    let totalDescuentos = 0;

    detalleRows.forEach(row => {
        subtotal += row.subtotal;

        // Calcular impuestos
        if ($('#iva13_' + row.id).is(':checked')) {
            totalImpuestos += row.subtotal * 0.13;
        }
        if ($('#iva4_' + row.id).is(':checked')) {
            totalImpuestos += row.subtotal * 0.04;
        }
    });

    const total = subtotal + totalImpuestos;

    $('#displaySubtotal').text(formatCurrency(subtotal));
    $('#displayImpuestos').text(formatCurrency(totalImpuestos));
    $('#displayTotal').text(formatCurrency(total));
}

function guardarBorrador() {
    const documento = buildDocumentoObject();
    documento.estado = 'Borrador';

    apiPost('/api/documentos-electronicos/borrador', documento)
        .done(function(response) {
            showSuccess('Borrador guardado exitosamente');
            window.location.href = '/Documentos';
        })
        .fail(function(xhr) {
            showError('Error al guardar el borrador');
        });
}

function firmarYEnviar() {
    confirmDialog('¿Está seguro de generar y enviar este documento a Hacienda?',
        'Confirmar envío', function() {

        const documento = buildDocumentoObject();

        showLoading('Generando documento y enviando a Hacienda...');

        apiPost('/api/documentos-electronicos/generar', documento)
            .done(function(response) {
                hideLoading();
                Swal.fire({
                    icon: 'success',
                    title: '¡Documento generado!',
                    html: `
                        <p>Clave: <code>${response.clave}</code></p>
                        <p>Consecutivo: <strong>${response.consecutivo}</strong></p>
                    `,
                    confirmButtonText: 'Ver Documento'
                }).then(() => {
                    window.location.href = '/DocumentoDetalle?id=' + response.id;
                });
            })
            .fail(function(xhr) {
                hideLoading();
                showError('Error al generar el documento');
            });
    });
}

function buildDocumentoObject() {
    return {
        tipoDocumento: $('#TipoDocumento').val(),
        sucursalId: parseInt($('#SucursalId').val()),
        terminalId: parseInt($('#TerminalId').val()),
        clienteId: parseInt($('#ClienteId').val()),
        condicionVenta: $('#CondicionVenta').val(),
        medioPago: $('#MedioPago').val(),
        moneda: $('#Moneda').val(),
        tipoCambio: parseFloat($('#TipoCambio').val()) || 1,
        detalles: detalleRows.map(row => ({
            productoId: row.productoId,
            descripcion: row.descripcion,
            cantidad: row.cantidad,
            precioUnitario: row.precioUnitario,
            descuentoPorcentaje: row.descuentoPorcentaje,
            impuestos: row.impuestos
        }))
    };
}
```

---

### 2. Documentos.cshtml (Lista de Documentos)

**Filtros:**
- Rango de fechas (desde/hasta)
- Tipo de documento (multi-select)
- Estado (multi-select)
- Cliente (autocomplete)

**Tabla con acciones:**
- Ver Detalle
- Descargar XML
- Descargar PDF
- Enviar Email
- Reenviar a Hacienda (si rechazado)
- Generar NC/ND (si aceptado)

```javascript
function descargarXML(id) {
    window.open(API_BASE_URL + '/api/documentos-electronicos/' + id + '/xml', '_blank');
}

function enviarEmail(id) {
    Swal.fire({
        title: 'Enviar Documento por Email',
        input: 'email',
        inputLabel: 'Correo electrónico',
        inputPlaceholder: 'correo@ejemplo.com',
        showCancelButton: true,
        confirmButtonText: 'Enviar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            apiPost('/api/documentos-electronicos/' + id + '/enviar-email', {
                email: result.value
            })
            .done(function() {
                showSuccess('Email enviado exitosamente');
            });
        }
    });
}

function generarNotaCredito(id) {
    confirmDialog('¿Está seguro de generar una Nota de Crédito para este documento?',
        'Confirmar NC', function() {
        window.location.href = '/CrearDocumento?tipo=NC&referencia=' + id;
    });
}
```

---

## 📊 ENDPOINTS DE API REQUERIDOS

### Dashboard
```
GET /api/dashboard/resumen
```

### Categorías
```
GET    /api/categorias
GET    /api/categorias/{id}
POST   /api/categorias
PUT    /api/categorias
DELETE /api/categorias/{id}
```

### Productos
```
GET    /api/productos
GET    /api/productos/{id}
GET    /api/productos/search?q={query}
POST   /api/productos
PUT    /api/productos
DELETE /api/productos/{id}
```

### Clientes
```
GET    /api/clientes
GET    /api/clientes/{id}
GET    /api/clientes/search?q={query}
POST   /api/clientes
PUT    /api/clientes
DELETE /api/clientes/{id}
```

### Documentos Electrónicos
```
GET    /api/documentos-electronicos
GET    /api/documentos-electronicos/{id}
POST   /api/documentos-electronicos/borrador
POST   /api/documentos-electronicos/generar
GET    /api/documentos-electronicos/{id}/xml
GET    /api/documentos-electronicos/{id}/pdf
POST   /api/documentos-electronicos/{id}/enviar-email
POST   /api/documentos-electronicos/{id}/reenviar-hacienda
```

### Catálogos
```
GET /api/catalogos/provincias
GET /api/catalogos/cantones/{provinciaId}
GET /api/catalogos/distritos/{cantonId}
GET /api/catalogos/tipos-identificacion
GET /api/catalogos/condiciones-venta
GET /api/catalogos/medios-pago
GET /api/catalogos/actividades-economicas
GET /api/catalogos/unidades-medida
GET /api/catalogos/tipos-impuesto
```

---

## 🎨 GUÍA DE ESTILOS Y CONVENCIONES

### Colores del Sistema
```css
--primary: #667eea    (Morado)
--success: #10b981    (Verde)
--danger: #ef4444     (Rojo)
--warning: #f59e0b    (Amarillo)
--info: #3b82f6       (Azul)
--secondary: #6c757d  (Gris)
```

### Badges para Estados
```html
<!-- Documentos -->
<span class="badge bg-success">Aceptado</span>
<span class="badge bg-danger">Rechazado</span>
<span class="badge bg-warning">Pendiente</span>
<span class="badge bg-info">Procesando</span>

<!-- Estados Generales -->
<span class="badge bg-success">Activo</span>
<span class="badge bg-secondary">Inactivo</span>
```

### Iconos FontAwesome
```
Dashboard:      fa-chart-line
Empresas:       fa-building
Usuarios:       fa-users
Clientes:       fa-user-tie
Proveedores:    fa-truck
Productos:      fa-boxes
Categorías:     fa-tags
Sucursales:     fa-store
Terminales:     fa-cash-register
Inventario:     fa-cubes
Documentos:     fa-file-invoice
Gastos:         fa-receipt
Reportes:       fa-chart-bar
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

Para cada nueva página CRUD, seguir estos pasos:

### Frontend (.cshtml)
- [ ] Crear archivo .cshtml con estructura de card
- [ ] Agregar meta tag con JWT token
- [ ] Crear tabla con DataTables
- [ ] Crear modal con formulario
- [ ] Implementar funciones JavaScript:
  - [ ] loadDataTable()
  - [ ] setupFormSubmit()
  - [ ] openCreateModal()
  - [ ] edit(id)
  - [ ] save()
  - [ ] deleteRecord(id)
- [ ] Agregar validaciones de formulario
- [ ] Probar responsive en móvil

### Backend (.cshtml.cs)
- [ ] Crear PageModel con [Authorize]
- [ ] Agregar propiedad JwtToken
- [ ] Implementar OnGet() para obtener token

### Testing
- [ ] Probar creación de registro
- [ ] Probar edición de registro
- [ ] Probar eliminación con confirmación
- [ ] Probar validaciones
- [ ] Probar búsqueda y filtros
- [ ] Probar en diferentes navegadores
- [ ] Verificar manejo de errores

---

## 🔧 TROUBLESHOOTING COMÚN

### Error: "JwtToken is null"
**Solución:** Verificar que el token se esté almacenando en los Claims durante el login:
```csharp
new Claim("Token", loginResponse.Token)
```

### DataTable no carga datos
**Solución:** Verificar en el navegador (F12):
1. Network tab → Ver si el request se hace correctamente
2. Console tab → Ver errores JavaScript
3. Verificar formato de respuesta de la API

### Select2 no funciona
**Solución:** Asegurarse de inicializar después de que el DOM esté listo:
```javascript
$(document).ready(function() {
    initSelect2('#MiSelect');
});
```

### CORS Error
**Solución:** Verificar en el backend que CORS esté configurado:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("https://localhost:5001")
                       .AllowAnyMethod()
                       .AllowAnyHeader());
});
```

---

## 📚 RECURSOS ADICIONALES

- **CoreUI Documentation:** https://coreui.io/docs/
- **DataTables:** https://datatables.net/
- **Select2:** https://select2.org/
- **SweetAlert2:** https://sweetalert2.github.io/
- **Chart.js:** https://www.chartjs.org/
- **FontAwesome Icons:** https://fontawesome.com/icons

---

## 🎯 PRÓXIMOS PASOS RECOMENDADOS

1. **Implementar CrearDocumento.cshtml** (Máxima prioridad)
2. **Implementar Documentos.cshtml** (Lista y filtros)
3. **Implementar Productos.cshtml** (Template complejo)
4. **Implementar Clientes.cshtml** (Con direcciones CR)
5. **Implementar Empresas.cshtml** (Más complejo - tabs)
6. **Resto de páginas CRUD** (usar templates básicos)

---

## 💡 NOTAS FINALES

- **Todos los archivos JavaScript deben usar las funciones de site.js**
- **Todas las llamadas a API deben usar apiGet/apiPost/apiPut/apiDelete**
- **Todas las notificaciones deben usar SweetAlert2**
- **Todos los DataTables deben usar initDataTable()**
- **Todo el código debe estar en español (UI y comentarios)**
- **Seguir el patrón establecido para consistencia**

Este sistema está diseñado para ser **profesional, escalable y fácil de mantener**.
