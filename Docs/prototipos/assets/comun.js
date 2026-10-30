// Prototipos GalaxyVentas — datos ficticios y utilidades compartidas.
// Simula lo que devolvería el back-end; no hay servidor.

const IGV = 0.18; // RN-03
const HORAS_SESION = 8; // P-18

const DATOS = {
  // SGP_USUARIO (RF-20). Todos usan la contraseña de prueba «Ventas2026».
  usuarios: [
    { login: 'jperez', clave: 'Ventas2026', nombre: 'Juan Pérez', puntoVenta: '0001', activo: true, vigenteDesde: '2026-01-01', vigenteHasta: '2026-12-31', expiraClave: '2027-03-31' },
    { login: 'mlopez', clave: 'Ventas2026', nombre: 'María López', puntoVenta: '0001', activo: true, vigenteDesde: '2026-01-01', vigenteHasta: '2026-12-31', expiraClave: null },
    { login: 'rtorres', clave: 'Ventas2026', nombre: 'Raúl Torres', puntoVenta: '0001', activo: false, vigenteDesde: '2026-01-01', vigenteHasta: '2026-12-31', expiraClave: null },
    { login: 'acastro', clave: 'Ventas2026', nombre: 'Ana Castro', puntoVenta: '0001', activo: true, vigenteDesde: '2026-01-01', vigenteHasta: '2026-06-30', expiraClave: null },
    { login: 'lgomez', clave: 'Ventas2026', nombre: 'Luis Gómez', puntoVenta: '0001', activo: true, vigenteDesde: '2026-01-01', vigenteHasta: '2026-12-31', expiraClave: '2026-09-30' },
    { login: 'pvargas', clave: 'Ventas2026', nombre: 'Pedro Vargas', puntoVenta: null, activo: true, vigenteDesde: '2026-01-01', vigenteHasta: '2026-12-31', expiraClave: null }
  ],
  puntoVenta: { codigo: '0001', descripcion: 'Tienda Central', zonaVenta: 'LIM01' },
  canal: { codigo: '001', descripcion: 'Contado Soles', moneda: 'PEN' },
  moneda: { codigo: 'PEN', descripcion: 'Soles' }, // RN-16: todas las ventas y pagos son en soles
  almacen: { codigo: 'ALM-CENTRAL', descripcion: 'Almacén Central' }, // P-13

  // SGP_ARTICULO + SGP_TARIFA vigente + stock calculado del kardex (RF-01)
  articulos: [
    { codigo: 'ART-0001', descripcion: 'Cuaderno A4 cuadriculado 100 hojas', unidad: 'UND', precio: 8.5, stock: 120 },
    { codigo: 'ART-0002', descripcion: 'Lapicero tinta azul (caja x 12)', unidad: 'CJA', precio: 14.9, stock: 45 },
    { codigo: 'ART-0003', descripcion: 'Papel bond A4 75 g (paquete x 500)', unidad: 'PQT', precio: 19.5, stock: 3 },
    { codigo: 'ART-0004', descripcion: 'Archivador palanca lomo ancho', unidad: 'UND', precio: 11.2, stock: 60 },
    { codigo: 'ART-0005', descripcion: 'Calculadora científica', unidad: 'UND', precio: 65.0, stock: 0 },
    { codigo: 'ART-0006', descripcion: 'Resaltador colores surtidos (x 4)', unidad: 'PQT', precio: 9.8, stock: 80 },
    { codigo: 'ART-0007', descripcion: 'Mochila escolar 18 L', unidad: 'UND', precio: 89.9, stock: 12 }
  ],

  // SGP_CLIENTE (RF-02)
  clientes: [
    { codigo: 1001, tipoDoc: 'DNI', numDoc: '45678912', tipoPersona: 'Natural', nombre: 'Carlos Ramírez Soto', direccion: 'Av. Los Olivos 123, Lima' },
    { codigo: 1002, tipoDoc: 'RUC', numDoc: '20512345678', tipoPersona: 'Jurídica', nombre: 'Distribuidora Ejemplo S.A.C.', direccion: 'Jr. Comercio 450, Lima' },
    { codigo: 1003, tipoDoc: 'DNI', numDoc: '70123456', tipoPersona: 'Natural', nombre: 'Lucía Fernández Ríos', direccion: 'Calle Las Flores 89, Miraflores' }
  ],

  // SGP_TABLA / SGP_DETALLE_TABLA, solo registros activos (RF-03, RN-02)
  catalogos: {
    // P-14: la letra de la serie es el primer carácter del valor; códigos por confirmar
    tiposComprobante: [
      { valor: 'B', descripcion: 'Boleta' },
      { valor: 'F', descripcion: 'Factura' }
    ],
    formasPago: [{ valor: 'C00', descripcion: 'Contado' }],
    mediosPago: [
      { valor: 'EF', descripcion: 'Efectivo', requiereBanco: false },
      { valor: 'TJ', descripcion: 'Tarjeta', requiereBanco: true, requiereTarjeta: true },
      { valor: 'DP', descripcion: 'Depósito', requiereBanco: true },
      { valor: 'CH', descripcion: 'Cheque', requiereBanco: true }
    ],
    // Catálogo 114 (pga_mediopago_tipo_tarjeta_listar); RN-22
    tiposTarjeta: [
      { valor: '1', descripcion: 'Crédito' },
      { valor: '2', descripcion: 'Débito' }
    ],
    // P-20: códigos por confirmar
    tiposDocumento: [
      { valor: '1', descripcion: 'DNI', longitud: 8 },
      { valor: '6', descripcion: 'RUC', longitud: 11 }
    ]
  },

  // SGP_BANCO
  bancos: [
    { codigo: 'BCO001', descripcion: 'Banco Ejemplo Uno' },
    { codigo: 'BCO002', descripcion: 'Banco Ejemplo Dos' },
    { codigo: 'BCO003', descripcion: 'Banco Ejemplo Tres' }
  ],

  // SGP_IMPRESORA por vchNumSerie; vchCorrelativo guarda el PRÓXIMO número a emitir (RF-10, RN-13)
  impresoras: {
    B0001: { codigo: 11, correlativo: 1522 },
    F0001: { codigo: 12, correlativo: 846 }
  },
  tipoOrden: { codigo: 'V01', descripcion: 'Venta', tipoLiquidacion: '1' }, // SGP_TIPO_ORDEN (RF-04)

  ultimaTransaccion: 50232,
  ultimoComprobante: 70412
};

