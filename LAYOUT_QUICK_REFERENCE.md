# LAYOUT QUICK REFERENCE GUIDE

## Page Setup

### Basic Page Structure
```cshtml
@page
@model YourNamespace.YourPageModel
@{
    ViewData["Title"] = "Page Title";
    ViewData["Breadcrumb"] = "Page Name";
}

<div class="page-header">
    <h1>Your Page Title</h1>
    <p class="text-muted">Optional description</p>
</div>

<div class="card">
    <div class="card-header">
        <h5 class="mb-0">Section Title</h5>
    </div>
    <div class="card-body">
        <!-- Your content here -->
    </div>
</div>

@section Scripts {
    <script>
        // Your page-specific JavaScript
    </script>
}
```

## Common Patterns

### DataTable Card
```cshtml
<div class="card">
    <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="mb-0">Lista de Items</h5>
        <button type="button" class="btn btn-primary" onclick="openModal()">
            <i class="fas fa-plus me-2"></i>Nuevo
        </button>
    </div>
    <div class="card-body">
        <table id="tableItems" class="table table-striped table-bordered table-hover">
            <thead>
                <tr>
                    <th>Column 1</th>
                    <th>Column 2</th>
                    <th>Acciones</th>
                </tr>
            </thead>
        </table>
    </div>
</div>
```

### Stats Cards Row
```cshtml
<div class="row mb-4">
    <div class="col-md-3">
        <div class="card text-center">
            <div class="card-body">
                <div class="text-primary mb-2">
                    <i class="fas fa-users fa-2x"></i>
                </div>
                <h3 class="mb-1">1,234</h3>
                <p class="text-muted mb-0">Total Clientes</p>
            </div>
        </div>
    </div>
    <!-- Repeat for other stats -->
</div>
```

### Form Modal
```cshtml
<div class="modal fade" id="modalItem" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <form id="formItem">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalTitle">Nuevo Item</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="Id" name="Item.Id" value="0" />

                    <div class="mb-3">
                        <label for="Name" class="form-label">Nombre *</label>
                        <input type="text" class="form-control" id="Name"
                               name="Item.Name" required />
                    </div>

                    <!-- More fields -->
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary"
                            data-bs-dismiss="modal">Cancelar</button>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-save me-2"></i>Guardar
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>
```

## Button Styles

### Primary Actions
```html
<button class="btn btn-primary">
    <i class="fas fa-plus me-2"></i>Crear Nuevo
</button>
```

### Table Action Buttons
```html
<!-- Edit -->
<button class="btn btn-sm btn-info" onclick="edit(${id})">
    <i class="fas fa-edit"></i>
</button>

<!-- Delete -->
<button class="btn btn-sm btn-danger" onclick="deleteRecord(${id})">
    <i class="fas fa-trash"></i>
</button>

<!-- View -->
<button class="btn btn-sm btn-secondary" onclick="view(${id})">
    <i class="fas fa-eye"></i>
</button>
```

### Button Group
```html
<div class="btn-group" role="group">
    <button class="btn btn-outline-primary">
        <i class="fas fa-download me-2"></i>Descargar
    </button>
    <button class="btn btn-outline-primary">
        <i class="fas fa-print me-2"></i>Imprimir
    </button>
</div>
```

## Status Badges

```html
<!-- Success -->
<span class="badge bg-success">Activo</span>

<!-- Warning -->
<span class="badge bg-warning text-dark">Pendiente</span>

<!-- Danger -->
<span class="badge bg-danger">Rechazado</span>

<!-- Info -->
<span class="badge bg-info">En Proceso</span>

<!-- Secondary -->
<span class="badge bg-secondary">Inactivo</span>
```

## Alerts

### Success Alert
```html
<div class="alert alert-success alert-dismissible fade show" role="alert">
    <i class="fas fa-check-circle me-2"></i>
    <strong>Éxito!</strong> La operación se completó correctamente.
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>
```

### Warning Alert
```html
<div class="alert alert-warning alert-dismissible fade show" role="alert">
    <i class="fas fa-exclamation-triangle me-2"></i>
    <strong>Advertencia!</strong> Por favor revise la información.
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>
```

## Custom CSS Classes

### Text Utilities
```html
<span class="text-gradient">Gradient Text</span>
<div class="shadow-custom">Box with custom shadow</div>
```

### Layout Helpers
```html
<div class="page-header">
    <h1>Page Title</h1>
    <p>Description</p>
</div>
```

## JavaScript Patterns

### DataTable Initialization
```javascript
$(document).ready(function() {
    table = $('#tableItems').DataTable({
        ajax: {
            url: '?handler=Data',
            type: 'GET',
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            dataSrc: 'data'
        },
        columns: [
            { data: 'field1' },
            { data: 'field2' },
            {
                data: 'id',
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
        ],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        responsive: true,
        order: [[0, 'desc']]
    });
});
```

### SweetAlert2 Patterns

#### Success Message
```javascript
Swal.fire({
    icon: 'success',
    title: 'Éxito',
    text: 'Operación completada correctamente',
    confirmButtonColor: '#667eea'
});
```

#### Confirmation Dialog
```javascript
Swal.fire({
    title: '¿Está seguro?',
    text: "Esta acción no se puede revertir",
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sí, eliminar',
    cancelButtonText: 'Cancelar',
    confirmButtonColor: '#667eea',
    cancelButtonColor: '#6c757d'
}).then((result) => {
    if (result.isConfirmed) {
        // Perform action
    }
});
```

