/**
 * Theme Toggle - Dark Mode Support
 * Sistema de Facturación CR v4.4
 */

(function() {
    'use strict';

    // Theme constants
    const THEME_KEY = 'facturacion-theme';
    const THEME_LIGHT = 'light';
    const THEME_DARK = 'dark';
    const THEME_AUTO = 'auto';

    /**
     * Get the user's preferred theme from localStorage
     * Always defaults to LIGHT mode unless user explicitly changed it
     */
    function getPreferredTheme() {
        const storedTheme = localStorage.getItem(THEME_KEY);

        // Always default to light mode if no preference is stored
        // User must explicitly switch to dark mode
        return storedTheme || THEME_LIGHT;
    }

    /**
     * Apply theme to the document
     */
    function applyTheme(theme) {
        if (theme === THEME_DARK) {
            document.documentElement.setAttribute('data-theme', 'dark');
        } else {
            document.documentElement.removeAttribute('data-theme');
        }
    }

    /**
     * Save theme preference to localStorage
     */
    function saveTheme(theme) {
        localStorage.setItem(THEME_KEY, theme);
    }

    /**
     * Toggle between light and dark themes
     */
    function toggleTheme() {
        const currentTheme = getPreferredTheme();
        const newTheme = currentTheme === THEME_LIGHT ? THEME_DARK : THEME_LIGHT;

        applyTheme(newTheme);
        saveTheme(newTheme);

        // Update toggle button UI if it exists
        updateToggleButton(newTheme);

        // Dispatch custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: newTheme }
        }));

        return newTheme;
    }

    /**
     * Update the toggle button UI to reflect current theme
     */
    function updateToggleButton(theme) {
        const toggleBtn = document.getElementById('themeToggle');
        if (!toggleBtn) return;

        const icon = toggleBtn.querySelector('i');
        if (!icon) return;

        if (theme === THEME_DARK) {
            icon.className = 'fas fa-moon';
            toggleBtn.setAttribute('aria-label', 'Cambiar a modo claro');
            toggleBtn.setAttribute('title', 'Modo claro');
        } else {
            icon.className = 'fas fa-sun';
            toggleBtn.setAttribute('aria-label', 'Cambiar a modo oscuro');
            toggleBtn.setAttribute('title', 'Modo oscuro');
        }
    }

    /**
     * Initialize theme on page load
     */
    function initTheme() {
        const preferredTheme = getPreferredTheme();
        applyTheme(preferredTheme);
        updateToggleButton(preferredTheme);
    }

    /**
     * Setup event listeners
     */
    function setupEventListeners() {
        // Listen for toggle button clicks
        document.addEventListener('click', function(e) {
            if (e.target.closest('#themeToggle')) {
                e.preventDefault();
                toggleTheme();
            }
        });

        // Listen for keyboard shortcut (Ctrl/Cmd + Shift + D)
        document.addEventListener('keydown', function(e) {
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'D') {
                e.preventDefault();
                toggleTheme();
            }
        });
    }

    /**
     * Get current theme
     */
    function getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') === 'dark' ? THEME_DARK : THEME_LIGHT;
    }

    // Initialize theme immediately (before DOM ready) to prevent flash
    initTheme();

    // Setup event listeners when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', setupEventListeners);
    } else {
        setupEventListeners();
    }

    // Expose API to window for external use
    window.ThemeManager = {
        toggle: toggleTheme,
        get: getCurrentTheme,
        set: function(theme) {
            applyTheme(theme);
            saveTheme(theme);
            updateToggleButton(theme);
        },
        LIGHT: THEME_LIGHT,
        DARK: THEME_DARK
    };

})();
