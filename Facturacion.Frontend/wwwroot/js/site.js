// Global configuration
const API_BASE_URL = 'https://localhost:7001';

/**
 * Get JWT token from Claims (stored in authentication cookie)
 * The token is stored in the claims when user logs in
 */
function getJwtToken() {
    // Token is retrieved from claims by the PageModel and passed to the page
    // For client-side calls, we'll use a hidden field or meta tag
    const tokenElement = document.querySelector('meta[name="jwt-token"]');
    if (tokenElement) {
        return tokenElement.getAttribute('content');
    }
    return null;
}

/**
 * Get Anti-Forgery Token for POST requests
 */
function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').val();
}

/**
 * Generic AJAX call helper with JWT authentication
 * @param {string} method - HTTP method (GET, POST, PUT, DELETE)
 * @param {string} url - API endpoint URL (relative to API_BASE_URL)
 * @param {object} data - Request data (will be JSON stringified)
 * @param {object} options - Additional jQuery AJAX options
 * @returns {Promise} jQuery AJAX promise
 */
function apiCall(method, url, data = null, options = {}) {
    const token = getJwtToken();

    const defaultOptions = {
        url: API_BASE_URL + url,
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        dataType: 'json'
    };

    // Add JWT token if available
    if (token) {
        defaultOptions.headers['Authorization'] = 'Bearer ' + token;
    }

    // Add data if provided
    if (data !== null) {
        if (method === 'GET') {
            defaultOptions.data = data;
        } else {
            defaultOptions.data = JSON.stringify(data);
        }
    }

    // Merge with custom options
    const finalOptions = $.extend(true, {}, defaultOptions, options);

    return $.ajax(finalOptions);
}

/**
 * GET request helper
 */
function apiGet(url, params = null) {
    return apiCall('GET', url, params);
}

/**
 * POST request helper
 */
function apiPost(url, data) {
    return apiCall('POST', url, data);
}

/**
 * PUT request helper
 */
function apiPut(url, data) {
    return apiCall('PUT', url, data);
}

/**
 * DELETE request helper
 */
function apiDelete(url) {
    return apiCall('DELETE', url);
}

/**
 * Success notification
 * @param {string} message - Success message to display
 * @param {string} title - Optional title (defaults to "Éxito")
 */
function showSuccess(message, title = '¡Éxito!') {
    Swal.fire({
        icon: 'success',
        title: title,
        text: message,
        timer: 3000,
        showConfirmButton: false,
        position: 'top-end',
        toast: true
    });
}

/**
 * Error notification
 * @param {string} message - Error message to display
 * @param {string} title - Optional title (defaults to "Error")
 */
function showError(message, title = 'Error') {
    Swal.fire({
        icon: 'error',
        title: title,
        text: message,
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#667eea'
    });
}

/**
 * Warning notification
 * @param {string} message - Warning message to display
 * @param {string} title - Optional title (defaults to "Advertencia")
 */
function showWarning(message, title = 'Advertencia') {
    Swal.fire({
        icon: 'warning',
        title: title,
        text: message,
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#f59e0b'
    });
}

/**
 * Info notification
 * @param {string} message - Info message to display
 * @param {string} title - Optional title (defaults to "Información")
 */
function showInfo(message, title = 'Información') {
    Swal.fire({
        icon: 'info',
        title: title,
        text: message,
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#3b82f6'
    });
}

/**
 * Confirmation dialog
 * @param {string} message - Message to display
 * @param {string} title - Dialog title
 * @param {function} onConfirm - Callback function when confirmed
 */
function confirmDialog(message, title = '¿Está seguro?', onConfirm) {
    Swal.fire({
        title: title,
        text: message,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, confirmar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#667eea',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed && typeof onConfirm === 'function') {
            onConfirm();
        }
    });
}

/**
 * Delete confirmation dialog
 * @param {string} message - Message to display
 * @param {function} onConfirm - Callback function when confirmed
 */
function confirmDelete(message, onConfirm) {
    Swal.fire({
        title: '¿Está seguro?',
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed && typeof onConfirm === 'function') {
            onConfirm();
        }
    });
}

/**
 * Show loading spinner
 * @param {string} message - Loading message
 */
