/**
 * Validaciones dinámicas por tipo de documento según Hacienda v4.4
 * Recomendación 1 y 4 del sistema
 */

// Configuración de campos por tipo de documento
// 1=Obligatorio, 2=Condicional, 3=Opcional, 4=Inexistente
const CamposDocumento = {
    // Factura Electrónica (01)
    FacturaElectronica: {
        codigo: '01',
        nombre: 'Factura Electrónica',
        receptor: {
            identificacion: 1,          // Obligatorio
            nombre: 1,                   // Obligatorio
            ubicacion: 1,                // Obligatorio (excepto Extranjero No Domiciliado)
            actividadEconomica: 2,       // Condicional (si es para gasto deducible)
            otrasSenasExtranjero: 2      // Condicional (si es Extranjero No Domiciliado)
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,             // Condicional (si es crédito)
        medioPago: 1,
        tipoCambio: 2,                   // Condicional (si moneda != CRC)
        lineasDetalle: true,
        exoneraciones: true,
        informacionReferencia: 4          // No aplica
    },

    // Tiquete Electrónico (04)
    TiqueteElectronico: {
        codigo: '04',
        nombre: 'Tiquete Electrónico',
        receptor: {
            identificacion: 3,          // Opcional
            nombre: 3,                   // Opcional
            ubicacion: 3,                // Opcional
            actividadEconomica: 4,       // No aplica
            otrasSenasExtranjero: 2      // Condicional (si hay receptor extranjero)
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,
        medioPago: 1,
        tipoCambio: 2,
        lineasDetalle: true,
        exoneraciones: true,
        informacionReferencia: 4
    },

    // Nota de Crédito Electrónica (03)
    NotaCreditoElectronica: {
        codigo: '03',
        nombre: 'Nota de Crédito Electrónica',
        receptor: {
            identificacion: 2,          // Condicional (debe coincidir con doc original)
            nombre: 2,
            ubicacion: 2,
            actividadEconomica: 2,
            otrasSenasExtranjero: 2
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,
        medioPago: 1,
        tipoCambio: 1,                   // OBLIGATORIO en NC v4.4
        lineasDetalle: true,
        exoneraciones: true,
        informacionReferencia: 1         // OBLIGATORIO
    },

    // Nota de Débito Electrónica (02)
    NotaDebitoElectronica: {
        codigo: '02',
        nombre: 'Nota de Débito Electrónica',
        receptor: {
            identificacion: 2,
            nombre: 2,
            ubicacion: 2,
            actividadEconomica: 2,
            otrasSenasExtranjero: 2
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,
        medioPago: 1,
        tipoCambio: 1,                   // OBLIGATORIO en ND v4.4
        lineasDetalle: true,
        exoneraciones: true,
        informacionReferencia: 1         // OBLIGATORIO
    },

    // Factura Electrónica de Compra (08)
    FacturaElectronicaCompra: {
        codigo: '08',
        nombre: 'Factura Electrónica de Compra',
        receptor: {
            identificacion: 1,          // Obligatorio (proveedor extranjero)
            nombre: 1,
            ubicacion: 2,
            actividadEconomica: 1,       // OBLIGATORIO en FEC
            otrasSenasExtranjero: 4      // No aplica en FEC
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,
        medioPago: 1,
        tipoCambio: 2,
        lineasDetalle: true,
        exoneraciones: true,
        informacionReferencia: 2
    },

    // Factura Electrónica de Exportación (09)
    FacturaElectronicaExportacion: {
        codigo: '09',
        nombre: 'Factura Electrónica de Exportación',
        receptor: {
            identificacion: 1,          // OBLIGATORIO en FEE (cambio v4.4)
            nombre: 1,
            ubicacion: 4,                // No aplica (extranjero)
            actividadEconomica: 4,       // No aplica
            otrasSenasExtranjero: 2      // Condicional
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,
        medioPago: 1,
        tipoCambio: 2,
        lineasDetalle: true,
        exoneraciones: 4,                // NO permitidas en FEE
        informacionReferencia: 4
    },

    // Recibo Electrónico de Pago (10) - NUEVO v4.4
    ReciboElectronicoPago: {
        codigo: '10',
        nombre: 'Recibo Electrónico de Pago',
        receptor: {
            identificacion: 1,          // Obligatorio
            nombre: 1,
            ubicacion: 4,                // No aplica
            actividadEconomica: 4,       // No aplica
            otrasSenasExtranjero: 4      // No aplica
        },
        condicionVenta: 1,
        plazoCreditoDias: 2,
        medioPago: 1,
        tipoCambio: 2,
        lineasDetalle: false,            // REP NO tiene detalle
        exoneraciones: 4,
        informacionReferencia: 1         // OBLIGATORIO (factura que se paga)
    }
};

// Validación de identificación por tipo
const ValidacionesIdentificacion = {
    '01': { nombre: 'Cédula Física', longitud: 9, regex: /^\d{9}$/ },
    '02': { nombre: 'Cédula Jurídica', longitud: 10, regex: /^\d{10}$/ },
    '03': { nombre: 'DIMEX', minLongitud: 11, maxLongitud: 12, regex: /^\d{11,12}$/ },
    '04': { nombre: 'NITE', longitud: 10, regex: /^\d{10}$/ },
    '05': { nombre: 'Extranjero No Domiciliado', maxLongitud: 20, regex: /^.{1,20}$/ },
    '06': { nombre: 'No Contribuyente', maxLongitud: 20, regex: /^.{1,20}$/ }
};

/**
 * Aplica configuración de campos según el tipo de documento
 */
function aplicarConfiguracionCampos(tipoDocumento) {
    const config = CamposDocumento[tipoDocumento];
    if (!config) {
        console.warn('Tipo de documento no reconocido:', tipoDocumento);
        return;
    }

    console.log('Aplicando configuración para:', config.nombre);

    // Configurar sección de receptor
    configurarSeccionReceptor(config.receptor, tipoDocumento);

    // Configurar campos de condición de venta
    configurarCondicionVenta(config);

    // Configurar información de referencia
    configurarInformacionReferencia(config.informacionReferencia);

    // Configurar líneas de detalle
    configurarLineasDetalle(config.lineasDetalle);

    // Configurar exoneraciones
    configurarExoneraciones(config.exoneraciones);

    // Actualizar indicadores visuales
    actualizarIndicadoresObligatorios(config);
}

/**
 * Configura la sección de receptor según tipo de documento
 */
function configurarSeccionReceptor(configReceptor, tipoDocumento) {
    // Sección de receptor
    const seccionReceptor = document.getElementById('seccionReceptor');
    if (!seccionReceptor) return;

    // Mostrar/ocultar sección completa
    if (tipoDocumento === 'TiqueteElectronico') {
        seccionReceptor.classList.add('opcional');
        document.getElementById('lblReceptorSeccion')?.classList.remove('required');
    } else {
        seccionReceptor.classList.remove('opcional');
        document.getElementById('lblReceptorSeccion')?.classList.add('required');
    }

    // Campos de identificación
    const campoIdentificacion = document.getElementById('receptorIdentificacion');
    const campoNombre = document.getElementById('receptorNombre');

    if (campoIdentificacion) {
        campoIdentificacion.required = configReceptor.identificacion === 1;
        actualizarLabelRequerido(campoIdentificacion, configReceptor.identificacion);
    }

    if (campoNombre) {
        campoNombre.required = configReceptor.nombre === 1;
        actualizarLabelRequerido(campoNombre, configReceptor.nombre);
    }

    // Campos de ubicación
    const seccionUbicacion = document.getElementById('seccionUbicacionReceptor');
    if (seccionUbicacion) {
        if (configReceptor.ubicacion === 4) {
            seccionUbicacion.style.display = 'none';
        } else {
            seccionUbicacion.style.display = 'block';
            const campoProvincia = document.getElementById('receptorProvincia');
            if (campoProvincia) {
                campoProvincia.required = configReceptor.ubicacion === 1;
            }
        }
    }

    // Actividad económica del receptor
    const campoActividadEconomica = document.getElementById('receptorActividadEconomica');
    if (campoActividadEconomica) {
        if (configReceptor.actividadEconomica === 4) {
            campoActividadEconomica.closest('.mb-3')?.style.display = 'none';
        } else {
            campoActividadEconomica.closest('.mb-3')?.style.display = 'block';
            campoActividadEconomica.required = configReceptor.actividadEconomica === 1;
            actualizarLabelRequerido(campoActividadEconomica, configReceptor.actividadEconomica);
        }
    }

    // Otras señas extranjero
    const campoOtrasSenasExtranjero = document.getElementById('receptorOtrasSenasExtranjero');
    if (campoOtrasSenasExtranjero) {
        if (configReceptor.otrasSenasExtranjero === 4) {
            campoOtrasSenasExtranjero.closest('.mb-3')?.style.display = 'none';
        }
    }
}

/**
 * Configura campos de condición de venta
 */
function configurarCondicionVenta(config) {
    const condicionVenta = document.getElementById('condicionVenta');
    const plazoCreditoDias = document.getElementById('plazoCreditoDias');

    if (condicionVenta) {
        condicionVenta.addEventListener('change', function () {
            // Mostrar plazo de crédito si condición es "Crédito" (02)
            const esCredito = this.value === '02';
            if (plazoCreditoDias) {
                const contenedor = plazoCreditoDias.closest('.mb-3');
                if (contenedor) {
                    contenedor.style.display = esCredito ? 'block' : 'none';
                }
                plazoCreditoDias.required = esCredito;
            }
        });
    }
}

/**
 * Configura sección de información de referencia
 */
function configurarInformacionReferencia(condicion) {
    const seccionReferencia = document.getElementById('seccionInformacionReferencia');
    if (!seccionReferencia) return;

    if (condicion === 4) {
        // No aplica - ocultar sección
        seccionReferencia.style.display = 'none';
    } else if (condicion === 1) {
        // Obligatorio - mostrar y marcar como requerido
        seccionReferencia.style.display = 'block';
        seccionReferencia.classList.add('requerido');
        document.getElementById('tipoDocumentoReferencia')?.setAttribute('required', 'true');
        document.getElementById('numeroReferencia')?.setAttribute('required', 'true');
        document.getElementById('fechaEmisionReferencia')?.setAttribute('required', 'true');
        document.getElementById('codigoReferencia')?.setAttribute('required', 'true');
        document.getElementById('razonReferencia')?.setAttribute('required', 'true');
    } else {
        // Condicional u opcional
        seccionReferencia.style.display = 'block';
        seccionReferencia.classList.remove('requerido');
    }
}

/**
 * Configura sección de líneas de detalle
 */
function configurarLineasDetalle(permitido) {
    const seccionLineas = document.getElementById('seccionLineasDetalle');
    if (!seccionLineas) return;

    if (!permitido) {
        // REP no tiene líneas de detalle
        seccionLineas.style.display = 'none';
    } else {
        seccionLineas.style.display = 'block';
    }
}

/**
 * Configura sección de exoneraciones
 */
function configurarExoneraciones(condicion) {
    const seccionExoneraciones = document.getElementById('seccionExoneraciones');
    if (!seccionExoneraciones) return;

    if (condicion === 4) {
        // No permitido (FEE, REP)
        seccionExoneraciones.style.display = 'none';
    } else {
        seccionExoneraciones.style.display = 'block';
    }
}

/**
 * Actualiza indicadores visuales de campos obligatorios
 */
function actualizarIndicadoresObligatorios(config) {
    // Agregar asterisco a labels de campos obligatorios
    document.querySelectorAll('.label-requerido').forEach(label => {
        const asterisco = label.querySelector('.asterisco');
        if (asterisco) asterisco.remove();
    });
}

/**
 * Actualiza el label de un campo según su condición
 */
function actualizarLabelRequerido(campo, condicion) {
    if (!campo) return;

    const label = document.querySelector(`label[for="${campo.id}"]`);
    if (!label) return;

    // Remover asterisco existente
    const asterisco = label.querySelector('.asterisco');
    if (asterisco) asterisco.remove();

    // Agregar asterisco si es obligatorio
    if (condicion === 1) {
        const span = document.createElement('span');
        span.className = 'asterisco text-danger';
        span.textContent = ' *';
        label.appendChild(span);
    }
}

/**
 * Valida el formato de identificación según el tipo
 */
function validarIdentificacion(tipoIdentificacion, numero) {
    const config = ValidacionesIdentificacion[tipoIdentificacion];
    if (!config) {
        return { valido: false, mensaje: 'Tipo de identificación no reconocido' };
    }

    // Limpiar número
    numero = numero.replace(/[-\s]/g, '').trim();

    // Validar con regex
    if (!config.regex.test(numero)) {
        if (config.longitud) {
            return {
                valido: false,
                mensaje: `${config.nombre} debe tener exactamente ${config.longitud} dígitos`
            };
        } else {
            return {
                valido: false,
                mensaje: `${config.nombre} debe tener entre ${config.minLongitud} y ${config.maxLongitud} caracteres`
            };
        }
    }

    return { valido: true, mensaje: '' };
}

/**
 * Valida los totales del documento
 */
function validarTotales(documento) {
    const errores = [];
    const TOLERANCIA = 0.01;

    let totalCalculado = 0;

    documento.lineas.forEach((linea, index) => {
        // Validar cálculo de línea
        const montoEsperado = linea.cantidad * linea.precioUnitario;
        if (Math.abs(linea.montoTotal - montoEsperado) > TOLERANCIA) {
            errores.push(`Línea ${index + 1}: Monto total incorrecto`);
        }

        // Validar descuento no exceda monto
        if (linea.montoDescuento > linea.montoTotal) {
            errores.push(`Línea ${index + 1}: El descuento no puede ser mayor al monto total`);
        }

        totalCalculado += (linea.montoTotal - linea.montoDescuento + linea.montoImpuesto);
    });

    // Validar total general
    if (Math.abs(totalCalculado - documento.totalVenta) > TOLERANCIA) {
        errores.push(`El total de venta no coincide con la suma de las líneas`);
    }

    return errores;
}

/**
 * Valida campos obligatorios según tipo de documento
 */
function validarCamposObligatorios(tipoDocumento, datos) {
    const config = CamposDocumento[tipoDocumento];
    if (!config) {
        return ['Tipo de documento no válido'];
    }

    const errores = [];

    // Validar receptor según configuración
    if (config.receptor.identificacion === 1 && !datos.receptorIdentificacion) {
        errores.push('La identificación del receptor es obligatoria');
    }

    if (config.receptor.nombre === 1 && !datos.receptorNombre) {
        errores.push('El nombre del receptor es obligatorio');
    }

    // Validar información de referencia si es obligatoria
    if (config.informacionReferencia === 1) {
        if (!datos.tipoDocumentoReferencia) {
            errores.push('El tipo de documento de referencia es obligatorio');
        }
        if (!datos.numeroReferencia) {
            errores.push('El número de referencia es obligatorio');
        }
        if (!datos.fechaEmisionReferencia) {
            errores.push('La fecha de emisión del documento de referencia es obligatoria');
        }
        if (!datos.codigoReferencia) {
            errores.push('El código de referencia es obligatorio');
        }
        if (!datos.razonReferencia) {
            errores.push('La razón de la referencia es obligatoria');
        }
    }

    // Validar tipo de cambio en NC/ND
    if ((tipoDocumento === 'NotaCreditoElectronica' || tipoDocumento === 'NotaDebitoElectronica') &&
        datos.moneda !== 'CRC' && !datos.tipoCambio) {
        errores.push('El tipo de cambio es obligatorio en Notas de Crédito/Débito');
    }

    // Validar plazo de crédito
    if (datos.condicionVenta === '02' && !datos.plazoCreditoDias) {
        errores.push('El plazo de crédito es obligatorio cuando la condición de venta es Crédito');
    }

    // Validar actividad económica en FEC
    if (tipoDocumento === 'FacturaElectronicaCompra' && !datos.receptorActividadEconomica) {
        errores.push('La actividad económica del receptor es obligatoria en Factura Electrónica de Compra');
    }

    // Validar que FEE no tenga exoneraciones
    if (tipoDocumento === 'FacturaElectronicaExportacion' && datos.tieneExoneraciones) {
        errores.push('Las exoneraciones no son permitidas en Factura Electrónica de Exportación');
    }

    // Validar que REP no tenga líneas de detalle
    if (tipoDocumento === 'ReciboElectronicoPago' && datos.lineas && datos.lineas.length > 0) {
        errores.push('El Recibo Electrónico de Pago no debe tener líneas de detalle');
    }

    return errores;
}

/**
 * Valida código CAByS
 */
function validarCodigoCAByS(codigo) {
    if (!codigo) return { valido: false, mensaje: 'El código CAByS es obligatorio' };

    // Limpiar código
    codigo = codigo.replace(/\s/g, '');

    // Validar longitud
    if (codigo.length !== 13) {
        return { valido: false, mensaje: 'El código CAByS debe tener exactamente 13 dígitos' };
    }

    // Validar que sean solo números
    if (!/^\d+$/.test(codigo)) {
        return { valido: false, mensaje: 'El código CAByS debe contener solo números' };
    }

    return { valido: true, mensaje: '' };
}

/**
 * Genera la clave numérica de 50 dígitos
 */
function generarClaveNumerica(params) {
    const {
        fecha,              // Date
        identificacionEmisor, // string (cédula del emisor)
        consecutivo,        // string (formato XXX-YYYYY-ZZ-AAAAAAAAAA)
        situacion = 1       // 1=Normal, 2=Contingencia, 3=Sin internet
    } = params;

    // País (3 dígitos)
    let clave = '506';

    // Fecha (6 dígitos: ddmmyy)
    clave += fecha.getDate().toString().padStart(2, '0');
    clave += (fecha.getMonth() + 1).toString().padStart(2, '0');
    clave += fecha.getFullYear().toString().slice(-2);

    // Identificación emisor (12 dígitos)
    clave += identificacionEmisor.replace(/\D/g, '').padStart(12, '0');

    // Consecutivo (20 dígitos, sin guiones)
    clave += consecutivo.replace(/-/g, '');

    // Situación (1 dígito)
    clave += situacion.toString();

    // Código de seguridad (8 dígitos aleatorios)
    clave += Math.floor(10000000 + Math.random() * 90000000).toString();

    return clave;
}

/**
 * Formatea el consecutivo según Hacienda
 */
function formatearConsecutivo(sucursal, terminal, tipoDocumento, secuencial) {
    // Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
    return `${sucursal.toString().padStart(3, '0')}-${terminal.toString().padStart(5, '0')}-${tipoDocumento.toString().padStart(2, '0')}-${secuencial.toString().padStart(10, '0')}`;
}

// Exportar funciones para uso global
window.DocumentoValidaciones = {
    CamposDocumento,
    ValidacionesIdentificacion,
    aplicarConfiguracionCampos,
    validarIdentificacion,
    validarTotales,
    validarCamposObligatorios,
    validarCodigoCAByS,
    generarClaveNumerica,
    formatearConsecutivo
};
