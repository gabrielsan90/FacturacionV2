/**
 * Sistema de Notificaciones In-App
 * Gestiona la visualización y gestión de notificaciones en tiempo real
 * Utiliza Page Handlers de Razor Pages (NO llamadas directas al API)
 */

const NotificationSystem = (function () {
    // Configuración
    const config = {
        refreshInterval: 30000, // 30 segundos
        maxNotificationsShown: 10,
        pageUrl: '/Sistema/Notificaciones'
    };

    // Estado
    let refreshTimer = null;
    let currentEmpresaId = null;

    /**
     * Inicializa el sistema de notificaciones
     */
    function init() {
        // Obtener empresa actual desde cookie o claim
        currentEmpresaId = getCurrentEmpresaId();

        if (!currentEmpresaId) {
            console.warn('No empresa ID found, notifications disabled');
            return;
        }

        // Cargar notificaciones iniciales
        loadNotifications();

        // Configurar actualización automática
        startAutoRefresh();

        // Configurar event handlers
        setupEventHandlers();
    }

    /**
     * Obtiene el ID de la empresa actual
     */
    function getCurrentEmpresaId() {
        // Intentar obtener del claim en el header
        const empresaIdElement = document.querySelector('[data-empresa-id]');
        if (empresaIdElement) {
            return empresaIdElement.getAttribute('data-empresa-id');
        }

        // Intentar obtener de cookie
        const empresaIdCookie = getCookie('EmpresaId');
        if (empresaIdCookie) {
            return empresaIdCookie;
        }

        // Intentar obtener del currentCompanyName dropdown
        const companySelector = document.getElementById('companySelector');
        if (companySelector) {
            const empresaId = companySelector.getAttribute('data-empresa-id');
            if (empresaId) {
                return empresaId;
            }
        }

        return null;
    }

    /**
     * Obtiene una cookie por nombre
     */
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    /**
     * Obtiene el token antiforgery para POST requests
     */
    function getAntiForgeryToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    /**
     * Carga las notificaciones desde el page handler
     */
    async function loadNotifications() {
        try {
            const response = await fetch(`${config.pageUrl}?handler=Resumen&empresaId=${currentEmpresaId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin'
            });

            if (!response.ok) {
                console.error('Failed to load notifications:', response.status);
                return;
            }

            const result = await response.json();

            if (result.success && result.data) {
                updateBadge(result.data.totalNoLeidas);
                updateNotificationList(result.data.notificacionesRecientes);
            } else {
                updateBadge(0);
                updateNotificationList([]);
            }

        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    /**
     * Actualiza el badge de notificaciones
     */
    function updateBadge(count) {
        const badge = document.getElementById('notificationBadge');
        if (!badge) return;

        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }

    /**
     * Actualiza la lista de notificaciones
     */
    function updateNotificationList(notifications) {
        const listContainer = document.getElementById('notificationList');
        if (!listContainer) return;

        // Limpiar lista actual
        listContainer.innerHTML = '';

        if (!notifications || notifications.length === 0) {
            listContainer.innerHTML = `
                <div class="notification-item text-center py-4">
                    <i class="fas fa-inbox text-muted" style="font-size: 2rem;"></i>
                    <p class="text-muted mt-2 mb-0">No hay notificaciones</p>
                </div>
            `;
            return;
        }

        // Mostrar solo las primeras N notificaciones
        const notificationsToShow = notifications.slice(0, config.maxNotificationsShown);

        notificationsToShow.forEach(notif => {
            const item = createNotificationItem(notif);
            listContainer.appendChild(item);
        });
    }

    /**
     * Crea un elemento de notificación
     */
    function createNotificationItem(notif) {
        const div = document.createElement('div');
        div.className = `notification-item d-flex ${notif.leida ? 'read' : 'unread'}`;
        div.setAttribute('data-notification-id', notif.id);
        div.style.cursor = 'pointer';

        // Determinar clase de color basada en el tipo
        const colorClass = notif.color || 'info';

        div.innerHTML = `
            <div class="notification-icon ${colorClass}">
                <i class="${notif.icono || 'fas fa-bell'}"></i>
            </div>
            <div class="flex-grow-1">
                <div class="fw-semibold ${notif.importante ? 'text-danger' : ''}">
                    ${notif.titulo}
                    ${notif.importante ? '<i class="fas fa-exclamation-circle ms-1"></i>' : ''}
                </div>
                <div class="small text-muted">${notif.mensaje}</div>
                <div class="small text-muted">${notif.tiempoTranscurrido || ''}</div>
            </div>
            ${!notif.leida ? '<div class="unread-indicator"></div>' : ''}
        `;

        // Agregar event listener
        div.addEventListener('click', () => handleNotificationClick(notif));

        return div;
    }

    /**
     * Maneja el click en una notificación
     */
    async function handleNotificationClick(notif) {
        // Marcar como leída si no lo está
        if (!notif.leida) {
            await markAsRead(notif.id);
        }

        // Navegar si tiene URL de acción
        if (notif.urlAccion) {
            window.location.href = notif.urlAccion;
        }

        // Cerrar dropdown
        const dropdown = document.getElementById('notificationDropdown');
        if (dropdown) {
            const bsDropdown = bootstrap.Dropdown.getInstance(dropdown);
            if (bsDropdown) {
                bsDropdown.hide();
            }
        }
    }

    /**
     * Marca una notificación como leída via page handler
     */
    async function markAsRead(notificationId) {
        try {
            const response = await fetch(`${config.pageUrl}?handler=MarcarLeida&id=${notificationId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                credentials: 'same-origin'
            });

            if (response.ok) {
                await loadNotifications();
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    /**
     * Marca todas las notificaciones como leídas
     */
    async function markAllAsRead() {
        try {
            const response = await fetch(`${config.pageUrl}?handler=MarcarTodasLeidas&empresaId=${currentEmpresaId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                credentials: 'same-origin'
            });

            if (response.ok) {
                // Mostrar mensaje de éxito
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Notificaciones marcadas',
                        text: 'Todas las notificaciones han sido marcadas como leídas',
                        timer: 2000,
                        showConfirmButton: false,
                        toast: true,
                        position: 'top-end'
                    });
                }

                // Recargar notificaciones
                await loadNotifications();
            } else {
                console.error('Failed to mark all as read:', response.status);
            }
        } catch (error) {
            console.error('Error marking all as read:', error);
        }
    }

    /**
     * Configura event handlers
     */
    function setupEventHandlers() {
        // Botón "Marcar todas como leídas"
        const markAllButton = document.getElementById('markAllNotificationsRead');
        if (markAllButton) {
            markAllButton.addEventListener('click', async (e) => {
                e.preventDefault();
                await markAllAsRead();
            });
        }

        // Recargar al abrir el dropdown
        const dropdown = document.getElementById('notificationDropdown');
        if (dropdown) {
            dropdown.addEventListener('show.bs.dropdown', () => {
                loadNotifications();
            });
        }
    }

    /**
     * Inicia la actualización automática
     */
    function startAutoRefresh() {
        if (refreshTimer) {
            clearInterval(refreshTimer);
        }

        refreshTimer = setInterval(() => {
            loadNotifications();
        }, config.refreshInterval);
    }

    /**
     * Detiene la actualización automática
     */
    function stopAutoRefresh() {
        if (refreshTimer) {
            clearInterval(refreshTimer);
            refreshTimer = null;
        }
    }

    /**
     * Recarga las notificaciones manualmente
     */
    function refresh() {
        loadNotifications();
    }

    /**
     * Actualiza la empresa actual
     */
    function setEmpresa(empresaId) {
        if (empresaId !== currentEmpresaId) {
            currentEmpresaId = empresaId;
            loadNotifications();
        }
    }

    // API Pública
    return {
        init: init,
        refresh: refresh,
        markAllAsRead: markAllAsRead,
        setEmpresa: setEmpresa,
        stopAutoRefresh: stopAutoRefresh
    };
})();

// Función global para compatibilidad con el layout actual
function markAllAsRead() {
    NotificationSystem.markAllAsRead();
}

// Auto-inicializar cuando el DOM esté listo
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
        NotificationSystem.init();
    });
} else {
    NotificationSystem.init();
}
