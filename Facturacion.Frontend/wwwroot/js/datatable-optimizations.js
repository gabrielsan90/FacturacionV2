/**
 * DataTable Optimizations - Sistema de Facturación CR v4.4
 * Performance improvements, lazy loading, and virtual scrolling
 */

(function(window, $) {
    'use strict';

    /**
     * Default configuration for optimized DataTables
     */
    const defaultConfig = {
        // Performance
        deferRender: true,
        processing: true,
        serverSide: false, // Change to true for very large datasets

        // Pagination
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'Todos']],

        // Responsive
        responsive: true,
        autoWidth: false,

        // Styling
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"tr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',

        // Language
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json',
            processing: '<div class="spinner-modern mx-auto"></div><p class="mt-3 text-muted">Procesando...</p>',
            loadingRecords: '<div class="spinner-modern mx-auto"></div><p class="mt-3 text-muted">Cargando...</p>',
            emptyTable: '<div class="empty-state"><div class="empty-state-icon"><i class="fas fa-inbox"></i></div><h3 class="empty-state-title">No hay datos disponibles</h3></div>'
        },

        // Performance callbacks
        initComplete: function(settings, json) {
            const api = this.api();
            const tableId = settings.nTable.id;

            // Hide loader
            const loader = document.getElementById(tableId + 'Loader');
            if (loader) loader.style.display = 'none';

            // Show table or empty state
            const container = document.getElementById(tableId + 'Container');
            const emptyState = document.getElementById('emptyState' + tableId.replace('table', ''));

            if (api.data().count() > 0) {
                if (container) container.style.display = 'block';
                if (emptyState) emptyState.style.display = 'none';
            } else {
                if (container) container.style.display = 'none';
                if (emptyState) emptyState.style.display = 'block';
            }

            // Update badge count
            const badge = document.getElementById('badgeTotal' + tableId.replace('table', ''));
            if (badge) badge.textContent = api.data().count();

            // Announce to screen readers
            if (window.Accessibility) {
                window.Accessibility.announce(`Tabla cargada con ${api.data().count()} registros`);
            }
        },

        drawCallback: function(settings) {
            // Re-initialize tooltips after draw
            if (typeof bootstrap !== 'undefined') {
                const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
                [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
            }
        }
    };

    /**
     * Create an optimized DataTable
     * @param {string} selector - Table selector
     * @param {object} config - Additional configuration
     * @returns {object} DataTable instance
     */
    function createOptimizedTable(selector, config = {}) {
        const finalConfig = $.extend(true, {}, defaultConfig, config);

        // Add export buttons if requested
        if (config.export) {
            finalConfig.dom = '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                              '<"row"<"col-sm-12 col-md-6"B><"col-sm-12 col-md-6">>' +
                              '<"row"<"col-sm-12"tr>>' +
                              '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>';

            finalConfig.buttons = [
                {
                    extend: 'excelHtml5',
                    text: '<i class="fas fa-file-excel me-1"></i> Excel',
                    className: 'btn btn-success btn-sm',
                    exportOptions: {
                        columns: ':visible:not(.no-export)'
                    }
                },
                {
                    extend: 'pdfHtml5',
                    text: '<i class="fas fa-file-pdf me-1"></i> PDF',
                    className: 'btn btn-danger btn-sm',
                    exportOptions: {
                        columns: ':visible:not(.no-export)'
                    },
                    customize: function(doc) {
                        doc.defaultStyle.fontSize = 9;
                        doc.styles.tableHeader.fontSize = 10;
                        doc.styles.tableHeader.bold = true;
                    }
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print me-1"></i> Imprimir',
                    className: 'btn btn-info btn-sm',
                    exportOptions: {
                        columns: ':visible:not(.no-export)'
                    }
                }
            ];
        }

        return $(selector).DataTable(finalConfig);
    }

    /**
     * Create a server-side DataTable for large datasets
     * @param {string} selector - Table selector
     * @param {string} ajaxUrl - AJAX URL for data
     * @param {array} columns - Column definitions
     * @param {object} config - Additional configuration
     * @returns {object} DataTable instance
     */
    function createServerSideTable(selector, ajaxUrl, columns, config = {}) {
        const serverConfig = {
            serverSide: true,
            processing: true,
            ajax: {
                url: ajaxUrl,
                type: 'GET',
                data: function(d) {
                    // Add custom filters
                    if (config.customFilters) {
                        $.extend(d, config.customFilters());
                    }
                    return d;
                },
                error: function(xhr, error, code) {
                    console.error('DataTable Error:', error);
                    if (window.FacturacionHelpers) {
                        window.FacturacionHelpers.showToast('Error al cargar datos', 'error');
                    }
                }
            },
            columns: columns
        };

        return createOptimizedTable(selector, $.extend({}, serverConfig, config));
    }

    /**
     * Add search debouncing to improve performance
     * @param {object} table - DataTable instance
     * @param {number} delay - Delay in milliseconds
     */
    function addSearchDebounce(table, delay = 500) {
        let searchTimeout;

        $('div.dataTables_filter input').off('keyup.DT input.DT');
        $('div.dataTables_filter input').on('keyup', function() {
            const searchValue = this.value;
            clearTimeout(searchTimeout);

            searchTimeout = setTimeout(function() {
                table.search(searchValue).draw();
            }, delay);
        });
    }

    /**
     * Enable column filtering
     * @param {object} table - DataTable instance
     * @param {array} columns - Array of column indices to enable filtering
     */
    function addColumnFilters(table, columns = []) {
        const tfoot = $(table.table().node()).find('tfoot');

        if (tfoot.length === 0) {
            $(table.table().node()).append('<tfoot><tr></tr></tfoot>');
        }

        const footerRow = $(table.table().node()).find('tfoot tr');
        footerRow.empty();

        table.columns().every(function(index) {
            const column = this;
            const header = $(column.header()).text();

            if (columns.length === 0 || columns.includes(index)) {
                const select = $('<select class="form-select form-select-sm"><option value="">Filtrar ' + header + '</option></select>')
                    .appendTo(footerRow.append('<th></th>').find('th:last'))
                    .on('change', function() {
                        const val = $.fn.dataTable.util.escapeRegex($(this).val());
                        column.search(val ? '^' + val + '$' : '', true, false).draw();
                    });

                column.data().unique().sort().each(function(d) {
                    if (d) {
                        // Extract text from HTML if needed
                        const text = $('<div>').html(d).text();
                        select.append('<option value="' + text + '">' + text + '</option>');
                    }
                });
            } else {
                footerRow.append('<th></th>');
            }
        });
    }

    /**
     * Add row details/child rows functionality
     * @param {object} table - DataTable instance
     * @param {function} formatFunction - Function to format child row content
     */
    function addRowDetails(table, formatFunction) {
        $(table.table().body()).on('click', 'td.details-control', function() {
            const tr = $(this).closest('tr');
            const row = table.row(tr);

            if (row.child.isShown()) {
                row.child.hide();
                tr.removeClass('shown');
                $(this).find('i').removeClass('fa-minus-circle').addClass('fa-plus-circle');
            } else {
                row.child(formatFunction(row.data())).show();
                tr.addClass('shown');
                $(this).find('i').removeClass('fa-plus-circle').addClass('fa-minus-circle');
            }
        });
    }

    /**
     * Enable row selection
     * @param {object} table - DataTable instance
     * @param {string} mode - 'single' or 'multiple'
     */
    function enableRowSelection(table, mode = 'single') {
        $(table.table().body()).on('click', 'tr', function() {
            if (mode === 'single') {
                if ($(this).hasClass('selected')) {
                    $(this).removeClass('selected');
                } else {
                    table.$('tr.selected').removeClass('selected');
                    $(this).addClass('selected');
                }
            } else {
                $(this).toggleClass('selected');
            }
        });
    }

    /**
     * Get selected rows data
     * @param {object} table - DataTable instance
     * @returns {array} Array of selected row data
     */
    function getSelectedRows(table) {
        return table.rows('.selected').data().toArray();
    }

    /**
     * Refresh table data
     * @param {object} table - DataTable instance
     * @param {boolean} resetPaging - Whether to reset to page 1
     */
    function refreshTable(table, resetPaging = false) {
        if (window.Accessibility) {
            window.Accessibility.announce('Actualizando tabla...');
        }

        table.ajax.reload(function() {
            if (window.Accessibility) {
                window.Accessibility.announce('Tabla actualizada');
            }
            if (window.FacturacionHelpers) {
                window.FacturacionHelpers.showToast('Tabla actualizada', 'success');
            }
        }, resetPaging);
    }

    /**
     * Export table to Excel (client-side)
     * @param {object} table - DataTable instance
     * @param {string} filename - Filename for export
     */
    function exportToExcel(table, filename = 'export') {
        const data = table.buttons.exportData();
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.aoa_to_sheet([data.header].concat(data.body));

        XLSX.utils.book_append_sheet(wb, ws, 'Datos');
        XLSX.writeFile(wb, filename + '.xlsx');
    }

    /**
     * Add global search highlighting
     * @param {object} table - DataTable instance
     */
    function addSearchHighlighting(table) {
        table.on('draw', function() {
            const body = $(table.table().body());

            body.unhighlight();

            const searchTerm = table.search();
            if (searchTerm) {
                body.highlight(searchTerm);
            }
        });
    }

    /**
     * Create a simple configuration helper
     */
    const presets = {
        basic: defaultConfig,

        withExport: $.extend(true, {}, defaultConfig, { export: true }),

        serverSide: $.extend(true, {}, defaultConfig, {
            serverSide: true,
            processing: true
        }),

        compact: $.extend(true, {}, defaultConfig, {
            pageLength: 10,
            lengthMenu: [[10, 25, 50], [10, 25, 50]],
            dom: '<"row"<"col-sm-12"f>><"row"<"col-sm-12"tr>><"row"<"col-sm-12"p>>'
        }),

        noFilter: $.extend(true, {}, defaultConfig, {
            searching: false,
            dom: '<"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>'
        })
    };

    // Expose API
    window.DataTableOptimizations = {
        createOptimizedTable,
        createServerSideTable,
        addSearchDebounce,
        addColumnFilters,
        addRowDetails,
        enableRowSelection,
        getSelectedRows,
        refreshTable,
        exportToExcel,
        addSearchHighlighting,
        presets,
        defaultConfig
    };

    // jQuery plugin
    $.fn.optimizedDataTable = function(config) {
        return createOptimizedTable(this, config);
    };

})(window, jQuery);

/**
 * Global helper functions for DataTable operations
 */

// Export table to Excel (global function for onclick)
function exportarExcel() {
    const table = $('.datatable, table[id^="table"]').DataTable();
    if (table && table.buttons) {
        table.button('.buttons-excel').trigger();
    } else {
        console.warn('Table or export buttons not found');
        if (window.FacturacionHelpers) {
            window.FacturacionHelpers.showToast('Función de exportación no disponible', 'warning');
        }
    }
}

// Export table to PDF (global function for onclick)
function exportarPDF() {
    const table = $('.datatable, table[id^="table"]').DataTable();
    if (table && table.buttons) {
        table.button('.buttons-pdf').trigger();
    } else {
        console.warn('Table or export buttons not found');
        if (window.FacturacionHelpers) {
            window.FacturacionHelpers.showToast('Función de exportación no disponible', 'warning');
        }
    }
}

// Refresh table (global function for onclick)
function refrescarTabla() {
    const tables = $.fn.dataTable.tables({ api: true });
    tables.ajax.reload(null, false); // Reload without resetting paging

    if (window.FacturacionHelpers) {
        window.FacturacionHelpers.showToast('Tabla actualizada', 'success');
    }
}