const r4 = n => Math.round((n + Number.EPSILON) * 10000) / 10000; // RN-05
const fmt = n => 'S/ ' + n.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const fmt2 = n => n.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); // P-11: el comprobante se muestra con 2 decimales
const fmt4 = n => n.toLocaleString('es-PE', { minimumFractionDigits: 4, maximumFractionDigits: 4 });
const sumar = (filas, campo) => r4(filas.reduce((s, f) => s + f[campo], 0));
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
// RN-13: serie = primer carácter del tipo de comprobante + punto de venta; número de 7 dígitos
const serieDe = (tipo, puntoVenta) => tipo.charAt(0) + puntoVenta;
const numeroComprobante = (serie, n) => `${serie}-${String(n).padStart(7, '0')}`;
const fechaHora = d => d.toLocaleString('es-PE', { dateStyle: 'short', timeStyle: 'short' });
const dosDigitos = n => String(n).padStart(2, '0');
const diaLocal = d => `${d.getFullYear()}-${dosDigitos(d.getMonth() + 1)}-${dosDigitos(d.getDate())}`; // AAAA-MM-DD

// RF-05 / RN-04, convención del procedimiento de referencia: el subtotal de la línea INCLUYE el IGV.
// bruto = valor de venta (cantidad × tarifa, P-15); subTotal = bruto + IGV
function calcularLinea(cantidad, precio) {
  const bruto = r4(cantidad * precio);
  const igv = r4(bruto * IGV);
  return { bruto, descuento: 0, igv, subTotal: r4(bruto + igv) }; // descuento: P-03
}