function showLoading(message = 'Cargando...') {
    Swal.fire({
        title: message,
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

/**
 * Hide loading spinner
 */
function hideLoading() {
    Swal.close();
}

/**
 * Format number as Costa Rican currency (Colones)
 * @param {number} amount - Amount to format
 * @returns {string} Formatted currency string
 */
function formatCurrency(amount) {
    return '₡' + parseFloat(amount).toLocaleString('es-CR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

/**
 * Format number as US currency (Dollars)
 * @param {number} amount - Amount to format
 * @returns {string} Formatted currency string
 */
function formatDollars(amount) {
    return '$' + parseFloat(amount).toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

/**
 * Format date to Costa Rican format (DD/MM/YYYY)
 * @param {string|Date} date - Date to format
 * @returns {string} Formatted date string
 */
function formatDate(date) {
    if (!date) return '';
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
}

/**
 * Format date and time to Costa Rican format (DD/MM/YYYY HH:mm)
 * @param {string|Date} date - Date to format
 * @returns {string} Formatted date-time string
 */
function formatDateTime(date) {
    if (!date) return '';
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
}

/**
 * Parse DD/MM/YYYY to ISO date string
 * @param {string} dateString - Date string in DD/MM/YYYY format
 * @returns {string} ISO date string
 */
function parseCostaRicanDate(dateString) {
    if (!dateString) return null;
    const parts = dateString.split('/');
    if (parts.length !== 3) return null;
    return `${parts[2]}-${parts[1]}-${parts[0]}`;
}

/**
 * Initialize Select2 with common settings
 * @param {string} selector - jQuery selector
 * @param {object} options - Additional Select2 options
 */
function initSelect2(selector, options = {}) {
    const defaultOptions = {
        theme: 'bootstrap-5',
        language: 'es',
        width: '100%',
        placeholder: 'Seleccione...',
        allowClear: true
    };

    $(selector).select2($.extend({}, defaultOptions, options));
}

/**
 * Initialize Select2 with AJAX search
 * @param {string} selector - jQuery selector
 * @param {string} url - API endpoint for search
 * @param {function} processResults - Function to process results
 */
function initSelect2Ajax(selector, url, processResults) {
    $(selector).select2({
        theme: 'bootstrap-5',
        language: 'es',
        width: '100%',
        placeholder: 'Buscar...',
        allowClear: true,
        ajax: {
            url: API_BASE_URL + url,
            dataType: 'json',
            delay: 250,
            headers: {
                'Authorization': 'Bearer ' + getJwtToken()
            },
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page || 1
                };
            },
            processResults: processResults,
            cache: true
        },
        minimumInputLength: 2
    });
}

/**
 * Initialize DataTable with common settings in Spanish
 * @param {string} selector - jQuery selector
 * @param {object} options - Additional DataTable options
 * @returns {object} DataTable instance
 */
function initDataTable(selector, options = {}) {
    const defaultOptions = {
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
        },
        responsive: true,
        autoWidth: false,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Todos"]],
        order: [[0, 'desc']],
        dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
             "<'row'<'col-sm-12'tr>>" +
             "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>"
    };

    return $(selector).DataTable($.extend(true, {}, defaultOptions, options));
}

/**
 * Handle AJAX errors with user-friendly messages
 * @param {object} xhr - jQuery XHR object
 */
function handleAjaxError(xhr) {
    let errorMessage = 'Ha ocurrido un error inesperado';

    if (xhr.status === 0) {
        errorMessage = 'No se pudo conectar al servidor. Verifique su conexión.';
    } else if (xhr.status === 400) {
        errorMessage = 'Solicitud inválida. Verifique los datos enviados.';
        if (xhr.responseJSON && xhr.responseJSON.errors) {
            const errors = Object.values(xhr.responseJSON.errors).flat();
            errorMessage += '\n' + errors.join('\n');
        }
    } else if (xhr.status === 401) {
        errorMessage = 'No está autenticado. Inicie sesión nuevamente.';
        setTimeout(() => {
            window.location.href = '/Auth/Login';
        }, 2000);
    } else if (xhr.status === 403) {
        errorMessage = 'No tiene permisos para realizar esta acción.';
    } else if (xhr.status === 404) {
        errorMessage = 'Recurso no encontrado.';
    } else if (xhr.status === 500) {
        errorMessage = 'Error interno del servidor. Contacte al administrador.';
    } else if (xhr.responseJSON && xhr.responseJSON.message) {
        errorMessage = xhr.responseJSON.message;
    } else if (xhr.responseText) {
        try {
            const response = JSON.parse(xhr.responseText);
            if (response.message) {
                errorMessage = response.message;
            }
        } catch (e) {
            // If not JSON, use the text as is
            errorMessage = xhr.responseText;
        }
    }

    showError(errorMessage);
}

/**
 * Validate Costa Rican ID (Cédula)
 * @param {string} id - ID number to validate
 * @returns {boolean} True if valid
 */
function validateCostaRicanId(id) {
    if (!id) return false;

    // Remove spaces and dashes
    id = id.replace(/[\s-]/g, '');

    // Must be 9 digits
    if (!/^\d{9}$/.test(id)) return false;

    // First digit must be 1-9
    if (id[0] === '0') return false;

    return true;
}

/**
 * Format Costa Rican ID (Cédula)
 * @param {string} id - ID number
 * @returns {string} Formatted ID (X-XXXX-XXXX)
 */
function formatCostaRicanId(id) {
    if (!id) return '';

    id = id.replace(/[\s-]/g, '');

    if (id.length === 9) {
        return `${id.substr(0, 1)}-${id.substr(1, 4)}-${id.substr(5, 4)}`;
    }

    return id;
}

/**
 * Validate email address
 * @param {string} email - Email to validate
 * @returns {boolean} True if valid
 */
function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

/**
 * Debounce function for search inputs
 * @param {function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @returns {function} Debounced function
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Download file from blob
 * @param {Blob} blob - File blob
 * @param {string} filename - File name
 */
function downloadFile(blob, filename) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

/**
 * Global AJAX error handler
 */
$(document).ajaxError(function(event, xhr, settings, error) {
    // Only handle if not already handled
    if (!settings.suppressGlobalErrorHandler) {
        handleAjaxError(xhr);
    }
});

/**
 * Global DataTable defaults
 */
$.extend(true, $.fn.dataTable.defaults, {
    language: {
        url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
    }
});

/**
 * Initialize common elements on page load
 */
$(document).ready(function() {
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert:not(.alert-permanent)').fadeOut('slow');
    }, 5000);

    // BUG #14: Warn before leaving page if a modal with a dirty form is open
    window.addEventListener('beforeunload', function(e) {
        const openModal = document.querySelector('.modal.show');
        if (openModal) {
            const form = openModal.querySelector('form');
            if (form) {
                const inputs = form.querySelectorAll('input:not([type="hidden"]), textarea, select');
                const hasData = Array.from(inputs).some(function(el) {
                    if (el.type === 'checkbox' || el.type === 'radio') return false;
                    return el.value && el.value.trim() !== '' && el.value !== el.defaultValue;
                });
                if (hasData) {
                    e.preventDefault();
                    e.returnValue = '';
                }
            }
        }
    });
});
