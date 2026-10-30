// Configuración común de las llamadas jQuery: token antifalsificación, sesión expirada y errores ProblemDetails.
(() => {
    const token = $('meta[name="csrf-token"]').attr('content');
    const rutaInicioSesion = '/Cuenta/IniciarSesion';

    // Todo POST, PUT o DELETE envía el token en la cabecera que valida el servidor
    $.ajaxPrefilter((opciones, _, jqXHR) => {
        if (!/^(GET|HEAD|OPTIONS)$/i.test(opciones.type)) {
            jqXHR.setRequestHeader('RequestVerificationToken', token);
        }
    });

    // Sin sesión el servidor responde 401: se vuelve al inicio de sesión
    $(document).ajaxError((_, jqXHR) => {
        if (jqXHR.status === 401) {
            const retorno = encodeURIComponent(location.pathname + location.search);
            location.href = `${rutaInicioSesion}?ReturnUrl=${retorno}`;
        }
    });

    const formato = decimales => new Intl.NumberFormat('es-PE', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales,
    });
    const formato2 = formato(2);
    const formato4 = formato(4);

    window.ventas = {
        /** Importe con 2 decimales, como en el comprobante y el reporte (P-11). */
        fmt2: (n) => formato2.format(n ?? 0),

        /** Importe con los 4 decimales con que se graba (RN-05). */
        fmt4: (n) => formato4.format(n ?? 0),

        /** Escapa un texto para insertarlo como HTML. */
        esc: (texto) => String(texto ?? '').replace(/[&<>"']/g, (c) =>
            ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]),

        /** Lee JSON por GET y devuelve la respuesta. */
        getJson: async (url, datos) => await $.ajax({ url, type: 'GET', dataType: 'json', data: datos }),

        /** Envía un objeto como JSON por POST y devuelve la respuesta. */
        postJson: async (url, datos) => await $.ajax({
            url,
            type: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(datos),
        }),

        /** Devuelve los mensajes de un error ProblemDetails (RFC 7807) para mostrarlos al operador. */
        mensajesError: (jqXHR) => {
            const problema = jqXHR.responseJSON;
            if (!problema) {
                return ['No se pudo comunicar con el servidor.'];
            }
            if (problema.errors) {
                return Object.values(problema.errors).flat();
            }
            return [problema.detail ?? problema.title];
        },
    };
})();