// Cabecera de venta y comprobante: bruto = Σ subtotales de línea; subTotal = Σ (subtotal − IGV) (base imponible)
function totalesDe(lineas) {
  const subTotal = r4(lineas.reduce((s, l) => s + l.subTotal - l.igv, 0));
  const igv = sumar(lineas, 'igv');
  return { bruto: sumar(lineas, 'subTotal'), descuento: 0, recargo: 0, subTotal, igv, total: r4(subTotal + igv) };
}

const articulo = codigo => DATOS.articulos.find(a => a.codigo === codigo);
const descCatalogo = (lista, valor) => (DATOS.catalogos[lista].find(x => x.valor === valor) || {}).descripcion || valor;
const descBanco = codigo => (DATOS.bancos.find(b => b.codigo === codigo) || {}).descripcion || '';

// Persistencia de la sesión del prototipo (sesión de usuario, ventas, clientes, stock consumido, correlativos)
const sesion = {
  leer(clave, defecto) {
    try {
      const v = sessionStorage.getItem(clave);
      return v === null ? defecto : JSON.parse(v);
    } catch {
      return defecto;
    }
  },
  guardar(clave, valor) {
    try { sessionStorage.setItem(clave, JSON.stringify(valor)); } catch { /* sin almacenamiento */ }
  },
  quitar(clave) {
    try { sessionStorage.removeItem(clave); } catch { /* sin almacenamiento */ }
  }
};

function almacenamientoDisponible() {
  try {
    sessionStorage.setItem('_prueba', '1');
    sessionStorage.removeItem('_prueba');
    return true;
  } catch {
    return false;
  }
}

// ---- Sesión del operador (RF-18, RF-20) ----
const sesionActual = () => {
  const s = sesion.leer('sesionUsuario', null);
  return s && Date.parse(s.expira) > Date.now() ? s : null;
};

function cerrarSesion() {
  sesion.quitar('sesionUsuario');
  location.href = 'login.html';
}

// Las páginas que no son públicas exigen sesión válida; si falta o expiró, se vuelve al login (401).
// Sin almacenamiento (ventana privada estricta) no se exige, para que el prototipo siga navegable.
(() => {
  const publicas = ['login', 'indice'];
  if (publicas.includes(document.body.dataset.pagina) || !almacenamientoDisponible()) return;
  const s = sesion.leer('sesionUsuario', null);
  if (!s) location.replace('login.html');
  else if (Date.parse(s.expira) <= Date.now()) {
    sesion.quitar('sesionUsuario');
    location.replace('login.html?motivo=expirada');
  }
})();

// Usuario de la sesión; si no hay (página pública o sin almacenamiento) se usa el de prueba
const USUARIO = sesionActual() || { login: 'jperez', nombre: 'Juan Pérez', puntoVenta: '0001' };

// RF-02 / RF-22: clientes de ejemplo + los registrados en esta sesión
const todosLosClientes = () => [...DATOS.clientes, ...sesion.leer('clientesRegistrados', [])];

// RN-09: stock = stock base − salidas registradas en esta sesión (kardex simulado)
function stockDisponible(codigo) {
  const consumido = sesion.leer('stockConsumido', {});
  return articulo(codigo).stock - (consumido[codigo] || 0);
}

