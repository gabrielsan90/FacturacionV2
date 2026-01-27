# Modern UI Components & Helpers Documentation
## Sistema de Facturación CR v4.4

**Version:** 1.0
**Last Updated:** January 2026
**Theme:** Blue Professional (sin morado)

---

## Table of Contents

1. [Theme System](#theme-system)
2. [Color Palette](#color-palette)
3. [Helper Functions](#helper-functions)
4. [Accessibility Features](#accessibility-features)
5. [DataTable Optimizations](#datatable-optimizations)
6. [Component Patterns](#component-patterns)
7. [Best Practices](#best-practices)

---

## Theme System

### CSS Variables

The theme uses CSS variables for dynamic theming and dark mode support.

**File:** `wwwroot/css/modern-theme.css`

#### Primary Colors (Blue Professional)
```css
--primary: #0ea5e9        /* Sky Blue */
--primary-dark: #0284c7   /* Darker Blue */
--primary-light: #38bdf8  /* Lighter Blue */
--primary-rgb: 14, 165, 233
```

#### Semantic Colors
```css
--success: #10b981  /* Green */
--warning: #f59e0b  /* Amber */
--danger: #ef4444   /* Red */
--info: #3b82f6     /* Blue */
```

#### Background Colors
```css
/* Light Mode */
--body-bg: #f8fafc
--content-bg: #ffffff
--card-bg: #ffffff

/* Dark Mode */
[data-theme="dark"] {
    --body-bg: #0f172a
    --content-bg: #1e293b
    --card-bg: #1e293b
    --text-primary: #f1f5f9
}
```

### Dark Mode Toggle

**File:** `wwwroot/js/theme-toggle.js`

**Default Theme:** Light mode (always defaults to light unless user explicitly changes it)

#### Usage

```javascript
// Toggle theme
ThemeManager.toggle();

// Get current theme
const currentTheme = ThemeManager.get(); // 'light' or 'dark'

// Set specific theme
ThemeManager.set('dark');
ThemeManager.set('light');

// Listen for theme changes
window.addEventListener('themeChanged', function(e) {
    console.log('New theme:', e.detail.theme);
});
```

#### Persistence
- User preference is saved in `localStorage` with key `facturacion-theme`
- Persists across sessions until user changes it manually
- **Does NOT** follow system preference - always defaults to light mode
- User must explicitly click the toggle or use keyboard shortcut to change theme

#### Keyboard Shortcut
- **Windows/Linux:** `Ctrl + Shift + D`
- **Mac:** `Cmd + Shift + D`

#### HTML Button
```html
<button class="theme-toggle" id="themeToggle" aria-label="Cambiar tema">
    <span class="theme-toggle-slider">
        <i class="fas fa-sun"></i>
    </span>
</button>
```

---

## Color Palette

### Blue Professional Theme

| Color | Hex | RGB | Usage |
|-------|-----|-----|-------|
| Primary | `#0ea5e9` | `14, 165, 233` | Buttons, links, headers |
| Secondary | `#06b6d4` | `6, 182, 212` | Secondary actions |
| Success | `#10b981` | `16, 185, 129` | Success messages, positive states |
| Warning | `#f59e0b` | `245, 158, 11` | Warnings, pending states |
| Danger | `#ef4444` | `239, 68, 68` | Errors, destructive actions |
| Info | `#3b82f6` | `59, 130, 246` | Information, tooltips |

### Gradient Examples

```css
/* Success Gradient */
background: linear-gradient(135deg, #10b981 0%, #34d399 100%);

/* Warning Gradient */
background: linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%);

/* Primary Gradient */
background: linear-gradient(135deg, #0ea5e9 0%, #38bdf8 100%);
```

---

## Helper Functions

**File:** `wwwroot/js/helpers.js`
**Global Object:** `window.FacturacionHelpers`

### Currency Formatting

```javascript
// Format currency with symbol
FacturacionHelpers.formatCurrency(12500.50, 'CRC');
// Returns: "₡12,500.50"

FacturacionHelpers.formatCurrency(1500.75, 'USD');
// Returns: "$1,500.75"

FacturacionHelpers.formatCurrency(999.99, 'EUR');
// Returns: "€999.99"
```

**Supported Currencies:**
- `CRC` - Colón Costarricense (₡)
- `USD` - US Dollar ($)
- `EUR` - Euro (€)

### Date & Time Formatting

```javascript
// Format date (DD/MM/YYYY)
FacturacionHelpers.formatDate('2025-01-15T10:30:00');
// Returns: "15/01/2025"

// Format date and time (DD/MM/YYYY HH:mm)
FacturacionHelpers.formatDateTime('2025-01-15T10:30:00');
// Returns: "15/01/2025 10:30"
```

### Badge Generators

#### Document Type Badges
```javascript
FacturacionHelpers.getBadgeTipoDocumento('FE');
// Returns: <span class="badge" style="background: #0ea5e9;">FE</span>

FacturacionHelpers.getBadgeTipoDocumento('NC');
// Returns: <span class="badge" style="background: #f59e0b;">NC</span>
```

**Document Types:**
- `FE` - Factura Electrónica (Blue)
- `TE` - Tiquete Electrónico (Cyan)
- `NC` - Nota de Crédito (Warning)
- `ND` - Nota de Débito (Danger)
- `FEE` - Factura Exportación (Purple)

#### Hacienda Status Badges
```javascript
FacturacionHelpers.getBadgeEstadoHacienda('aceptado');
// Returns: <span class="badge bg-success">Aceptado</span>

FacturacionHelpers.getBadgeEstadoHacienda('rechazado');
// Returns: <span class="badge bg-danger">Rechazado</span>
```

**Hacienda Estados:**
- `aceptado` - Green (Success)
- `rechazado` - Red (Danger)
- `pendiente` - Blue (Info)
- `procesando` - Yellow (Warning)

### UI Helpers

#### Show Loading State
```javascript
FacturacionHelpers.showLoading('tableContainer', 'Cargando datos...');
```
Displays a spinner with custom message in the specified element.

#### Show Empty State
```javascript
FacturacionHelpers.showEmptyState('tableContainer', {
    icon: 'fas fa-inbox',
    title: 'No hay datos',
    description: 'No se encontraron registros',
    buttonText: 'Crear Nuevo',
    buttonAction: () => openCreateModal()
});
```

**Options:**
- `icon` - Font Awesome icon class
- `title` - Main heading
- `description` - Subtitle text
- `buttonText` - CTA button text (optional)
- `buttonAction` - Button click handler (optional)

#### Show Toast Notification
```javascript
// Success
FacturacionHelpers.showToast('Operación exitosa', 'success');

// Error
FacturacionHelpers.showToast('Error al guardar', 'error');

// Warning
FacturacionHelpers.showToast('Verifique los datos', 'warning');

// Info
FacturacionHelpers.showToast('Procesando...', 'info');
```

#### Confirm Action
```javascript
FacturacionHelpers.confirmAction(
    '¿Está seguro de eliminar este registro?',
    'Esta acción no se puede deshacer',
    function() {
        // User confirmed - execute action
        deleteRecord(id);
    },
    function() {
        // User cancelled (optional callback)
        console.log('Cancelled');
    }
);
```

### Utility Functions

#### Debounce
```javascript
const debouncedSearch = FacturacionHelpers.debounce(function(query) {
    performSearch(query);
}, 500);

// Call repeatedly - only executes after 500ms of no calls
$('#searchInput').on('keyup', function() {
    debouncedSearch($(this).val());
});
```

#### Copy to Clipboard
```javascript
FacturacionHelpers.copyToClipboard('Text to copy', function(success) {
    if (success) {
        FacturacionHelpers.showToast('Copiado al portapapeles', 'success');
    } else {
        FacturacionHelpers.showToast('Error al copiar', 'error');
    }
});
```

#### Validators
```javascript
// Email validation
if (FacturacionHelpers.isValidEmail('usuario@example.com')) {
    // Valid email
}

// Phone validation (Costa Rica format)
if (FacturacionHelpers.isValidPhone('8888-8888')) {
    // Valid phone
}

// Cédula validation (Costa Rica)
if (FacturacionHelpers.isValidCedula('1-2345-6789')) {
    // Valid cédula
}
```

---

## Accessibility Features

**File:** `wwwroot/js/accessibility.js`
**Global Object:** `window.Accessibility`

### Automatic Enhancements

The accessibility module automatically enhances:
- Form fields with ARIA attributes
- Tables with proper scope and roles
- Modals with focus trapping
- Buttons with keyboard support

#### Initialization
```javascript
// Auto-initializes on DOM ready
// Re-runs on dynamic content via MutationObserver
```

### Manual Functions

#### Announce to Screen Readers
```javascript
// Polite announcement (default)
Accessibility.announce('Tabla actualizada');

// Assertive announcement (interrupts)
Accessibility.announce('Error crítico', 'assertive');
```

#### Mark Element as Busy
```javascript
// Show loading state
Accessibility.setBusy(document.getElementById('table'), true);

// Remove loading state
Accessibility.setBusy(document.getElementById('table'), false);
```

#### Button Loading State
```javascript
const saveButton = document.getElementById('btnSave');

// Start loading
Accessibility.setButtonLoading(saveButton, true);
// Button shows: [Spinner] Procesando...

// Stop loading
Accessibility.setButtonLoading(saveButton, false);
// Button restores original text
```

#### Trap Focus in Modal
```javascript
const modal = document.getElementById('myModal');
Accessibility.trapFocus(modal);
```

### Keyboard Navigation

#### Built-in Shortcuts
- `ESC` - Close modals
- `Arrow Up/Down` - Navigate table rows
- `Ctrl+S` / `Cmd+S` - Submit active form
- `Space` - Activate buttons
- `Tab` - Cycle through focusable elements
- `Shift+Tab` - Cycle backwards

#### Custom Keyboard Navigation
```javascript
document.addEventListener('keydown', function(e) {
    if (e.key === 'F3') {
        e.preventDefault();
        document.getElementById('searchInput').focus();
    }
});
```

### Skip Links
```javascript
// Automatically added to all pages
// Visible only when focused (keyboard users)
```

---

## DataTable Optimizations

**File:** `wwwroot/js/datatable-optimizations.js`
**Global Object:** `window.DataTableOptimizations`

### Create Optimized Table

```javascript
const table = DataTableOptimizations.createOptimizedTable('#myTable', {
    // Additional configuration
    pageLength: 50,
    order: [[0, 'desc']]
});
```

### Default Configuration

```javascript
{
    deferRender: true,          // Render rows only when needed
    processing: true,            // Show processing indicator
    pageLength: 25,              // Default page size
    responsive: true,            // Mobile-friendly
    language: {
        url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
    }
}
```

### Server-Side Processing

**When to Use:** For tables with more than 1000 records, use server-side processing for better performance.

#### Example Implementation

**Frontend (JavaScript):**
```javascript
const table = $('#myTable').DataTable({
    processing: true,
    serverSide: true,
    ajax: {
        url: '?handler=Data',
        type: 'GET',
        data: function(d) {
            // d contains: draw, start, length, search, order, columns
            return $.extend({}, d, {
                customFilter: $('#myFilter').val()
            });
        },
        dataSrc: function(json) {
            // json contains: draw, recordsTotal, recordsFiltered, data
            $('#badgeCount').text(json.recordsTotal);
            return json.data;
        }
    },
    columns: [
        { data: 'id' },
        { data: 'name' }
    ],
    pageLength: 25,
    lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
});
```

**Backend (PageModel):**
```csharp
public async Task<IActionResult> OnGetDataAsync(
    int draw = 1,
    int start = 0,
    int length = 25)
{
    var allData = await GetAllRecordsAsync();
    var totalRecords = allData.Count;

    var pagedData = allData
        .Skip(start)
        .Take(length)
        .ToList();

    return new JsonResult(new
    {
        draw = draw,
        recordsTotal = totalRecords,
        recordsFiltered = totalRecords,
        data = pagedData
    });
}
```

**DataTables Parameters:**
- `draw`: Request counter (must be returned)
- `start`: Index of first record (0-based)
- `length`: Number of records per page
- `search[value]`: Global search term
- `order[i][column]`: Column index to sort
- `order[i][dir]`: Sort direction (asc/desc)

**Response Format:**
- `draw`: Same as request
- `recordsTotal`: Total records before filtering
- `recordsFiltered`: Total after filtering (for search)
- `data`: Array of records for current page

### Search Debouncing

```javascript
// Add 500ms delay to search (reduces server load)
DataTableOptimizations.addSearchDebounce(table, 500);
```

### Column Filtering

```javascript
// Enable filtering on specific columns
DataTableOptimizations.addColumnFilters(table, [0, 2, 3]);

// Enable on all columns
DataTableOptimizations.addColumnFilters(table);
```

### Row Selection

```javascript
// Single row selection
DataTableOptimizations.enableRowSelection(table, 'single');

// Multiple row selection
DataTableOptimizations.enableRowSelection(table, 'multiple');

// Get selected rows
const selectedRows = DataTableOptimizations.getSelectedRows(table);
selectedRows.forEach(row => {
    console.log(row.id, row.name);
});
```

### Refresh Table

```javascript
// Refresh and stay on current page
DataTableOptimizations.refreshTable(table, false);

// Refresh and reset to page 1
DataTableOptimizations.refreshTable(table, true);
```

### Export Functions

```javascript
// Global functions for onclick attributes
<button onclick="exportarExcel()">Excel</button>
<button onclick="exportarPDF()">PDF</button>
<button onclick="refrescarTabla()">Refresh</button>
```

### Configuration Presets

```javascript
// Basic preset (default)
const table1 = $('#table1').optimizedDataTable(
    DataTableOptimizations.presets.basic
);

// With export buttons
const table2 = $('#table2').optimizedDataTable(
    DataTableOptimizations.presets.withExport
);

// Server-side processing
const table3 = $('#table3').optimizedDataTable(
    DataTableOptimizations.presets.serverSide
);

// Compact (10 rows)
const table4 = $('#table4').optimizedDataTable(
    DataTableOptimizations.presets.compact
);

// No filter/search
const table5 = $('#table5').optimizedDataTable(
    DataTableOptimizations.presets.noFilter
);
```

---

## Component Patterns

### Page Header

```html
<div class="page-header mb-4">
    <div class="d-flex justify-content-between align-items-start">
        <div>
            <h2><i class="fas fa-icon"></i> Page Title</h2>
            <p class="mb-0">Page description goes here</p>
        </div>
        <button class="btn btn-primary" onclick="action()">
            <i class="fas fa-plus me-2"></i>Action Button
        </button>
    </div>
</div>
```

### Modern Card

```html
<div class="card shadow-custom">
    <div class="card-header">
        <div class="d-flex justify-content-between align-items-center">
            <div>
                <strong>Card Title</strong>
                <span class="badge bg-secondary ms-2" id="badgeCount">0</span>
            </div>
            <div class="btn-group">
                <button onclick="export()">
                    <i class="fas fa-file-excel"></i>
                </button>
                <button onclick="refresh()">
                    <i class="fas fa-sync-alt"></i>
                </button>
            </div>
        </div>
    </div>
    <div class="card-body">
        <!-- Content -->
    </div>
</div>
```

### Loading State

```html
<div id="tableLoader" class="text-center py-5">
    <div class="spinner-modern mx-auto"></div>
    <p class="text-muted mt-3">Cargando datos...</p>
</div>
```

### Empty State

```html
<div id="emptyState" class="empty-state" style="display: none;">
    <div class="empty-state-icon">
        <i class="fas fa-inbox"></i>
    </div>
    <h3 class="empty-state-title">No hay datos disponibles</h3>
    <p class="empty-state-description">Comienza agregando un nuevo registro</p>
    <button class="btn btn-primary" onclick="create()">
        <i class="fas fa-plus me-2"></i>Crear Nuevo
    </button>
</div>
```

### Table Container Pattern

```html
<div class="card-body">
    <!-- Loading State -->
    <div id="tableLoader" class="text-center py-5">
        <div class="spinner-modern mx-auto"></div>
        <p class="text-muted mt-3">Cargando...</p>
    </div>

    <!-- Table Container -->
    <div id="tableContainer" style="display: none;">
        <table id="dataTable" class="table table-hover">
            <!-- Table content -->
        </table>
    </div>

    <!-- Empty State -->
    <div id="emptyState" class="empty-state" style="display: none;">
        <div class="empty-state-icon">
            <i class="fas fa-inbox"></i>
        </div>
        <h3 class="empty-state-title">No hay registros</h3>
    </div>
</div>

<script>
// DataTable initialization with state management
const table = $('#dataTable').DataTable({
    ajax: {
        url: '?handler=Data',
        dataSrc: function(data) {
            // Hide loader
            $('#tableLoader').hide();

            if (data.length > 0) {
                // Show table
                $('#tableContainer').show();
                $('#emptyState').hide();
                $('#badgeCount').text(data.length);
            } else {
                // Show empty state
                $('#tableContainer').hide();
                $('#emptyState').show();
                $('#badgeCount').text('0');
            }

            return data;
        }
    }
});
</script>
```

### Metric Cards (Dashboard)

```html
<div class="row g-4 mb-4">
    <div class="col-sm-6 col-lg-3">
        <a href="/link" class="text-decoration-none">
            <div class="metric-card success card-hoverable">
                <div class="metric-label">Sales Today</div>
                <div class="metric-value">₡125,500.00</div>
                <div class="metric-icon">
                    <i class="fas fa-dollar-sign"></i>
                </div>
            </div>
        </a>
    </div>
    <!-- More metric cards -->
</div>
```

**Available Classes:**
- `metric-card success` - Green gradient
- `metric-card info` - Blue gradient
- `metric-card warning` - Yellow gradient
- `metric-card danger` - Red gradient
- `card-hoverable` - Adds hover effect

### Modal Pattern

```html
<div class="modal fade" id="myModal" tabindex="-1" data-bs-backdrop="static">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modalTitle">
                    <i class="fas fa-icon me-2 text-primary"></i>Modal Title
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="myForm">
                <div class="modal-body">
                    <!-- Form content -->
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times me-1"></i>Cancelar
                    </button>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-save me-1"></i>Guardar
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>
```

### Buttons

```html
<!-- Primary Actions -->
<button class="btn btn-primary">
    <i class="fas fa-save me-1"></i>Guardar
</button>

<!-- Secondary Actions -->
<button class="btn btn-secondary">
    <i class="fas fa-times me-1"></i>Cancelar
</button>

<!-- Icon Only (Small) -->
<button class="btn btn-sm btn-info" title="Editar">
    <i class="fas fa-edit"></i>
</button>

<!-- Loading State -->
<button class="btn btn-primary" disabled>
    <i class="fas fa-spinner fa-spin me-1"></i>Procesando...
</button>
```

### Badges

```html
<!-- Status Badges -->
<span class="badge bg-success">Activo</span>
<span class="badge bg-secondary">Inactivo</span>
<span class="badge bg-warning">Pendiente</span>
<span class="badge bg-danger">Rechazado</span>

<!-- Count Badges -->
<span class="badge bg-secondary">25</span>

<!-- Pill Badges -->
<span class="badge rounded-pill bg-primary">New</span>
```

---

## Best Practices

### 1. Always Use Helper Functions

✅ **Good:**
```javascript
FacturacionHelpers.formatCurrency(total, 'CRC');
```

❌ **Bad:**
```javascript
'₡' + total.toFixed(2);
```

### 2. Implement Loading States

✅ **Good:**
```javascript
function loadData() {
    FacturacionHelpers.showLoading('tableContainer');

    $.ajax({
        url: '?handler=Data',
        success: function(data) {
            renderTable(data);
        }
    });
}
```

❌ **Bad:**
```javascript
function loadData() {
    // No loading indicator
    $.ajax({
        url: '?handler=Data',
        success: function(data) {
            renderTable(data);
        }
    });
}
```

### 3. Use Empty States

✅ **Good:**
```javascript
if (data.length === 0) {
    FacturacionHelpers.showEmptyState('tableContainer', {
        icon: 'fas fa-inbox',
        title: 'No hay registros',
        description: 'Comienza agregando un nuevo registro'
    });
}
```

❌ **Bad:**
```javascript
if (data.length === 0) {
    $('#tableContainer').html('No data');
}
```

### 4. Announce State Changes

✅ **Good:**
```javascript
function deleteRecord(id) {
    // Delete logic...
    Accessibility.announce('Registro eliminado exitosamente');
    table.ajax.reload();
}
```

❌ **Bad:**
```javascript
function deleteRecord(id) {
    // Delete logic...
    table.ajax.reload(); // No feedback
}
```

### 5. Use Consistent Icons

✅ **Good:**
```html
<button class="btn btn-primary">
    <i class="fas fa-save me-1"></i>Guardar
</button>
```

❌ **Bad:**
```html
<button class="btn btn-primary">
    Save
</button>
```

### 6. Badge Counters

✅ **Good:**
```javascript
$('#badgeCount').text(`${data.length} ${data.length === 1 ? 'registro' : 'registros'}`);
```

❌ **Bad:**
```javascript
$('#badgeCount').text(data.length);
```

### 7. Modal Icons

✅ **Good:**
```html
<h5 class="modal-title">
    <i class="fas fa-user me-2 text-primary"></i>Editar Cliente
</h5>
```

❌ **Bad:**
```html
<h5 class="modal-title">Editar Cliente</h5>
```

### 8. Error Handling

✅ **Good:**
```javascript
$.ajax({
    url: '?handler=Save',
    success: function(response) {
        if (response.success) {
            FacturacionHelpers.showToast(response.message, 'success');
        } else {
            FacturacionHelpers.showToast(response.message, 'error');
        }
    },
    error: function(xhr) {
        FacturacionHelpers.showToast('Error de conexión', 'error');
    }
});
```

❌ **Bad:**
```javascript
$.ajax({
    url: '?handler=Save',
    success: function(response) {
        alert('Guardado');
    }
});
```

### 9. DataTable State Management

✅ **Good:**
```javascript
ajax: {
    url: '?handler=Data',
    dataSrc: function(data) {
        $('#tableLoader').hide();

        if (data.length > 0) {
            $('#tableContainer').show();
            $('#emptyState').hide();
        } else {
            $('#tableContainer').hide();
            $('#emptyState').show();
        }

        return data;
    }
}
```

❌ **Bad:**
```javascript
ajax: {
    url: '?handler=Data'
}
```

### 10. Theme-Aware Colors

✅ **Good:**
```css
color: var(--primary);
background: var(--card-bg);
```

❌ **Bad:**
```css
color: #0ea5e9;
background: #ffffff;
```

---

## Quick Reference

### Common Tasks

#### Initialize DataTable with All States
```javascript
const table = $('#myTable').DataTable({
    ajax: {
        url: '?handler=Data',
        dataSrc: function(data) {
            $('#tableLoader').hide();
            $('#tableContainer').show();
            $('#badgeCount').text(data.length);
            return data;
        }
    }
});
```

#### Create Page Header
```html
<div class="page-header mb-4">
    <div class="d-flex justify-content-between align-items-start">
        <div>
            <h2><i class="fas fa-icon"></i> Title</h2>
            <p class="mb-0">Description</p>
        </div>
        <button class="btn btn-primary" onclick="action()">
            <i class="fas fa-plus me-2"></i>New
        </button>
    </div>
</div>
```

#### Show Toast on AJAX Success
```javascript
success: function(response) {
    if (response.success) {
        FacturacionHelpers.showToast(response.message, 'success');
        table.ajax.reload();
    } else {
        FacturacionHelpers.showToast(response.message, 'error');
    }
}
```

#### Format Currency in DataTable
```javascript
{
    data: 'total',
    render: function(data) {
        return FacturacionHelpers.formatCurrency(data, 'CRC');
    }
}
```

---

## Browser Support

- **Chrome:** Latest 2 versions ✅
- **Firefox:** Latest 2 versions ✅
- **Safari:** Latest 2 versions ✅
- **Edge:** Latest 2 versions ✅
- **IE11:** Not supported ❌

---

## Related Documentation

- [ESPECIFICACION_SISTEMA.md](./ESPECIFICACION_SISTEMA.md) - System specification
- [FRONTEND_PATTERNS.md](./FRONTEND_PATTERNS.md) - Frontend patterns
- [ARCHITECTURE_GUIDE.md](./ARCHITECTURE_GUIDE.md) - Architecture guide

---

## Support

For questions or issues:
1. Check this documentation first
2. Review existing code patterns in Clientes.cshtml, Productos.cshtml
3. Consult with the development team

---

**Made with ❤️ for Sistema de Facturación CR v4.4**
