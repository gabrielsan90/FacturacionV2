# Adding Empresas to Navigation Menu

## Option 1: Add to _Layout.cshtml

Add this navigation item to your main navigation menu in:
`/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Pages/Shared/_Layout.cshtml`

### Example Snippet:

```html
<ul class="navbar-nav flex-grow-1">
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-page="/Index">
            <i class="fas fa-home"></i> Inicio
        </a>
    </li>

    <!-- ADD THIS -->
    @if (User.IsInRole("SuperUser"))
    {
        <li class="nav-item">
            <a class="nav-link text-dark" asp-area="" asp-page="/Empresas">
                <i class="fas fa-building"></i> Empresas
            </a>
        </li>
    }

    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-page="/Privacy">
            <i class="fas fa-shield-alt"></i> Privacidad
        </a>
    </li>
</ul>
```

## Option 2: Sidebar Menu (if using admin template)

```html
<li class="nav-item">
    <a class="nav-link" asp-page="/Empresas">
        <i class="fas fa-fw fa-building"></i>
        <span>Gestión de Empresas</span>
    </a>
</li>
```

## Option 3: Dropdown Menu (Administración)

```html
<li class="nav-item dropdown">
    <a class="nav-link dropdown-toggle text-dark" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
        <i class="fas fa-cog"></i> Administración
    </a>
    <ul class="dropdown-menu">
        @if (User.IsInRole("SuperUser"))
        {
            <li>
                <a class="dropdown-item" asp-page="/Empresas">
                    <i class="fas fa-building"></i> Empresas
                </a>
            </li>
        }
        <li><hr class="dropdown-divider"></li>
        <li>
            <a class="dropdown-item" asp-page="/Usuarios">
                <i class="fas fa-users"></i> Usuarios
            </a>
        </li>
    </ul>
</li>
```

## Option 4: Breadcrumb (for Empresas page itself)

Already included in the page, but you can add to _Layout.cshtml:

```html
@if (ViewContext.RouteData.Values["page"]?.ToString() == "/Empresas")
{
    <nav aria-label="breadcrumb">
        <ol class="breadcrumb">
            <li class="breadcrumb-item"><a asp-page="/Index">Inicio</a></li>
            <li class="breadcrumb-item active" aria-current="page">
                <i class="fas fa-building"></i> Empresas
            </li>
        </ol>
    </nav>
}
```

## Option 5: Dashboard Card/Widget

```html
<div class="col-md-4">
    <div class="card text-white bg-primary mb-3">
        <div class="card-header">
            <i class="fas fa-building"></i> Empresas
        </div>
        <div class="card-body">
            <h5 class="card-title">Gestión de Empresas</h5>
            <p class="card-text">Configure y administre las empresas del sistema.</p>
            <a asp-page="/Empresas" class="btn btn-light">
                <i class="fas fa-arrow-right"></i> Ir a Empresas
            </a>
        </div>
    </div>
</div>
```

## Recommended: Full Admin Sidebar Navigation

Create a new partial view: `_AdminNavigation.cshtml`

```html
@if (User.Identity?.IsAuthenticated == true)
{
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container-fluid">
            <a class="navbar-brand" asp-page="/Index">
                <i class="fas fa-receipt"></i> Facturación
            </a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav me-auto">
                    <li class="nav-item">
                        <a class="nav-link" asp-page="/Index">
                            <i class="fas fa-home"></i> Dashboard
                        </a>
                    </li>

                    @if (User.IsInRole("SuperUser"))
                    {
                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                                <i class="fas fa-cog"></i> Configuración
                            </a>
                            <ul class="dropdown-menu">
                                <li>
                                    <a class="dropdown-item" asp-page="/Empresas">
                                        <i class="fas fa-building"></i> Empresas
                                    </a>
                                </li>
                                <li><hr class="dropdown-divider"></li>
                                <li>
                                    <a class="dropdown-item" asp-page="/Usuarios">
                                        <i class="fas fa-users"></i> Usuarios
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item" asp-page="/Roles">
                                        <i class="fas fa-user-shield"></i> Roles
                                    </a>
                                </li>
                            </ul>
                        </li>
                    }

                    @if (User.IsInRole("Admin") || User.IsInRole("SuperUser"))
                    {
                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                                <i class="fas fa-file-invoice"></i> Facturación
                            </a>
                            <ul class="dropdown-menu">
                                <li>
                                    <a class="dropdown-item" asp-page="/Facturas">
                                        <i class="fas fa-file-invoice-dollar"></i> Facturas
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item" asp-page="/Clientes">
                                        <i class="fas fa-users"></i> Clientes
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item" asp-page="/Productos">
                                        <i class="fas fa-box"></i> Productos
                                    </a>
                                </li>
                            </ul>
                        </li>
                    }
                </ul>

                <ul class="navbar-nav ms-auto">
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                            <i class="fas fa-user"></i> @User.Identity?.Name
                        </a>
                        <ul class="dropdown-menu dropdown-menu-end">
                            <li>
                                <a class="dropdown-item" asp-page="/Profile">
                                    <i class="fas fa-user-circle"></i> Mi Perfil
                                </a>
                            </li>
                            <li><hr class="dropdown-divider"></li>
                            <li>
                                <a class="dropdown-item" asp-page="/Auth/Logout">
                                    <i class="fas fa-sign-out-alt"></i> Cerrar Sesión
                                </a>
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>
    </nav>
}
```

Then include it in _Layout.cshtml:

```html
<header>
    <partial name="_AdminNavigation" />
</header>
```

## Active Menu Item Highlighting

Add this JavaScript to your layout or site.js:

```javascript
// Highlight active menu item
$(document).ready(function() {
    var path = window.location.pathname;
    $('.navbar-nav .nav-link').each(function() {
        var href = $(this).attr('href');
        if (path === href) {
            $(this).addClass('active');
        }
    });
});
```

And add this CSS:

```css
.navbar-nav .nav-link.active {
    background-color: rgba(255, 255, 255, 0.1);
    border-radius: 4px;
    font-weight: 600;
}

.dropdown-item.active {
    background-color: #4e73df;
    color: white;
}
```

## Bootstrap Icons Required

Make sure Font Awesome is loaded in your _Layout.cshtml:

```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
```

Or use Bootstrap Icons:

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
```

Then replace icons:
- `fas fa-building` → `bi bi-building`
- `fas fa-home` → `bi bi-house-door`
- `fas fa-cog` → `bi bi-gear`

## Role-Based Menu

The navigation automatically shows/hides based on user role:

```csharp
@if (User.IsInRole("SuperUser"))
{
    // Only SuperUser can see Empresas
}

@if (User.IsInRole("Admin") || User.IsInRole("SuperUser"))
{
    // Admin and SuperUser can see
}

@if (User.Identity?.IsAuthenticated == true)
{
    // All authenticated users can see
}
```

## Summary

**Recommended Approach:**
1. Add Empresas link to existing navigation menu
2. Wrap in `@if (User.IsInRole("SuperUser"))` check
3. Use Font Awesome icon: `<i class="fas fa-building"></i>`
4. Place under "Configuración" or "Administración" section
5. Add active state highlighting with CSS

**Example Final Code:**
```html
@if (User.IsInRole("SuperUser"))
{
    <li class="nav-item">
        <a class="nav-link" asp-page="/Empresas">
            <i class="fas fa-building"></i> Empresas
        </a>
    </li>
}
```

This ensures only SuperUser role can access the Empresas management page, matching the `[Authorize(Roles = "SuperUser")]` attribute on the PageModel.