// Una venta con su comprobante, kardex y un único pago por el total (RF-04 … RF-14, RN-16)
function construirVenta({ numCodTransaccion, numCodComprobante, fecha, cliente, tipoComprobante, items, pago, numero, usuario = USUARIO.login, puntoVenta = DATOS.puntoVenta }) {
  const lineas = items.map((it, i) => {
    const a = articulo(it.codigo);
    return {
      linea: i + 1, codigo: a.codigo, descripcion: a.descripcion, unidad: a.unidad,
      cantidad: it.cantidad, precio: a.precio, ...calcularLinea(it.cantidad, a.precio)
    };
  });
  const totales = totalesDe(lineas);
  const serie = serieDe(tipoComprobante, puntoVenta.codigo);
  const numeroCompleto = numeroComprobante(serie, numero);
  return {
    numCodTransaccion,
    fecha,
    usuario,
    puntoVenta,
    canal: DATOS.canal,
    almacen: DATOS.almacen,
    tipoOrden: DATOS.tipoOrden,
    cliente,
    lineas,
    totales,
    comprobante: {
      numCodComprobante, // también se graba en la cabecera de la venta (RF-09)
      tipo: tipoComprobante,
      tipoDescripcion: descCatalogo('tiposComprobante', tipoComprobante),
      serie,
      numero,
      numeroCompleto,
      fecha,
      formaPago: 'C00',
      estado: '2', // vchEstado: cancelado (P-10)
      montoPagado: totales.total, // P-22: el procedimiento de referencia graba 0
      // SGP_MOVIMIENTO_TRANSACCION_BS: ítem de comprobante ↔ línea de transacción (P-04: 1 a 1)
      movimientos: lineas.map(l => ({ item: l.linea, linea: l.linea }))
    },
    // PGA_TRANSACCION_KARDEX (RF-08)
    kardex: lineas.map(l => ({
      codigo: l.codigo, tipo: 'VENTA', cantidad: -l.cantidad,
      descripcion: `Salida por venta ${numeroCompleto}`
    })),
    // SGP_MEDIO_PAGO_BS: un solo medio por el total (P-21)
    pago: { ...pago, monto: totales.total, estado: '2' }
  };
}

// Ventas de ejemplo ya registradas (para consultas y reporte)
const VENTAS_EJEMPLO = [
  construirVenta({
    numCodTransaccion: 50229, numCodComprobante: 70409, fecha: '2026-10-26T10:42:00', usuario: 'jperez',
    cliente: DATOS.clientes[0], tipoComprobante: 'B', numero: 1520,
    items: [{ codigo: 'ART-0001', cantidad: 3 }, { codigo: 'ART-0006', cantidad: 1 }],
    pago: { medio: 'EF' }
  }),
  construirVenta({
    numCodTransaccion: 50230, numCodComprobante: 70410, fecha: '2026-10-26T16:05:00', usuario: 'jperez',
    cliente: DATOS.clientes[1], tipoComprobante: 'F', numero: 845,
    items: [{ codigo: 'ART-0002', cantidad: 10 }, { codigo: 'ART-0004', cantidad: 5 }, { codigo: 'ART-0007', cantidad: 2 }],
    pago: { medio: 'TJ', banco: 'BCO002', operacion: '004512', tipoTarjeta: '1' }
  }),
  construirVenta({
    numCodTransaccion: 50231, numCodComprobante: 70411, fecha: '2026-10-27T11:20:00', usuario: 'mlopez',
    cliente: DATOS.clientes[2], tipoComprobante: 'B', numero: 1521,
    items: [{ codigo: 'ART-0006', cantidad: 2 }, { codigo: 'ART-0004', cantidad: 1 }],
    pago: { medio: 'DP', banco: 'BCO001', operacion: '778812' }
  }),
  // Venta de otro punto de venta: el operador de la Tienda Central no debe verla (RN-19)
  construirVenta({
    numCodTransaccion: 50232, numCodComprobante: 70412, fecha: '2026-10-27T15:00:00', usuario: 'cquispe',
    puntoVenta: { codigo: '0002', descripcion: 'Tienda Norte', zonaVenta: 'LIM02' },
    cliente: DATOS.clientes[0], tipoComprobante: 'B', numero: 300,
    items: [{ codigo: 'ART-0007', cantidad: 1 }],
    pago: { medio: 'EF' }
  })
];

