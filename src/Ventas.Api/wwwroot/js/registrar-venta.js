// Pantalla de registrar venta. Los importes los calcula el servidor (/Ventas/Cotizar) con las
// reglas del dominio; aquí solo se guardan los artículos, las cantidades y las selecciones.
(() => {
    const { getJson, postJson, mensajesError, fmt2, fmt4, esc } = window.ventas;
    const MEDIO_EFECTIVO = '1';
    const TIPO_DNI = '2';
    const TIPO_RUC = '1';

    const estado = {
        cliente: null,
        lineas: [],          // { codigoArticulo, cantidad }
        articulos: [],       // resultado de la última búsqueda
        cotizacion: null,
        tipo: 'B',
        tiposDocumento: [],
        mediosPago: [],
    };

    const listaHtml = (mensajes) => `<ul>${mensajes.map((m) => `<li>${esc(m)}</li>`).join('')}</ul>`;
    const mostrarErrores = (selector, mensajes) => $(selector).html(mensajes.length ? listaHtml(mensajes) : '');

    // ---------- 1. Cliente ----------
    const fichaCliente = (c) => `
        <dl class="ficha">
            <dt>Código</dt><dd class="codigo">${esc(c.codigo)}</dd>
            <dt>Documento</dt><dd>${c.tieneRuc ? 'RUC' : 'Doc.'} ${esc(c.numeroDocumento)}</dd>
            <dt>Nombre / razón social</dt><dd><b>${esc(c.nombre)}</b></dd>
            <dt>Dirección de facturación</dt><dd>${esc(c.direccionFacturacion) || '—'}</dd>
        </dl>`;

    const seleccionarCliente = (cliente, aviso = '') => {
        estado.cliente = cliente;
        $('#form-nuevo').prop('hidden', true);
        $('#cliente-res').html(aviso + fichaCliente(cliente));
    };

    const esDocumento = (texto) => /^\d+$/.test(texto);

    $('#form-cliente').on('submit', async (e) => {
        e.preventDefault();
        const texto = $('#num-doc').val().trim();
        estado.cliente = null;
        $('#form-nuevo').prop('hidden', true);

        try {
            const encontrados = await getJson('/Clientes/Buscar', { texto });
            if (encontrados.length === 1) {
                seleccionarCliente(encontrados[0]);
                return;
            }
            $('#cliente-res').html('');
            abrirSeleccionClientes(texto, encontrados);
        } catch (jqXHR) {
            if (jqXHR.status === 404) {
                $('#cliente-res').html(`
                    <div class="aviso aviso-err">${esc(mensajesError(jqXHR)[0])}</div>
                    <div class="acciones" style="margin-top:10px">
                        <button id="btn-nuevo-cliente" class="btn btn-sm" type="button">Registrar cliente nuevo</button>
                    </div>`);
            } else {
                $('#cliente-res').html(`<div class="aviso aviso-err">${listaHtml(mensajesError(jqXHR))}</div>`);
            }
        }
    });

    // ---------- 1.0 Ventana para elegir entre varios clientes ----------
    // Varias coincidencias por nombre, o documentos repetidos en los datos heredados
    const MAXIMO_COINCIDENCIAS = 20;
    const dialogo = document.getElementById('dlg-clientes');

    const abrirSeleccionClientes = (texto, encontrados) => {
        const motivo = esDocumento(texto) ? `con el documento ${texto}` : `que coinciden con «${texto}»`;
        const recorte = encontrados.length === MAXIMO_COINCIDENCIAS
            ? ` Se muestran los ${MAXIMO_COINCIDENCIAS} más recientes; precise la búsqueda si no está el que busca.`
            : '';
        $('#dlg-clientes-sub').text(`${encontrados.length} clientes ${motivo}.${recorte}`);
        $('#dlg-clientes-filas').html(encontrados.map((c, i) => `<tr>
            <td class="codigo">${esc(c.codigo)}</td>
            <td>${c.tieneRuc ? 'RUC' : 'Doc.'} ${esc(c.numeroDocumento)}</td>
            <td><b>${esc(c.nombre)}</b><br><span class="sub">${esc(c.direccionFacturacion) || 'Sin dirección'}</span></td>
            <td><button class="btn btn-sm" type="button" data-elegir="${i}">Elegir</button></td>
        </tr>`).join(''));
        $(dialogo).data('encontrados', encontrados);
        dialogo.showModal();
        $('#dlg-clientes-filas [data-elegir]').first().trigger('focus');
    };

    $('#dlg-clientes-filas').on('click', '[data-elegir]', (e) => {
        const encontrados = $(dialogo).data('encontrados');
        dialogo.close();
        seleccionarCliente(encontrados[Number($(e.currentTarget).data('elegir'))]);
    });

    // Cerrar sin elegir (botón, Esc o clic fuera de la ventana): se vuelve al cuadro de búsqueda
    $('#dlg-clientes-cerrar').on('click', () => dialogo.close());
    $(dialogo).on('click', (e) => { if (e.target === dialogo) dialogo.close(); });
    $(dialogo).on('close', () => { if (!estado.cliente) $('#num-doc').trigger('focus'); });

    // ---------- 1.1 Registrar cliente ----------
    // Misma regla que el servidor: DNI → natural; RUC 20… → jurídica; otro RUC → natural con negocio
    const esJuridica = () => $('#nc-tipo').val() === TIPO_RUC && $('#nc-num').val().trim().startsWith('20');

    const actualizarFormNuevo = () => {
        const ruc = $('#nc-tipo').val() === TIPO_RUC;
        $('#nc-persona').val(esJuridica() ? 'Jurídica' : ruc ? 'Natural con negocio' : 'Natural');
        $('#nc-natural').prop('hidden', esJuridica());
        $('#nc-juridica').prop('hidden', !esJuridica());
        $('#nc-dir-req').text(ruc ? '' : '(opcional)');
        $('#nc-num').attr('maxlength', ruc ? 11 : 8);
    };

    $('#cliente-res').on('click', '#btn-nuevo-cliente', () => {
        // Si se buscó por nombre, el documento queda vacío para que el operador lo ingrese
        const buscado = $('#num-doc').val().trim();
        const numero = esDocumento(buscado) ? buscado : '';
        $('#form-nuevo')[0].reset();
        $('#nc-tipo').val(numero.length === 11 ? TIPO_RUC : TIPO_DNI);
        $('#nc-num').val(numero);
        mostrarErrores('#nc-errores', []);
        actualizarFormNuevo();
        $('#form-nuevo').prop('hidden', false);
        $(numero ? '#nc-nombres' : '#nc-num').trigger('focus');
    });
    $('#nc-tipo').on('change', actualizarFormNuevo);
    $('#nc-num').on('input', actualizarFormNuevo);
    $('#nc-cancelar').on('click', () => $('#form-nuevo').prop('hidden', true));

    $('#form-nuevo').on('submit', async (e) => {
        e.preventDefault();
        const valor = (id) => $(id).val().trim() || null;
        const juridica = esJuridica();
        const datos = {
            tipoDocumento: $('#nc-tipo').val(),
            numeroDocumento: $('#nc-num').val().trim(),
            nombres: juridica ? null : valor('#nc-nombres'),
            apellidos: juridica ? null : valor('#nc-apellidos'),
            razonSocial: juridica ? valor('#nc-razon') : null,
            direccionFacturacion: valor('#nc-dir'),
            telefono: valor('#nc-tel'),
            email: valor('#nc-email'),
        };

        try {
            const cliente = await postJson('/Clientes/Registrar', datos);
            $('#num-doc').val(cliente.numeroDocumento);
            seleccionarCliente(cliente, `<div class="aviso aviso-ok">Cliente registrado con código ${esc(cliente.codigo)}.</div>`);
        } catch (jqXHR) {
            mostrarErrores('#nc-errores', mensajesError(jqXHR));
        }
    });

    // ---------- 2. Artículos ----------
    const COLUMNAS_ART = 8;

    // La tasa llega como fracción (0.18); los exonerados (0) se marcan aparte
    const tasaIgv = (tasa) => (tasa > 0 ? `${Math.round(tasa * 10000) / 100}%` : '<span class="chip chip-warn">Exonerado</span>');

    const filaArticulo = (a) => {
        const sinStock = a.stock <= 0;
        const chip = sinStock ? 'chip chip-err' : a.stock < 10 ? 'chip chip-warn' : 'chip';
        return `<tr>
            <td class="codigo">${esc(a.codigo)}</td>
            <td>${esc(a.descripcion)}</td>
            <td>${esc(a.unidadMedida)}</td>
            <td class="num">${fmt2(a.precioUnitario)}</td>
            <td class="num">${tasaIgv(a.tasaIgv)}</td>
            <td class="num"><span class="${chip}">${a.stock}</span></td>
            <td class="num"><input type="number" min="1" step="1" value="1" data-cant="${esc(a.codigo)}"
                aria-label="Cantidad de ${esc(a.codigo)}" ${sinStock ? 'disabled' : ''}></td>
            <td><button class="btn btn-sm" type="button" data-agregar="${esc(a.codigo)}"
                ${sinStock ? 'disabled title="Sin stock"' : ''}>Agregar</button></td>
        </tr>`;
    };

    // El servidor los devuelve ordenados por categoría: una fila de encabezado por cada grupo
    const renderArticulos = () => {
        if (!estado.articulos.length) {
            $('#tbody-art').html(`<tr><td colspan="${COLUMNAS_ART}" class="vacio">No hay artículos que coincidan.</td></tr>`);
            return;
        }
        const grupos = new Map();
        estado.articulos.forEach((a) => {
            if (!grupos.has(a.codigoCategoria)) grupos.set(a.codigoCategoria, { nombre: a.categoria, articulos: [] });
            grupos.get(a.codigoCategoria).articulos.push(a);
        });
        $('#tbody-art').html([...grupos.values()].map((g) => `
            <tr class="fila-grupo"><th colspan="${COLUMNAS_ART}" scope="rowgroup">${esc(g.nombre)}
                <span class="sub">(${g.articulos.length})</span></th></tr>
            ${g.articulos.map(filaArticulo).join('')}`).join(''));
    };

    const cargarCategorias = async () => {
        try {
            const categorias = await getJson('/Articulos/Categorias');
            $('#categoria-art').html(`<option value="">Todas</option>${categorias
                .map((c) => `<option value="${esc(c.codigo)}">${esc(c.descripcion)}</option>`).join('')}`);
        } catch (jqXHR) {
            $('#aviso-art').text(mensajesError(jqXHR).join(' '));
        }
    };

    const buscarArticulos = async () => {
        try {
            estado.articulos = await getJson('/Articulos/Disponibles', {
                texto: $('#buscar-art').val().trim(),
                categoria: $('#categoria-art').val() || undefined,
            });
            $('#aviso-art').text('');
        } catch (jqXHR) {
            estado.articulos = [];
            $('#aviso-art').text(mensajesError(jqXHR).join(' '));
        }
        renderArticulos();
    };

    let temporizadorBusqueda;
    $('#buscar-art').on('input', () => {
        clearTimeout(temporizadorBusqueda);
        temporizadorBusqueda = setTimeout(buscarArticulos, 300);
    });
    $('#categoria-art').on('change', buscarArticulos);

    $('#tbody-art').on('click', '[data-agregar]', async (e) => {
        const codigo = String($(e.currentTarget).data('agregar'));
        const cantidad = Number($(`[data-cant="${codigo}"]`).val());
        if (!Number.isInteger(cantidad) || cantidad <= 0) {
            $('#aviso-art').text('La cantidad debe ser un número entero mayor que 0.');
            return;
        }
        // Un artículo no se repite: si ya está, se suma la cantidad
        const existente = estado.lineas.find((l) => l.codigoArticulo === codigo);
        if (existente) existente.cantidad += cantidad;
        else estado.lineas.push({ codigoArticulo: codigo, cantidad });
        $('#aviso-art').text('');
        await cotizar();
    });

    // ---------- 3. Detalle (cotización del servidor) ----------
    let ultimaCotizacion = 0;

    const cotizar = async () => {
        const numero = ++ultimaCotizacion;
        try {
            const cotizacion = await postJson('/Ventas/Cotizar', { lineas: estado.lineas });
            if (numero !== ultimaCotizacion) return; // llegó una respuesta más reciente
            estado.cotizacion = cotizacion;
            mostrarErrores('#errores', []);
        } catch (jqXHR) {
            if (numero !== ultimaCotizacion) return;
            mostrarErrores('#errores', mensajesError(jqXHR));
        }
        renderLineas();
        renderResumen();
    };

    const renderLineas = () => {
        const lineas = estado.cotizacion?.lineas ?? [];
        $('#tbody-lineas').html(lineas.length
            ? lineas.map((l) => `<tr>
                <td>${l.linea}</td>
                <td><span class="codigo">${esc(l.codigoArticulo)}</span><br>${esc(l.descripcion)}
                    ${l.observacion ? `<div class="error-campo">${esc(l.observacion)}</div>` : ''}</td>
                <td class="num"><input type="number" min="1" step="1" value="${l.cantidad}"
                    data-linea="${esc(l.codigoArticulo)}" aria-label="Cantidad línea ${l.linea}"></td>
                <td class="num">${l.tarifa === null ? '—' : fmt4(l.tarifa)}</td>
                <td class="num">${fmt4(l.valorVenta)}</td>
                <td class="num">${fmt4(l.igv)}</td>
                <td class="num"><b>${fmt4(l.subTotal)}</b></td>
                <td><button class="btn-icono" type="button" data-quitar="${esc(l.codigoArticulo)}"
                    aria-label="Quitar ${esc(l.codigoArticulo)}">×</button></td>
            </tr>`).join('')
            : '<tr><td colspan="8" class="vacio">Agregue artículos desde la lista superior.</td></tr>');

        const c = estado.cotizacion;
        $('#tfoot-lineas').html(lineas.length ? `<tr>
            <td colspan="4">Totales</td>
            <td class="num">${fmt4(c.subTotal)}</td>
            <td class="num">${fmt4(c.montoIgv)}</td>
            <td class="num">${fmt4(c.montoTotal)}</td>
            <td></td>
        </tr>` : '');
    };

    $('#tbody-lineas').on('click', '[data-quitar]', async (e) => {
        const codigo = String($(e.currentTarget).data('quitar'));
        estado.lineas = estado.lineas.filter((l) => l.codigoArticulo !== codigo);
        await cotizar();
    });

    $('#tbody-lineas').on('change', '[data-linea]', async (e) => {
        const codigo = String($(e.currentTarget).data('linea'));
        const cantidad = Number($(e.currentTarget).val());
        if (!Number.isInteger(cantidad) || cantidad <= 0) {
            $('#aviso-art').text('La cantidad debe ser un número entero mayor que 0.');
            renderLineas();
            return;
        }
        estado.lineas.find((l) => l.codigoArticulo === codigo).cantidad = cantidad;
        $('#aviso-art').text('');
        await cotizar();
    });

    // ---------- 4. Comprobante ----------
    const consultarNumeracion = async () => {
        try {
            const n = await getJson('/Ventas/SiguienteNumero', { tipoComprobante: estado.tipo });
            $('#prev-numero').val(n.numero ? `${n.serie}-${n.numero} (provisional)` : `Sin impresora para la serie ${n.serie}`);
        } catch (jqXHR) {
            $('#prev-numero').val(mensajesError(jqXHR)[0]);
        }
    };

    $('#radios-tipo').on('change', 'input[name="tipo"]', async (e) => {
        estado.tipo = $(e.currentTarget).val();
        await consultarNumeracion();
    });

    // ---------- 5. Medio de pago ----------
    const medioSeleccionado = () => $('#medio').val();

    $('#medio').on('change', () => {
        $('#datos-banco').prop('hidden', medioSeleccionado() === MEDIO_EFECTIVO);
        $('#banco').val('');
        $('#operacion').val('');
        renderResumen();
    });

    // ---------- Resumen ----------
    const renderResumen = () => {
        const c = estado.cotizacion;
        $('#r-subtotal').text(`S/ ${fmt2(c?.subTotal)}`);
        $('#r-igv').text(`S/ ${fmt2(c?.montoIgv)}`);
        $('#r-total').text(`S/ ${fmt2(c?.montoTotal)}`);
        $('#monto').val(fmt2(c?.montoTotal)); // el pago es siempre por el total (RN-16)
        $('#r-medio').text($('#medio option:selected').text());
    };

    // ---------- Registrar ----------
    const validar = () => {
        const errores = [];
        if (!estado.cliente) errores.push('Seleccione el cliente.');
        if (!estado.lineas.length) errores.push('Agregue al menos un artículo.');
        if (estado.cotizacion && !estado.cotizacion.valida && estado.lineas.length)
            errores.push('Corrija las líneas observadas en el detalle.');
        if (estado.tipo === 'F' && estado.cliente && !estado.cliente.tieneRuc)
            errores.push('Para emitir una factura el cliente debe tener RUC.');
        if (medioSeleccionado() !== MEDIO_EFECTIVO) {
            if (!$('#banco').val()) errores.push('Seleccione el banco.');
            if (!$('#operacion').val().trim()) errores.push('Ingrese el número de operación.');
        }
        return errores;
    };

    $('#btn-registrar').on('click', async () => {
        const errores = validar();
        mostrarErrores('#errores', errores);
        if (errores.length) return;

        const efectivo = medioSeleccionado() === MEDIO_EFECTIVO;
        const datos = {
            codigoCliente: estado.cliente.codigo,
            tipoComprobante: estado.tipo,
            lineas: estado.lineas,
            pago: {
                medioPago: Number(medioSeleccionado()),
                codigoBanco: efectivo ? null : $('#banco').val(),
                numeroOperacion: efectivo ? null : $('#operacion').val().trim(),
            },
        };

        const $boton = $('#btn-registrar').prop('disabled', true).text('Registrando…');
        try {
            // 201: se muestra el comprobante emitido con el aviso de venta registrada
            const { urlComprobante } = await postJson('/Ventas/Registrar', datos);
            location.href = `${urlComprobante}?nuevo=true`;
        } catch (jqXHR) {
            mostrarErrores('#errores', mensajesError(jqXHR));
            // Otra venta pudo consumir el stock o el precio pudo cambiar: se recalcula el detalle
            await cotizar();
            $boton.prop('disabled', false).text('Registrar venta');
        }
    });

    // ---------- Inicio ----------
    const opciones = (lista, vacio) =>
        (vacio ? `<option value="">${esc(vacio)}</option>` : '') +
        lista.map((x) => `<option value="${esc(x.valor)}">${esc(x.descripcion)}</option>`).join('');

    const iniciar = async () => {
        try {
            const [tiposDocumento, tiposComprobante, mediosPago, bancos] = await Promise.all([
                getJson('/Catalogos/TiposDocumento'),
                getJson('/Catalogos/TiposComprobante'),
                getJson('/Catalogos/MediosPago'),
                getJson('/Catalogos/Bancos'),
            ]);
            // Solo se registran clientes con DNI o RUC
            $('#nc-tipo').html(opciones(tiposDocumento.filter((t) => t.valor === TIPO_DNI || t.valor === TIPO_RUC)));
            $('#radios-tipo').html(tiposComprobante.map((t) => `
                <label class="radio"><input type="radio" name="tipo" value="${esc(t.valor)}"
                    ${t.valor === estado.tipo ? 'checked' : ''}> ${esc(t.descripcion)}</label>`).join(''));
            $('#medio').html(opciones(mediosPago)).val(MEDIO_EFECTIVO);
            $('#banco').html(opciones(bancos, 'Seleccione…'));
        } catch (jqXHR) {
            mostrarErrores('#errores', mensajesError(jqXHR));
        }

        await Promise.all([cargarCategorias(), buscarArticulos(), cotizar(), consultarNumeracion()]);
    };

    iniciar();
})();
