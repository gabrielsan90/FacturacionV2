/**
 * Helper Functions - Sistema de Facturación CR v4.4
 * Common utility functions for formatting and data manipulation
 */

(function(window) {
    'use strict';

    /**
     * Format currency with proper symbol based on currency type
     * @param {number} value - The amount to format
     * @param {string} currency - Currency code (CRC, USD, EUR)
     * @returns {string} Formatted currency string
     */
    function formatCurrency(value, currency = 'CRC') {
        if (value === null || value === undefined) return '₡0.00';

        const symbols = {
            'CRC': '₡',
            'USD': '$',
            'EUR': '€',
            '1': '₡',  // TipoMoneda enum values
            '2': '$',
            '3': '€'
        };

        const symbol = symbols[currency] || '₡';
        const formatted = parseFloat(value).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');

        return `${symbol}${formatted}`;
    }

    /**
     * Format date to Costa Rica locale (dd/MM/yyyy)
     * @param {string|Date} date - The date to format
     * @returns {string} Formatted date string
     */
    function formatDate(date) {
        if (!date) return '';

        const d = new Date(date);
        if (isNaN(d.getTime())) return '';

        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = d.getFullYear();

        return `${day}/${month}/${year}`;
    }

    /**
     * Format datetime to Costa Rica locale with time
     * @param {string|Date} datetime - The datetime to format
     * @returns {string} Formatted datetime string
     */
    function formatDateTime(datetime) {
        if (!datetime) return '';

        const d = new Date(datetime);
        if (isNaN(d.getTime())) return '';

        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = d.getFullYear();
        const hours = String(d.getHours()).padStart(2, '0');
        const minutes = String(d.getMinutes()).padStart(2, '0');

        return `${day}/${month}/${year} ${hours}:${minutes}`;
    }

    /**
     * Format time only (HH:mm)
     * @param {string|Date} datetime - The datetime to format
     * @returns {string} Formatted time string
     */
    function formatTime(datetime) {
        if (!datetime) return '';

        const d = new Date(datetime);
        if (isNaN(d.getTime())) return '';

        const hours = String(d.getHours()).padStart(2, '0');
        const minutes = String(d.getMinutes()).padStart(2, '0');

        return `${hours}:${minutes}`;
    }

    /**
     * Format identification number (cédula, DIMEX, etc.)
     * @param {string} numero - The identification number
     * @param {number} tipo - Type of identification
     * @returns {string} Formatted identification
     */
    function formatIdentificacion(numero, tipo) {
        if (!numero) return '';

        // Remove any existing formatting
        const clean = numero.replace(/[^0-9]/g, '');

        // Format based on type
        switch(tipo) {
            case 1: // Física (9 digits: 0-0000-0000)
                if (clean.length === 9) {
                    return `${clean.substring(0,1)}-${clean.substring(1,5)}-${clean.substring(5)}`;
                }
                break;
            case 2: // Jurídica (10 digits: 0-000-000000)
                if (clean.length === 10) {
                    return `${clean.substring(0,1)}-${clean.substring(1,4)}-${clean.substring(4)}`;
                }
                break;
            case 3: // DIMEX (11-12 digits)
                return clean;
            case 4: // NITE (10 digits)
                return clean;
        }

        return numero;
    }

    /**
     * Get badge HTML for document type
     * @param {string} tipo - Document type code
     * @returns {string} HTML badge element
     */
    function getBadgeTipoDocumento(tipo) {
        const badges = {
            '1': { text: 'FE', color: '#0ea5e9', title: 'Factura Electrónica' },
            '2': { text: 'ND', color: '#ef4444', title: 'Nota de Débito' },
            '3': { text: 'NC', color: '#f59e0b', title: 'Nota de Crédito' },
            '4': { text: 'TE', color: '#06b6d4', title: 'Tiquete Electrónico' },
            '8': { text: 'FCC', color: '#64748b', title: 'Factura Compra' },
            '9': { text: 'FEE', color: '#10b981', title: 'Factura Exportación' },
            'FE': { text: 'FE', color: '#0ea5e9', title: 'Factura Electrónica' },
            'ND': { text: 'ND', color: '#ef4444', title: 'Nota de Débito' },
            'NC': { text: 'NC', color: '#f59e0b', title: 'Nota de Crédito' },
            'TE': { text: 'TE', color: '#06b6d4', title: 'Tiquete Electrónico' },
            'FEE': { text: 'FEE', color: '#10b981', title: 'Factura Exportación' }
        };

        const badge = badges[tipo] || { text: tipo, color: '#64748b', title: tipo };
        return `<span class="badge" style="background: ${badge.color}; color: white;" title="${badge.title}">${badge.text}</span>`;
    }

    /**
     * Get badge HTML for Hacienda status
     * @param {string} estado - Status code or text
     * @returns {string} HTML badge element
     */
    function getBadgeEstadoHacienda(estado) {
        const badges = {
            'Aceptado': { icon: 'fa-check-circle', color: '#10b981', text: 'Aceptado' },
            'Rechazado': { icon: 'fa-times-circle', color: '#ef4444', text: 'Rechazado' },
            'Pendiente': { icon: 'fa-clock', color: '#f59e0b', text: 'Pendiente' },
            'Procesando': { icon: 'fa-spinner', color: '#0ea5e9', text: 'Procesando' },
            'Error': { icon: 'fa-exclamation-triangle', color: '#ef4444', text: 'Error' },
            'Borrador': { icon: 'fa-file', color: '#64748b', text: 'Borrador' }
        };

        const badge = badges[estado] || { icon: 'fa-circle', color: '#64748b', text: estado };
        return `<span class="badge" style="background: ${badge.color}; color: white;"><i class="fas ${badge.icon} me-1"></i>${badge.text}</span>`;
    }

    /**
     * Get badge HTML for ambiente (environment)
     * @param {number|string} ambiente - Environment (1=Producción, 2=Pruebas)
     * @returns {string} HTML badge element
     */
    function getBadgeAmbiente(ambiente) {
        if (ambiente === 1 || ambiente === '1' || ambiente === 'Producción') {
            return '<span class="badge" style="background: #10b981; color: white;"><i class="fas fa-shield-alt me-1"></i>Producción</span>';
        } else {
            return '<span class="badge" style="background: #f59e0b; color: white;"><i class="fas fa-flask me-1"></i>Pruebas</span>';
        }
    }

    /**
     * Truncate text with ellipsis
     * @param {string} text - Text to truncate
     * @param {number} length - Maximum length
     * @returns {string} Truncated text
     */
    function truncateText(text, length = 50) {
        if (!text) return '';
        if (text.length <= length) return text;
        return text.substring(0, length) + '...';
    }

    /**
     * Show loading state
     * @param {string} elementId - ID of element to show loading in
     * @param {string} message - Optional loading message
     */
    function showLoading(elementId, message = 'Cargando...') {
        const element = document.getElementById(elementId);
        if (!element) return;

        element.innerHTML = `
            <div class="text-center py-5">
                <div class="spinner-modern mx-auto"></div>
                <p class="text-muted mt-3">${message}</p>
            </div>
        `;
    }

    /**
     * Show empty state
     * @param {string} elementId - ID of element to show empty state in
     * @param {object} options - Configuration options
     */
    function showEmptyState(elementId, options = {}) {
        const defaults = {
            icon: 'fa-inbox',
            title: 'No hay datos',
            description: 'No se encontraron resultados',
            buttonText: null,
            buttonAction: null
        };

        const config = { ...defaults, ...options };
        const element = document.getElementById(elementId);
        if (!element) return;

        let buttonHTML = '';
        if (config.buttonText && config.buttonAction) {
            buttonHTML = `<button class="btn btn-primary mt-3" onclick="${config.buttonAction}">
                <i class="fas fa-plus me-2"></i>${config.buttonText}
            </button>`;
        }

        element.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">
                    <i class="fas ${config.icon}"></i>
                </div>
                <h3 class="empty-state-title">${config.title}</h3>
                <p class="empty-state-description">${config.description}</p>
                ${buttonHTML}
            </div>
        `;
    }

    /**
     * Show toast notification
     * @param {string} message - Message to display
     * @param {string} type - Type of notification (success, error, warning, info)
     */
    function showToast(message, type = 'info') {
        const icons = {
            success: 'success',
            error: 'error',
            warning: 'warning',
            info: 'info'
        };

        Swal.fire({
            icon: icons[type] || 'info',
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true
        });
    }

    /**
     * Confirm action dialog
     * @param {string} title - Dialog title
     * @param {string} text - Dialog text
     * @param {function} onConfirm - Callback on confirm
     */
    function confirmAction(title, text, onConfirm) {
        Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0ea5e9',
            cancelButtonColor: '#64748b',
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed && typeof onConfirm === 'function') {
                onConfirm();
            }
        });
    }

    /**
     * Debounce function
     * @param {function} func - Function to debounce
     * @param {number} wait - Wait time in milliseconds
     * @returns {function} Debounced function
     */
    function debounce(func, wait = 300) {
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
     * Copy text to clipboard
     * @param {string} text - Text to copy
     * @param {string} successMessage - Success message to show
     */
    function copyToClipboard(text, successMessage = 'Copiado al portapapeles') {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text).then(() => {
                showToast(successMessage, 'success');
            }).catch(() => {
                fallbackCopyToClipboard(text, successMessage);
            });
        } else {
            fallbackCopyToClipboard(text, successMessage);
        }
    }

    /**
     * Fallback copy to clipboard for older browsers
     */
    function fallbackCopyToClipboard(text, successMessage) {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            document.execCommand('copy');
            showToast(successMessage, 'success');
        } catch (err) {
            showToast('Error al copiar', 'error');
        }

        document.body.removeChild(textArea);
    }

    /**
     * Validate email format
     * @param {string} email - Email to validate
     * @returns {boolean} True if valid
     */
    function isValidEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    /**
     * Validate Costa Rica cedula (9 digits)
     * @param {string} cedula - Cedula to validate
     * @returns {boolean} True if valid
     */
    function isValidCedula(cedula) {
        const clean = cedula.replace(/[^0-9]/g, '');
        return clean.length === 9;
    }

    // Expose functions to global scope
    window.FacturacionHelpers = {
        formatCurrency,
        formatDate,
        formatDateTime,
        formatTime,
        formatIdentificacion,
        getBadgeTipoDocumento,
        getBadgeEstadoHacienda,
        getBadgeAmbiente,
        truncateText,
        showLoading,
        showEmptyState,
        showToast,
        confirmAction,
        debounce,
        copyToClipboard,
        isValidEmail,
        isValidCedula
    };

    // Also expose individual functions for backwards compatibility
    window.formatCurrency = formatCurrency;
    window.formatDate = formatDate;
    window.formatDateTime = formatDateTime;
    window.formatTime = formatTime;

})(window);