// Se descartan las ventas guardadas por una versión anterior del prototipo (tenían varios pagos)
const todasLasVentas = () => [...VENTAS_EJEMPLO, ...sesion.leer('ventasRegistradas', []).filter(v => v.pago)];
const esDeMiPuntoVenta = v => v.puntoVenta.codigo === USUARIO.puntoVenta; // RN-19
const ventasDelPuntoVenta = () => todasLasVentas().filter(esDeMiPuntoVenta);

// RF-19: respuesta de error en formato ProblemDetails (RFC 7807). errores: [{ campo?, mensaje }]
const TIPOS_PROBLEMA = {
  400: ['https://tools.ietf.org/html/rfc9110#section-15.5.1', 'Datos de entrada inválidos'],
  401: ['https://tools.ietf.org/html/rfc9110#section-15.5.2', 'No autorizado'],
  403: ['https://tools.ietf.org/html/rfc9110#section-15.5.4', 'Acceso denegado'],
  404: ['https://tools.ietf.org/html/rfc9110#section-15.5.5', 'No encontrado'],
  409: ['https://tools.ietf.org/html/rfc9110#section-15.5.10', 'Conflicto'],
  422: ['https://tools.ietf.org/html/rfc4918#section-11.2', 'Regla de negocio incumplida']
};

function htmlProblema(status, errores) {
  const [type, title] = TIPOS_PROBLEMA[status];
  const conCampos = errores.some(e => e.campo);
  const problema = conCampos
    ? { type, title, status, errors: errores.reduce((acc, e) => ({ ...acc, [e.campo]: [...(acc[e.campo] || []), e.mensaje] }), {}) }
    : { type, title, status, detail: errores.map(e => e.mensaje).join(' ') };
  return `
    <b>${status} · ${title}</b> <span class="rf">RF-19</span>
    <ul>${errores.map(e => `<li>${esc(e.mensaje)}</li>`).join('')}</ul>
    <details><summary>Respuesta del servidor (ProblemDetails)</summary><pre>${esc(JSON.stringify(problema, null, 2))}</pre></details>`;
}

// Mostrar / ocultar anotaciones de requerimientos (preferencia local del visitante)
function iniciarAnotaciones() {
  const btn = document.getElementById('toggle-anot');
  if (!btn) return;
  let ocultas = false;
  try { ocultas = localStorage.getItem('anot-ocultas') === '1'; } catch { /* sin almacenamiento */ }
  const aplicar = () => {
    document.body.classList.toggle('sin-anotaciones', ocultas);
    btn.textContent = ocultas ? 'Mostrar anotaciones' : 'Ocultar anotaciones';
    btn.setAttribute('aria-pressed', String(!ocultas));
  };
  btn.addEventListener('click', () => {
    ocultas = !ocultas;
    try { localStorage.setItem('anot-ocultas', ocultas ? '1' : '0'); } catch { /* sin almacenamiento */ }
    aplicar();
  });
  aplicar();
}

function iniciarSesionInfo() {
  const el = document.getElementById('sesion-info');
  if (!el) return;
  const s = sesionActual();
  if (!s) {
    el.innerHTML = '<a href="login.html">Iniciar sesión</a>';
    return;
  }
  el.textContent = `${s.nombre} (${s.login}) · PV ${DATOS.puntoVenta.codigo} ${DATOS.puntoVenta.descripcion}`;
  const btn = document.createElement('button');
  btn.type = 'button';
  btn.className = 'btn btn-sec btn-sm';
  btn.textContent = 'Cerrar sesión';
  btn.addEventListener('click', cerrarSesion);
  el.after(btn);
}

document.addEventListener('DOMContentLoaded', () => {
  iniciarAnotaciones();
  iniciarSesionInfo();
});
