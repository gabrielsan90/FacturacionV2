---
name: dotnet-frontend
description: Build Razor Pages UI with jQuery, Bootstrap 5, and DataTables. Use when creating pages, forms, CRUD modals, data tables, AJAX handlers, or any user interface work in the Frontend project.
---

# .NET Frontend Developer

Implement user interfaces with ASP.NET Core Razor Pages, jQuery and Bootstrap.

## Folder Structure

```
[ProjectName].Frontend/
├── Pages/
│   ├── Auth/                    # Login, Logout, Register
│   ├── Entities/                # Pages by entity
│   │   ├── Entities.cshtml
│   │   └── Entities.cshtml.cs
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── Services/
│   └── ApiService.cs
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/                     # Bootstrap, jQuery, DataTables
└── Program.cs
```

## PageModel with Full CRUD

```csharp
[Authorize]
public class EntitiesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EntitiesModel> _logger;

    public EntitiesModel(IHttpClientFactory httpClientFactory, ILogger<EntitiesModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.GetAsync("api/entity");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<EntityDto>>();
                return new JsonResult(new { success = true, data });
            }
            return new JsonResult(new { success = false, message = "Error fetching data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnGetDataAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] EntityDto dto)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            HttpResponseMessage response = dto.Id == 0
                ? await client.PostAsJsonAsync("api/entity", dto)
                : await client.PutAsJsonAsync($"api/entity/{dto.Id}", dto);

            if (response.IsSuccessStatusCode)
                return new JsonResult(new { success = true, message = "Saved successfully" });

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnPostSaveAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            var response = await client.DeleteAsync($"api/entity/{id}");
            return response.IsSuccessStatusCode
                ? new JsonResult(new { success = true, message = "Deleted successfully" })
                : new JsonResult(new { success = false, message = "Error deleting" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnPostDeleteAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
```

## Razor Page with DataTables and Modal

```html
@page
@model EntitiesModel
@{ ViewData["Title"] = "Entity Management"; }

<div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>@ViewData["Title"]</h1>
        <button class="btn btn-primary" onclick="openModal()">
            <i class="bi bi-plus-lg"></i> New
        </button>
    </div>

    <div class="card">
        <div class="card-body">
            <table id="tblData" class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>

<!-- Modal -->
<div class="modal fade" id="modalForm" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modalTitle">New</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="formData">
                    <input type="hidden" id="txtId" value="0">
                    <div class="mb-3">
                        <label for="txtName" class="form-label">Name</label>
                        <input type="text" class="form-control" id="txtName" required>
                    </div>
                    <div class="mb-3">
                        <div class="form-check">
                            <input type="checkbox" class="form-check-input" id="chkActive" checked>
                            <label class="form-check-label" for="chkActive">Active</label>
                        </div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" onclick="save()">Save</button>
            </div>
        </div>
    </div>
</div>

@section Scripts {
<script>
let table;
const modal = new bootstrap.Modal(document.getElementById('modalForm'));

$(document).ready(function() { loadData(); });

function loadData() {
    if (table) table.destroy();
    $.ajax({
        url: '?handler=Data',
        type: 'GET',
        success: function(response) {
            if (response.success) initDataTable(response.data);
            else Swal.fire('Error', response.message, 'error');
        },
        error: () => Swal.fire('Error', 'Connection error', 'error')
    });
}

function initDataTable(data) {
    table = $('#tblData').DataTable({
        data: data,
        columns: [
            { data: 'id' },
            { data: 'name' },
            { data: 'active', render: d => d 
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>' },
            { data: null, render: d => `
                <button class="btn btn-sm btn-warning" onclick="edit(${d.id})"><i class="bi bi-pencil"></i></button>
                <button class="btn btn-sm btn-danger" onclick="remove(${d.id})"><i class="bi bi-trash"></i></button>`
            }
        ],
        language: { url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json' }
    });
}

function openModal() {
    $('#txtId').val(0);
    $('#txtName').val('');
    $('#chkActive').prop('checked', true);
    $('#modalTitle').text('New');
    modal.show();
}

function edit(id) {
    const row = table.rows().data().toArray().find(r => r.id === id);
    if (row) {
        $('#txtId').val(row.id);
        $('#txtName').val(row.name);
        $('#chkActive').prop('checked', row.active);
        $('#modalTitle').text('Edit');
        modal.show();
    }
}

function save() {
    const dto = {
        id: parseInt($('#txtId').val()),
        name: $('#txtName').val(),
        active: $('#chkActive').is(':checked')
    };
    if (!dto.name) { Swal.fire('Validation', 'Name is required', 'warning'); return; }

    $.ajax({
        url: '?handler=Save',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dto),
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function(response) {
            if (response.success) { modal.hide(); loadData(); Swal.fire('Success', response.message, 'success'); }
            else Swal.fire('Error', response.message, 'error');
        },
        error: () => Swal.fire('Error', 'Connection error', 'error')
    });
}

function remove(id) {
    Swal.fire({
        title: 'Delete?', text: 'This action cannot be undone', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, delete', cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '?handler=Delete&id=' + id,
                type: 'POST',
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: function(response) {
                    if (response.success) { loadData(); Swal.fire('Success', response.message, 'success'); }
                    else Swal.fire('Error', response.message, 'error');
                }
            });
        }
    });
}
</script>
}
```

## HttpClient Configuration (Program.cs)

```csharp
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
```

## Unbreakable Rules

1. **NEVER** use "Index.cshtml" as entity page name
2. **ALWAYS** use PascalCase plural: `Products.cshtml`
3. **ALWAYS** suffix "Async" on handlers: `OnPostSaveAsync()`
4. **ALWAYS** use `public async Task<IActionResult>` for handlers
5. **ALWAYS** include antiforgery token in AJAX POST
6. **ALWAYS** use IHttpClientFactory
7. **ALWAYS** load data via AJAX, NOT in OnGet
8. **ALWAYS** use DataTables for tables
9. **ALWAYS** use SweetAlert2 for messages
10. **ALWAYS** validate on client before sending

## Required Libraries

- Bootstrap 5.x
- jQuery 3.x  
- DataTables 1.x
- SweetAlert2
- Bootstrap Icons
