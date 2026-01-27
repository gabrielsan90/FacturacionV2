/**
 * Accessibility Enhancement - Sistema de Facturación CR v4.4
 * ARIA labels, keyboard navigation, and screen reader support
 */

(function(window) {
    'use strict';

    /**
     * Initialize accessibility features
     */
    function init() {
        enhanceFormAccessibility();
        enhanceTableAccessibility();
        enhanceModalAccessibility();
        setupKeyboardNavigation();
        setupFocusManagement();
        setupSkipLinks();
        announcePageChanges();
    }

    /**
     * Enhance form accessibility
     */
    function enhanceFormAccessibility() {
        // Add aria-required to required fields
        document.querySelectorAll('[required]').forEach(field => {
            field.setAttribute('aria-required', 'true');
        });

        // Add aria-invalid to invalid fields
        document.querySelectorAll('.is-invalid').forEach(field => {
            field.setAttribute('aria-invalid', 'true');
        });

        // Connect labels with inputs
        document.querySelectorAll('input, select, textarea').forEach(field => {
            if (!field.id) return;

            const label = document.querySelector(`label[for="${field.id}"]`);
            if (label && !field.getAttribute('aria-label') && !field.getAttribute('aria-labelledby')) {
                field.setAttribute('aria-labelledby', field.id + '_label');
                label.id = field.id + '_label';
            }
        });

        // Add descriptive text for form groups
        document.querySelectorAll('.form-group, .mb-3').forEach(group => {
            const label = group.querySelector('label');
            const input = group.querySelector('input, select, textarea');
            const helpText = group.querySelector('.form-text, small');

            if (input && helpText && !helpText.id) {
                const helpId = 'help_' + (input.id || Math.random().toString(36).substr(2, 9));
                helpText.id = helpId;
                input.setAttribute('aria-describedby', helpId);
            }
        });
    }

    /**
     * Enhance table accessibility
     */
    function enhanceTableAccessibility() {
        document.querySelectorAll('table').forEach(table => {
            // Add role and aria-label if missing
            if (!table.getAttribute('role')) {
                table.setAttribute('role', 'table');
            }

            const caption = table.querySelector('caption');
            if (!caption && !table.getAttribute('aria-label')) {
                const cardHeader = table.closest('.card')?.querySelector('.card-header');
                if (cardHeader) {
                    const labelId = 'table_label_' + Math.random().toString(36).substr(2, 9);
                    cardHeader.id = labelId;
                    table.setAttribute('aria-labelledby', labelId);
                }
            }

            // Enhance headers
            table.querySelectorAll('thead th').forEach(th => {
                if (!th.getAttribute('scope')) {
                    th.setAttribute('scope', 'col');
                }
            });

            // Add row index for screen readers
            table.querySelectorAll('tbody tr').forEach((tr, index) => {
                if (!tr.getAttribute('aria-rowindex')) {
                    tr.setAttribute('aria-rowindex', index + 2); // +2 because header is row 1
                }
            });
        });
    }

    /**
     * Enhance modal accessibility
     */
    function enhanceModalAccessibility() {
        document.querySelectorAll('.modal').forEach(modal => {
            // Ensure modal has role and aria-modal
            if (!modal.getAttribute('role')) {
                modal.setAttribute('role', 'dialog');
            }
            if (!modal.getAttribute('aria-modal')) {
                modal.setAttribute('aria-modal', 'true');
            }

            // Connect modal with its label
            const modalTitle = modal.querySelector('.modal-title');
            if (modalTitle && !modal.getAttribute('aria-labelledby')) {
                if (!modalTitle.id) {
                    modalTitle.id = 'modal_title_' + Math.random().toString(36).substr(2, 9);
                }
                modal.setAttribute('aria-labelledby', modalTitle.id);
            }

            // Trap focus in modal when open
            modal.addEventListener('shown.bs.modal', function() {
                trapFocus(modal);
            });

            // Restore focus when modal closes
            let previousFocus;
            modal.addEventListener('show.bs.modal', function() {
                previousFocus = document.activeElement;
            });

            modal.addEventListener('hidden.bs.modal', function() {
                if (previousFocus) {
                    previousFocus.focus();
                }
            });
        });
    }

    /**
     * Setup keyboard navigation
     */
    function setupKeyboardNavigation() {
        // ESC to close modals
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                const openModal = document.querySelector('.modal.show');
                if (openModal) {
                    const closeButton = openModal.querySelector('[data-bs-dismiss="modal"]');
                    if (closeButton) closeButton.click();
                }
            }
        });

        // Arrow navigation for DataTables
        document.addEventListener('keydown', function(e) {
            if (!e.target.closest('table')) return;

            const currentRow = e.target.closest('tr');
            if (!currentRow) return;

            let nextRow;
            switch(e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    nextRow = currentRow.nextElementSibling;
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    nextRow = currentRow.previousElementSibling;
                    break;
            }

            if (nextRow) {
                const firstFocusable = nextRow.querySelector('button, a, input, [tabindex="0"]');
                if (firstFocusable) {
                    firstFocusable.focus();
                }
            }
        });

        // Ctrl+S to save forms (prevent default browser save)
        document.addEventListener('keydown', function(e) {
            if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                const form = document.activeElement.closest('form');
                if (form) {
                    e.preventDefault();
                    const submitButton = form.querySelector('[type="submit"], .btn-primary');
                    if (submitButton) submitButton.click();
                }
            }
        });

        // Spacebar to activate buttons (in addition to Enter)
        document.addEventListener('keydown', function(e) {
            if (e.key === ' ' && e.target.tagName === 'BUTTON') {
                e.preventDefault();
                e.target.click();
            }
        });
    }

    /**
     * Trap focus within an element (for modals)
     */
    function trapFocus(element) {
        const focusableElements = element.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );

        if (focusableElements.length === 0) return;

        const firstFocusable = focusableElements[0];
        const lastFocusable = focusableElements[focusableElements.length - 1];

        // Focus first element
        firstFocusable.focus();

        element.addEventListener('keydown', function(e) {
            if (e.key !== 'Tab') return;

            if (e.shiftKey) {
                // Shift + Tab
                if (document.activeElement === firstFocusable) {
                    e.preventDefault();
                    lastFocusable.focus();
                }
            } else {
                // Tab
                if (document.activeElement === lastFocusable) {
                    e.preventDefault();
                    firstFocusable.focus();
                }
            }
        });
    }

    /**
     * Setup focus management
     */
    function setupFocusManagement() {
        // Visible focus indicators
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Tab') {
                document.body.classList.add('keyboard-navigation');
            }
        });

        document.addEventListener('mousedown', function() {
            document.body.classList.remove('keyboard-navigation');
        });

        // Focus first error on form validation
        document.querySelectorAll('form').forEach(form => {
            form.addEventListener('submit', function(e) {
                const firstError = form.querySelector('.is-invalid, [aria-invalid="true"]');
                if (firstError) {
                    setTimeout(() => {
                        firstError.focus();
                        firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }, 100);
                }
            });
        });
    }

    /**
     * Setup skip links for keyboard users
     */
    function setupSkipLinks() {
        // Add skip to main content link if not exists
        if (!document.querySelector('.skip-link')) {
            const skipLink = document.createElement('a');
            skipLink.href = '#mainContent';
            skipLink.className = 'skip-link';
            skipLink.textContent = 'Saltar al contenido principal';
            skipLink.setAttribute('aria-label', 'Saltar navegación');

            // Add styles inline
            skipLink.style.cssText = `
                position: absolute;
                top: -40px;
                left: 0;
                background: var(--primary);
                color: white;
                padding: 8px;
                text-decoration: none;
                z-index: 9999;
                border-radius: 0 0 4px 0;
            `;

            skipLink.addEventListener('focus', function() {
                this.style.top = '0';
            });

            skipLink.addEventListener('blur', function() {
                this.style.top = '-40px';
            });

            document.body.insertBefore(skipLink, document.body.firstChild);

            // Add id to main content if missing
            const mainContent = document.querySelector('.body, main, [role="main"]');
            if (mainContent && !mainContent.id) {
                mainContent.id = 'mainContent';
                mainContent.setAttribute('tabindex', '-1');
            }
        }
    }

    /**
     * Announce page changes to screen readers
     */
    function announcePageChanges() {
        // Create live region for announcements
        if (!document.getElementById('aria-live-region')) {
            const liveRegion = document.createElement('div');
            liveRegion.id = 'aria-live-region';
            liveRegion.className = 'sr-only';
            liveRegion.setAttribute('aria-live', 'polite');
            liveRegion.setAttribute('aria-atomic', 'true');
            document.body.appendChild(liveRegion);
        }
    }

    /**
     * Announce message to screen readers
     * @param {string} message - Message to announce
     * @param {string} priority - 'polite' or 'assertive'
     */
    function announce(message, priority = 'polite') {
        let liveRegion = document.getElementById('aria-live-region');

        if (!liveRegion) {
            announcePageChanges();
            liveRegion = document.getElementById('aria-live-region');
        }

        liveRegion.setAttribute('aria-live', priority);
        liveRegion.textContent = message;

        // Clear after 1 second
        setTimeout(() => {
            liveRegion.textContent = '';
        }, 1000);
    }

    /**
     * Mark element as busy/loading
     * @param {HTMLElement} element - Element to mark as busy
     * @param {boolean} busy - True if busy, false if done
     */
    function setBusy(element, busy) {
        if (busy) {
            element.setAttribute('aria-busy', 'true');
            announce('Cargando...');
        } else {
            element.removeAttribute('aria-busy');
            announce('Carga completada');
        }
    }

    /**
     * Enhanced button with loading state
     * @param {HTMLElement} button - Button element
     * @param {boolean} loading - True if loading, false if done
     */
    function setButtonLoading(button, loading) {
        if (loading) {
            button.disabled = true;
            button.setAttribute('aria-busy', 'true');
            button.dataset.originalText = button.textContent;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Procesando...';
        } else {
            button.disabled = false;
            button.removeAttribute('aria-busy');
            if (button.dataset.originalText) {
                button.textContent = button.dataset.originalText;
                delete button.dataset.originalText;
            }
        }
    }

    /**
     * Add accessible tooltips
     */
    function enhanceTooltips() {
        document.querySelectorAll('[title]').forEach(element => {
            if (!element.getAttribute('aria-label')) {
                element.setAttribute('aria-label', element.getAttribute('title'));
            }
        });
    }

    /**
     * Enhance button groups
     */
    function enhanceButtonGroups() {
        document.querySelectorAll('.btn-group').forEach(group => {
            if (!group.getAttribute('role')) {
                group.setAttribute('role', 'group');
            }

            const label = group.getAttribute('aria-label');
            if (!label) {
                const parentLabel = group.closest('[aria-label]');
                if (parentLabel) {
                    group.setAttribute('aria-label', parentLabel.getAttribute('aria-label'));
                }
            }
        });
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Re-initialize on dynamic content
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.addedNodes.length) {
                enhanceFormAccessibility();
                enhanceTableAccessibility();
                enhanceTooltips();
                enhanceButtonGroups();
            }
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    // Expose API
    window.Accessibility = {
        announce,
        setBusy,
        setButtonLoading,
        trapFocus,
        enhanceFormAccessibility,
        enhanceTableAccessibility,
        enhanceModalAccessibility
    };

})(window);