#### Toast Notification
```javascript
Swal.fire({
    icon: 'success',
    title: 'Guardado',
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000
});
```

### AJAX Request Pattern
```javascript
$.ajax({
    url: '?handler=Save',
    type: 'POST',
    data: formData,
    processData: false,
    contentType: false,
    headers: {
        "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        if (response.success) {
            Swal.fire('Éxito', response.message, 'success');
            $('#modalItem').modal('hide');
            table.ajax.reload();
        } else {
            Swal.fire('Error', response.message, 'error');
        }
    },
    error: function() {
        Swal.fire('Error', 'No se pudo completar la operación', 'error');
    }
});
```

## Responsive Classes

### Visibility
```html
<!-- Hide on mobile, show on desktop -->
<div class="d-none d-md-block">Desktop Only</div>

<!-- Show on mobile, hide on desktop -->
<div class="d-block d-md-none">Mobile Only</div>

<!-- Hide on small, show on medium+ -->
<span class="d-none d-lg-inline">Large+ Only</span>
```

### Column Layouts
```html
<div class="row">
    <!-- Full width on mobile, half on tablet, third on desktop -->
    <div class="col-12 col-md-6 col-lg-4">
        Content
    </div>
</div>
```

## Form Layouts

### Horizontal Form
```html
<form>
    <div class="row mb-3">
        <label for="inputName" class="col-sm-3 col-form-label">Nombre</label>
        <div class="col-sm-9">
            <input type="text" class="form-control" id="inputName">
        </div>
    </div>
</form>
```

### Inline Form
```html
<form class="row g-3">
    <div class="col-auto">
        <label for="inputSearch" class="visually-hidden">Buscar</label>
        <input type="text" class="form-control" id="inputSearch" placeholder="Buscar...">
    </div>
    <div class="col-auto">
        <button type="submit" class="btn btn-primary">
            <i class="fas fa-search"></i>
        </button>
    </div>
</form>
```

## Common Icons (Font Awesome)

```html
<!-- General -->
<i class="fas fa-home"></i>        <!-- Home -->
<i class="fas fa-search"></i>      <!-- Search -->
<i class="fas fa-plus"></i>        <!-- Add -->
<i class="fas fa-edit"></i>        <!-- Edit -->
<i class="fas fa-trash"></i>       <!-- Delete -->
<i class="fas fa-save"></i>        <!-- Save -->
<i class="fas fa-times"></i>       <!-- Close/Cancel -->
<i class="fas fa-check"></i>       <!-- Check/Confirm -->

<!-- Navigation -->
<i class="fas fa-chevron-left"></i>
<i class="fas fa-chevron-right"></i>
<i class="fas fa-chevron-down"></i>
<i class="fas fa-bars"></i>        <!-- Menu/Hamburger -->

<!-- Actions -->
<i class="fas fa-download"></i>
<i class="fas fa-upload"></i>
<i class="fas fa-print"></i>
<i class="fas fa-filter"></i>
<i class="fas fa-sync"></i>        <!-- Refresh -->

<!-- Status -->
<i class="fas fa-check-circle"></i>     <!-- Success -->
<i class="fas fa-exclamation-triangle"></i> <!-- Warning -->
<i class="fas fa-info-circle"></i>      <!-- Info -->
<i class="fas fa-times-circle"></i>     <!-- Error -->

<!-- Business -->
<i class="fas fa-building"></i>     <!-- Company -->
<i class="fas fa-users"></i>        <!-- Users -->
<i class="fas fa-user-tie"></i>     <!-- Client -->
<i class="fas fa-truck"></i>        <!-- Supplier -->
<i class="fas fa-box"></i>          <!-- Product -->
<i class="fas fa-receipt"></i>      <!-- Invoice -->
<i class="fas fa-warehouse"></i>    <!-- Inventory -->
<i class="fas fa-chart-line"></i>   <!-- Dashboard -->
<i class="fas fa-chart-bar"></i>    <!-- Reports -->
```

## Color Variables Reference

```css
/* Use these in your custom styles */
var(--primary-color)      /* #667eea */
var(--secondary-color)    /* #764ba2 */
var(--sidebar-bg)         /* #ffffff */
var(--body-bg)            /* #f5f7fa */
var(--text-primary)       /* #212529 */
var(--text-secondary)     /* #6c757d */
var(--text-muted)         /* #adb5bd */
```

## Tips and Best Practices

1. **Always use icons with text** for better UX
2. **Add loading states** to buttons during AJAX calls
3. **Validate forms** both client and server side
4. **Use proper ARIA labels** for accessibility
5. **Test on mobile devices** not just desktop
6. **Keep modals focused** - one action per modal
7. **Provide feedback** for every user action
8. **Use consistent spacing** - follow Bootstrap utilities
9. **Keep forms simple** - group related fields
10. **Test with real data** before deploying

## Need Help?

Refer to these documents:
- `FRONTEND_PATTERNS.md` - Complete coding standards
- `LAXOM_IMPLEMENTATION_NOTES.md` - Layout details
- Bootstrap 5 Docs - https://getbootstrap.com/docs/5.3/
