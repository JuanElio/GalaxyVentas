# Requerimientos funcionales — Sistema de Ventas de Punto de Venta

| | |
|---|---|
| **Proyecto** | GalaxyVentas — Sistema de ventas de artículos para punto de venta |
| **Versión** | 0.5 (borrador) |
| **Fecha** | 29/10/2026 |
| **Base de datos** | `BDVenta` (SQL Server) — esquema en [database/BDVentas.sql](database/BDVentas.sql); procedimientos almacenados de referencia en [database/StoreProcedure.sql](database/StoreProcedure.sql) |
| **Estado** | Pendiente de validación. Los puntos marcados **[PENDIENTE]** usan un supuesto provisional (sección 7). |

### Historial de versiones

| Versión | Fecha | Cambios |
|---|---|---|
| 0.5 | 29/10/2026 | Se resuelven P-02 (contraseña en texto plano), P-09 (códigos de medio de pago; débito y crédito son medios distintos, se elimina RN-22), P-10 (`intEstado = 1`), P-13 (primer almacén activo del punto de venta), P-14 (serie `F`/`B`), P-15 (la tarifa incluye IGV: cambia RF-06 y RN-04) y el canal (fijo `01`). El stock se calcula sin canal (RN-09, RF-09). Serie: los puntos de venta se guardan como `' 001'` (RF-11). Se resuelven P-06 (escala de tarifa) y P-20 (códigos de documento y persona); moneda `SOL`. |
| 0.4 | 29/10/2026 | Los procedimientos almacenados de `StoreProcedure.sql` pasan a ser la referencia de cómo se graban los datos (sección 1). Se ajustan a ellos: cálculo de importes (RF-05, RN-04: el subtotal de la línea incluye IGV) |
| 0.3 | 29/10/2026 | Se exige el token de acceso en cada llamada (`Authorization: Bearer <token>`): RF-20 devuelve el token; RF-18 agrega criterios de aceptación (401 sin token, token inválido o expirado, o usuario no vigente. Tarjeta de crédito y de débito se registran como un solo medio "tarjeta" con tipo obligatorio. |
| 0.2 | 28/10/2026 | Se redefine el documento como sistema de ventas de punto de venta . Se agregan RF-20 (iniciar sesión), RF-21 (reporte de ventas registradas) y RF-22 (registrar cliente). |
| 0.1 | 27/10/2026 | Versión inicial: RF-01 a RF-19, reglas de negocio RN-01 a RN-17, requerimientos no funcionales RNF-01 a RNF-05, supuestos P-01 a P-16 y fuera de alcance. |

---

## 1. Objetivo y alcance

Implementar un sistema que permita al operador de un punto de venta **registrar la venta de artículos en un punto de venta, generar su comprobante (boleta o factura) y registrar el medio de pago** con el que se cancela, actualizando el stock. Todo se realiza sobre la base de datos existente `BDVenta`, **sin modificar su estructura**.

**Procedimientos almacenados de referencia.** Los procedimientos de [StoreProcedure.sql](database/StoreProcedure.sql) son la referencia de cómo se graban hoy los datos: la venta y sus importes (`pga_cabecera_transaccion_hemeroteca_agregar`), el comprobante, la serie y el correlativo, y el kardex (`pga_hemeroteca_emitir_comprobante`) y el medio de pago (`pga_hemeroteca_comprobante_cancelar`). El sistema debe grabar los datos con las mismas convenciones, para ser compatible con la información existente, aunque no esté obligado a invocar esos procedimientos. Donde un procedimiento usa un valor heredado discutible, se indica en la sección 7 (P-22).

**Incluido**

- Inicio de sesión del operador con usuario y contraseña.
- Consulta de maestros necesarios para vender: artículos (con precio y stock), clientes y catálogos.
- Registro de clientes nuevos.
- Registro de la venta (cabecera y detalle).
- Cálculo de importes e IGV.
- Generación del comprobante con serie y número correlativo.
- Registro del medio de pago y su asignación al comprobante.
- Registro del movimiento de stock (kardex).
- Consulta de ventas y comprobantes registrados.
- Reporte de ventas registradas.

**Excluido** (ver sección 8)

Anulación de comprobantes, notas de crédito/débito, modificación y baja de clientes, mantenimiento de los demás maestros, pagos digitales, integración con Baan/SUNAT, impresión de comprobantes y gestión de usuarios y permisos.

## 2. Actores

| Actor | Descripción |
|---|---|
| **Operador (vendedor / cajero)** | Usuario registrado en `SGP_USUARIO`, asignado a un punto de venta. Busca o registra clientes, registra ventas y cobros, y emite comprobantes desde la aplicación web del punto de venta. |

## 3. Glosario

| Término | Significado |
|---|---|
| **Venta / transacción** | Operación comercial con un cliente. Tabla `SGP_CABECERA_TRANSACCION_BS` + `SGP_DETALLE_TRANSACCION_BS`. |
| **Comprobante** | Documento de venta (boleta o factura) con serie y número. Tabla `SGP_COMPROBANTE_BS` + `SGP_DETALLE_COMPROBANTE_BS`. |
| **Medio de pago** | Forma en que el cliente paga (efectivo, tarjeta, depósito, etc.). Tabla `SGP_MEDIO_PAGO_BS`. Tarjeta de débito y tarjeta de crédito son medios de pago distintos (P-09). |
| **Punto de venta** | Local o caja donde se vende. Tabla `SGP_PUNTO_VENTA`. Cada operador tiene uno asignado en `SGP_USUARIO.vchCodPuntoVenta`; es el único en el que puede operar (RN-19). |
| **Canal / tarifa canal** | Lista de precios aplicable (`SGP_TARIFA_CANAL`). El precio de cada artículo depende del canal (`SGP_TARIFA`). |
| **Kardex** | Registro de movimientos de stock (`PGA_TRANSACCION_KARDEX`). |
| **Catálogo** | Lista de códigos genérica (`SGP_TABLA` / `SGP_DETALLE_TABLA`): formas de pago, tipos de comprobante, medios de pago, monedas, tipos de documento, etc. |
| **Registro activo** | Registro con `chrSituacionRegistro = 'A'`. |
| **Tipo de orden** | Clasificación de la venta (`SGP_TIPO_ORDEN`). Determina el tipo de liquidación (`intCodTipoLiquidacion`). |
| **Procedimiento de referencia** | Procedimiento almacenado de `StoreProcedure.sql` que define cómo se graban los datos (sección 1). |

---

## 4. Requerimientos funcionales

Cada requerimiento indica las reglas de negocio aplicables (sección 5) y sus criterios de aceptación. Los requerimientos agregados después de la versión 0.1 se numeran a continuación del último (RF-21 en adelante) y se ubican en la sección que les corresponde, para no renumerar los existentes.

En todos los requerimientos, **el punto de venta no se ingresa**: es el asignado al usuario en `SGP_USUARIO.vchCodPuntoVenta`, que se obtiene al iniciar sesión (RF-21) y se guarda en la sesión del usuario (RF-19, RN-19).

### 4.1 Consulta de maestros

#### RF-01 — Consultar artículos disponibles para la venta

El sistema debe listar los artículos que se pueden vender en un punto de venta y canal, con su precio vigente y stock disponible.

- **Entrada:** texto de búsqueda opcional (código o descripción). El punto de venta es el del usuario (`SGP_USUARIO.vchCodPuntoVenta`) y el canal es siempre `01` (no se ingresa).
- **Salida por artículo:** código, descripción, unidad de medida, precio unitario vigente, stock disponible.
- **Reglas:** RN-01, RN-06, RN-09.
- **Criterios de aceptación:**
  - Solo se listan artículos activos que tengan una tarifa vigente para el canal a la fecha actual.
  - El stock mostrado es el calculado para ese punto de venta (RN-09).
  - Si el punto de venta o el canal no existe o está inactivo, se responde *no encontrado*.

#### RF-02 — Buscar cliente

El sistema debe permitir buscar un cliente por su número de documento o por su nombre o razón social.

- **Entrada:** un texto: si son solo dígitos, se busca por número de documento (DNI/RUC); si no, por nombre o razón social (mínimo 3 caracteres). En la búsqueda por nombre cada palabra (hasta 5) debe aparecer en `vchNombres`, `vchApellidos` o `vchRazonSocial`, en cualquier orden; muchos clientes heredados tienen el nombre completo en `vchNombres`.
- **Salida:** código de cliente, tipo y número de documento, nombre o razón social, dirección de facturación.
- **Fuente:** `SGP_CLIENTE` (`vchNumDocumento`, `vchTipDocumento`, `vchNombres`, `vchApellidos`, `vchRazonSocial`, `vchDirecFacturacion`).
- **Reglas:** RN-01.
- **Criterios de aceptación:** solo devuelve clientes activos; si no existe, responde *no encontrado* y el operador puede registrarlo (RF-03). Como la BD tiene documentos repetidos entre clientes activos (datos heredados), devuelve todas las coincidencias (máximo 20, la más reciente primero) para que el operador elija.

#### RF-03 — Registrar cliente

Si el cliente no existe, el operador debe poder registrarlo para continuar con la venta.

- **Entrada:**
  - Tipo de documento (DNI/RUC, del catálogo) y número de documento (obligatorios).
  - Persona natural: nombres y apellidos (obligatorios).
  - Persona jurídica (RUC): razón social (obligatoria).
  - Dirección de facturación (obligatoria para RUC), teléfono y correo (opcionales).
- **Salida:** los mismos datos de RF-02, incluido el código de cliente generado, para usarlo de inmediato en la venta.
- **Destino:** `SGP_CLIENTE` (`vchTipDocumento`, `vchNumDocumento`, `vchTipPersona`, `vchNombres`, `vchApellidos`, `vchRazonSocial`, `vchDirecFacturacion`, `vchTelefono`, `vchEmail`, `chrSituacionRegistro = 'A'`). `numCodCliente` lo genera la BD (IDENTITY).
- **Reglas:** RN-02, RN-20.
- **Criterios de aceptación:**
  - Si ya existe un cliente activo con el mismo tipo y número de documento, se rechaza como *conflicto* (409) indicando que ya está registrado.
  - El número de documento cumple el formato de su tipo: DNI de 8 dígitos, RUC de 11 dígitos **[PENDIENTE P-20]**.
  - El tipo de persona se deduce del documento (DNI → natural, RUC → jurídica o natural con negocio) **[PENDIENTE P-20]**.
  - Se registran los datos de auditoría (RF-18).

#### RF-04 — Consultar catálogos

El sistema debe devolver los valores activos de un catálogo (formas de pago, tipos de comprobante, medios de pago, monedas, tipos de documento, etc.).

- **Entrada:** código del catálogo (`SGP_TABLA.numCodigo`).
- **Salida:** lista de `numItem`, `vchValor`, `vchDescripcion`.
- **Reglas:** RN-02.
- **Criterios de aceptación:** no se devuelven registros inactivos, ni del catálogo ni de sus ítems.

### 4.2 Registro de la venta

#### RF-05 — Registrar venta

El sistema debe registrar una venta con sus líneas de detalle, generar el comprobante y registrar el pago en **una sola operación** (RF-05 a RF-15 se ejecutan juntos).

- **Entrada:**
  - Usuario que registra: se toma de la sesión iniciada (RF-21), no se ingresa.
  - Punto de venta: no se ingresa; es el asignado al usuario en `SGP_USUARIO.vchCodPuntoVenta` (RN-19).
  - Canal (tarifa canal): no se ingresa; es siempre `01`.
  - Almacén (`vchCodAlmacen`): no se ingresa; es el primer almacén activo (menor `vchCodAlmacen`) del punto de venta en `SGP_PUNTO_VENTA_ALMACEN` (P-13). Si no tiene ninguno, la venta se rechaza.
  - Tipo de orden: no se ingresa; es `V01` (valor por defecto del procedimiento de referencia), activo en `SGP_TIPO_ORDEN`.
  - Moneda: no se ingresa; siempre soles (RN-21).
  - Cliente (código).
  - Tipo de comprobante (boleta o factura) y forma de pago.
  - Líneas: código de artículo y cantidad (1 o más).
  - Pago: un medio de pago y los datos que exige (banco y número de operación). El monto no se ingresa: es el total de la venta (RN-16).
- **Salida:** número de transacción, comprobante generado (serie, número, fecha, totales) y pago registrado.
- **Reglas:** RN-01 a RN-16, RN-19, RN-21.
- **Criterios de aceptación:**
  - Se crea un registro en `SGP_CABECERA_TRANSACCION_BS` y una línea en `SGP_DETALLE_TRANSACCION_BS` por cada artículo, numeradas desde 1 (`numLinTransaccion`).
  - La cabecera se crea con `chrSituacionRegistro = 'I'` e `intEstado = 1` (P-10), y pasa a `'A'` al emitirse el comprobante, como en el procedimiento de referencia. Como todo ocurre en una sola transacción (RN-15), una venta nunca queda en `'I'`.
  - El tipo de liquidación (`vchTipLiquidacion`) se toma de `SGP_TIPO_ORDEN.intCodTipoLiquidacion` del tipo de orden.
  - El código de canal (`vchCodTarifaCanal`) se graba con la normalización del procedimiento de referencia: sin espacios, y si tiene 2 caracteres se antepone un espacio (`01` → `' 01'`).
  - Un artículo no puede repetirse en dos líneas de la misma venta.
  - La fecha de venta (`dtmFechaVenta`) es la fecha y hora del servidor.
  - Si cualquier validación falla, no se graba nada (RN-15).

#### RF-06 — Calcular importes

El sistema debe calcular los importes de cada línea y de la cabecera; el operador **no** ingresa importes.

Se sigue la convención del procedimiento de referencia, en la que **el subtotal de la línea incluye el IGV**. A diferencia del procedimiento, **la tarifa ya incluye el IGV** (P-15), así que el IGV se desglosa en lugar de sumarse.

- **Por línea** (`SGP_DETALLE_TRANSACCION_BS`): `numSubTotal = Cantidad × Tarifa` (importe de la línea con IGV); `ValorVenta = numSubTotal / 1.18`, redondeado a 4 decimales; `numIGV = numSubTotal − ValorVenta`; `numMontoBruto = Cantidad × Tarifa`.
- **Cabecera de la venta y del comprobante:** `numMontoBruto = Σ numSubTotal` de las líneas; `numSubTotal = Σ (numSubTotal − numIGV)` (base imponible); `numMontoIGV = Σ numIGV`; `numMontoTotal = numSubTotal + numMontoIGV`; `numMontoDescuento = numMontoRecargo = 0`.
- **Detalle del comprobante:** `numPrecioUnitario = numTarifa`; `numSubTotal` y `numIGV` iguales a los de la línea; `numMontoBruto = numSubTotal + numMontoRecargo − numMontoDescuento`.
- **Reglas:** RN-03, RN-04, RN-05.
- **Criterios de aceptación:**
  - Para 2 unidades con tarifa 59.0000: línea con subtotal 118.0000 e IGV 18.0000; cabecera con bruto 118.0000, subtotal 100.0000, IGV 18.0000 y total 118.0000.
  - El total de la venta es siempre Σ (cantidad × tarifa): el desglose del IGV no altera lo que paga el cliente.
  - Los totales de cabecera y comprobante coinciden exactamente con la suma de sus líneas.

#### RF-07 — Determinar el precio

El precio unitario de cada línea se toma de la tarifa vigente del artículo para el canal de la venta.

- **Fuente:** `SGP_TARIFA` (`vchCodArticulo`, `vchCodTarifaCanal`, `numCantidad`, `numPrecioUnidad`, `dtmFecInicio`, `dtmFecCaducidad`).
- **Reglas:** RN-06, RN-07.
- **Criterios de aceptación:**
  - Si el artículo no tiene tarifa vigente para el canal, la venta se rechaza indicando el artículo.
  - El precio aplicado se guarda en `numTarifa` del detalle de la transacción y en `numPrecioUnitario` del detalle del comprobante.

#### RF-08 — Validar stock

Antes de grabar, el sistema debe verificar que hay stock suficiente de cada artículo en el punto de venta de la venta (RN-09).

- **Reglas:** RN-09, RN-10.
- **Criterios de aceptación:**
  - Si la cantidad pedida supera el stock disponible, la venta se rechaza indicando artículo, stock disponible y cantidad pedida.
  - Dos ventas simultáneas del último stock no pueden aprobarse ambas.

#### RF-09 — Registrar movimiento de stock

Por cada línea de la venta, el sistema debe registrar una salida en el kardex.

- **Destino:** `PGA_TRANSACCION_KARDEX`, con los valores del procedimiento de referencia:

  | Columna | Valor |
  |---|---|
  | `CodArticulo`, `CodPuntoVenta` | Los de la línea y la venta (punto de venta sin espacios, comparado con `LTRIM/RTRIM`) |
  | `CodTarifaCanal` | `'01'`. El procedimiento de referencia graba NULL por un error (la variable nunca se asigna); el sistema nuevo graba el canal. |
  | `TipoTransaccion` | `'VENTA'` |
  | `FechaTransaccion` | Fecha de la venta sin hora |
  | `Cantidad` | Cantidad vendida en negativo |
  | `CodTransaccion` | Número de transacción de la venta |
  | `Descripcion` | `'Salida por venta '` + serie + `'-'` + número del comprobante |
  | `Estado`, `Proceso` | Vacíos (`''`) |
  | `GuidRegistro` | Identificador único nuevo |
  | `SituacionRegistro` | `'A'` |

- **Reglas:** RN-09, RN-11.
- **Criterios de aceptación:** después de la venta, el stock consultado (RF-01) disminuye exactamente en la cantidad vendida.

### 4.3 Generación del comprobante

#### RF-10 — Generar comprobante

Al registrar la venta, el sistema debe generar su comprobante.

- **Cabecera** (`SGP_COMPROBANTE_BS`): cliente, punto de venta, zona de venta (la del punto de venta), forma de pago, tipo de comprobante, serie, número, fecha de emisión (fecha y hora del servidor), importes (RF-06), monto pagado, moneda (soles) y tipo de cambio (1) (RN-21). Además, como en el procedimiento de referencia: `vchEstado = '2'` (cancelado), `chrFlagFacturado = 'S'` y `chrSituacionRegistro = 'A'`.
- **Detalle** (`SGP_DETALLE_COMPROBANTE_BS`): un ítem por cada línea de la venta (`numItem` igual a `numLinTransaccion`) con artículo, descripción del artículo, unidad, cantidad, precio unitario e importes (RF-06). Las columnas de avisos y publicaciones se graban en 0 o NULL.
- **Enlace:** un registro en `SGP_MOVIMIENTO_TRANSACCION_BS` por cada par ítem de comprobante ↔ línea de transacción.
- **Cabecera de la venta:** se actualiza `numCodComprobante` con el código del comprobante generado y `chrSituacionRegistro = 'A'`.
- **Reglas:** RN-12, RN-13, RN-14.
- **Criterios de aceptación:**
  - Una venta genera exactamente un comprobante **[PENDIENTE P-04]**.
  - Desde la venta se puede llegar a su comprobante (`numCodComprobante` y `SGP_MOVIMIENTO_TRANSACCION_BS`) y viceversa.

#### RF-11 — Numerar el comprobante

El sistema debe asignar serie y número correlativo al comprobante según el punto de venta y el tipo de comprobante.

- **Fuente:** `SGP_IMPRESORA`, buscando por `vchNumSerie`. Como en el procedimiento de referencia, la serie se forma con el primer carácter del tipo de comprobante seguido del código del punto de venta sin espacios (p. ej. boleta en el punto de venta `' 001'` → serie `B001`). La letra es `F` para factura y `B` para boleta (P-14). Los códigos de punto de venta se guardan con un espacio inicial (`' 001'`), así que la serie siempre cabe en `vchNumSerie` (`varchar(4)`). La impresora se busca solo por `vchNumSerie`: su `vchTipComprobante` no es fiable (p. ej. la serie `B002` tiene tipo `F`).
- **Reglas:** RN-13.
- **Criterios de aceptación:**
  - `vchCorrelativo` guarda el **próximo** número a emitir: el comprobante toma ese valor (si está vacío, 1) y la impresora queda con el valor + 1.
  - El número del comprobante (`vchNumComprobante`) tiene 7 dígitos con ceros a la izquierda (p. ej. `0001521`).
  - Nunca se asigna el mismo número a dos comprobantes de la misma serie, aunque se registren a la vez.
  - Si la venta falla, el correlativo no avanza.
  - Si no existe una impresora activa con la serie del punto de venta y tipo de comprobante, la venta se rechaza.

#### RF-12 — Conservar los datos del cliente en el comprobante

El comprobante debe guardar el nombre, dirección y documento del cliente tal como están en el momento de la emisión (`vchNomComprobante`, `vchDirComprobante`, `vchTipoDocumento`, `vchNumDocumento`).

- **Criterios de aceptación:** si después se modifica el cliente, el comprobante ya emitido no cambia.

### 4.4 Registro del medio de pago

#### RF-13 — Registrar el medio de pago

El sistema debe registrar el medio de pago de la venta. Como en el procedimiento de referencia, cada venta tiene **un solo medio de pago por el total** **[PENDIENTE P-21]**.

- **Destino:** `SGP_MEDIO_PAGO_BS` (`numCodTransaccion`, `vchMedioPago`, `numMonto`, `numMontoOrigen`, `vchCodMoneda`, `numTipoCambio`, `vchCodBanco`, `vchNumDocumento`, `vchTipoTarjeta`, `numLinea`, `vchEstado`, `chrFlagPendienteAjuste`, `vchOrigen`).
- **Reglas:** RN-02, RN-16, RN-21.
- **Criterios de aceptación:**
  - El pago queda asociado a la transacción con `numMonto` y `numMontoOrigen` iguales al total de la venta.
  - Se graba como en el procedimiento de referencia: `vchEstado = '2'`, `numLinea = 0`, `chrFlagPendienteAjuste = 'N'`, `vchOrigen = 'PGA'`; un banco vacío se graba como NULL.
  - El pago se registra en soles con tipo de cambio 1 (RN-21, P-22).
  - El medio de pago debe existir y estar activo en su catálogo.
  - Códigos de `vchMedioPago` (P-09): `1` efectivo, `2` cheque, `3` tarjeta de débito, `4` tarjeta de crédito, `5` boleta de depósito.
  - Si el medio es cheque, tarjeta (débito o crédito) o depósito, el banco y el número de operación son obligatorios.
  - Débito y crédito son medios de pago distintos (`3` y `4`); `vchTipoTarjeta` se graba en NULL.

#### RF-14 — Asignar el pago al comprobante

El pago debe quedar asignado al comprobante por el total.

- **Destino:** `SGP_MEDIO_PAGO_COMPROBANTE_BS` (`numCodComprobante`, `numCodMediPago`, `numMontoPago`).
- **Criterios de aceptación:**
  - `numMontoPago` es igual al total del comprobante.
  - El comprobante queda marcado como pagado con `vchFlagMedioPago = 'S'`, como en el procedimiento de referencia.
  - `SGP_COMPROBANTE_BS.numMontoPagado` es igual al monto asignado **[PENDIENTE P-22]**.

#### RF-15 — Validar el pago

- **Reglas:** RN-16.
- **Criterios de aceptación:**
  - La venta debe tener exactamente un medio de pago; si no lo tiene, se rechaza (400).
  - El monto pagado es siempre el total de la venta: el operador no lo ingresa, así que no puede haber diferencia entre ambos.

### 4.5 Consultas

#### RF-16 — Consultar una venta

Dado el número de transacción, el sistema debe devolver la cabecera, sus líneas, el comprobante asociado y el pago.

- **Reglas:** RN-19.
- **Criterios de aceptación:**
  - Si no existe o no está activa, responde *no encontrado* (404).
  - Si pertenece a un punto de venta distinto al del usuario, responde *acceso denegado* (403).

#### RF-17 — Consultar un comprobante

Dado el código de comprobante, o su serie y número, el sistema debe devolver la cabecera, el detalle y el pago asignado.

- **Reglas:** RN-19.
- **Criterios de aceptación:**
  - Si no existe, responde *no encontrado* (404).
  - Si pertenece a un punto de venta distinto al del usuario, responde *acceso denegado* (403).

### 4.6 Requerimientos transversales

#### RF-18 — Auditoría

Todo registro creado debe guardar el login del usuario en `vchCreador` (o `Creador` en tablas `PGA_`) y la fecha y hora en `dtmFechaCreacion` (o `FechaCreacion`). Toda modificación (p. ej. el correlativo de la impresora) debe guardar `vchModificador` y `dtmFechaModificacion`.

#### RF-19 — Validación del usuario

Toda página y toda llamada AJAX (jQuery), salvo el inicio de sesión, requieren una sesión válida (RF-21). La sesión se mantiene con la cookie de autenticación de ASP.NET Core, que el navegador envía automáticamente en cada petición. Además, toda petición que modifica datos (formularios y llamadas AJAX por POST) debe enviar el token antifalsificación (*antiforgery*) de la página. El usuario de la sesión debe seguir existiendo en `SGP_USUARIO` (`vchLogin`), estar activo y estar dentro de su periodo de vigencia (`dtmFechaInicioAct` ≤ hoy ≤ `dtmFechaFinAct`).

- **Reglas:** RN-18, RN-19.
- **Criterios de aceptación:**
  - Si no hay sesión, la cookie no es válida o la sesión expiró: una página redirige al inicio de sesión, y una llamada AJAX responde *no autorizado* (401) sin ejecutar la operación.
  - Si el usuario de la sesión ya no está activo o vigente, se trata como sesión no válida, aunque la cookie no haya expirado.
  - Una petición que modifica datos sin token antifalsificación, o con uno inválido, se rechaza (400) sin ejecutar la operación.
  - El usuario y el punto de venta de la operación se toman de la sesión, nunca de los datos enviados en la petición. El punto de venta de la sesión es el de `SGP_USUARIO.vchCodPuntoVenta` al iniciar sesión.
  - Si el punto de venta asignado al usuario en `SGP_USUARIO` cambia, la sesión deja de ser válida y el usuario debe volver a iniciar sesión.
  - Si la petición se refiere a un punto de venta distinto al de la sesión, se rechaza como *acceso denegado* (403).
  - El usuario de la sesión es el que se registra en la auditoría (RF-18).

#### RF-20 — Respuestas de error

Los errores de las llamadas AJAX se devuelven en formato ProblemDetails (RFC 7807), con mensajes en español, y la vista los muestra junto a los campos o en un aviso. En las páginas, los errores de validación se muestran en el mismo formulario. Códigos:

| Situación | Código HTTP |
|---|---|
| Datos de entrada inválidos (campo obligatorio, cantidad ≤ 0, código de catálogo inexistente) o token antifalsificación ausente o inválido | 400 |
| Sin sesión, sesión expirada, usuario no vigente o credenciales incorrectas (en páginas: redirección al inicio de sesión) | 401 |
| Operación sobre un punto de venta distinto al del usuario | 403 |
| Recurso no encontrado (venta, comprobante, cliente, artículo) | 404 |
| Conflicto: el recurso ya existe (cliente duplicado) o concurrencia (p. ej. stock consumido por otra venta simultánea) | 409 |
| Regla de negocio incumplida (stock insuficiente, sin tarifa vigente, pago incompleto) | 422 |

### 4.7 Acceso al sistema

#### RF-21 — Iniciar sesión

El operador debe identificarse con su usuario y contraseña antes de usar el sistema.

- **Entrada:** usuario (`vchLogin`) y contraseña.
- **Salida:** sesión iniciada con la cookie de autenticación, que guarda el login, el nombre (`vchNombre`), el punto de venta asignado (`vchCodPuntoVenta`) y la expiración. Después se redirige a la pantalla de registrar venta.
- **Fuente:** `SGP_USUARIO` (`vchLogin`, `vchPassword`, `chrSituacionRegistro`, `dtmFechaInicioAct`, `dtmFechaFinAct`, `dtmFechaExpiraClave`, `vchCodPuntoVenta`).
- **Reglas:** RN-01, RN-18, RN-19.
- **Criterios de aceptación:**
  - Con usuario activo, vigente y contraseña correcta, se inicia la sesión. `vchPassword` está en texto plano (P-02): se compara de forma exacta (distingue mayúsculas) y en tiempo constante.
  - Si el usuario no existe, está inactivo, no está vigente o la contraseña es incorrecta, se rechaza con un mensaje genérico ("Usuario o contraseña incorrectos"), sin indicar cuál dato falló.
  - Si la contraseña está expirada (`dtmFechaExpiraClave` < hoy), se rechaza indicando que la contraseña expiró **[PENDIENTE P-17]**.
  - Si el usuario no tiene punto de venta asignado, se rechaza.
  - La contraseña nunca se devuelve, ni se guarda en la cookie, ni se registra en logs.
  - La sesión expira tras un tiempo configurable **[PENDIENTE P-18]**; después se aplica RF-19 (redirección al inicio de sesión o 401).
  - El operador puede cerrar sesión desde cualquier pantalla: la cookie se elimina y se vuelve al inicio de sesión.

### 4.8 Reportes

#### RF-22 — Reporte de ventas registradas

El operador debe poder obtener el reporte de las ventas registradas en su punto de venta.

- **Entrada (filtros):** rango de fechas (obligatorio), tipo de comprobante, medio de pago y usuario (opcionales).
- **Salida por venta:** fecha de emisión, tipo de comprobante, serie y número, documento y nombre del cliente, subtotal, IGV, total, medio de pago, estado y usuario que la registró.
- **Totales del reporte:** cantidad de comprobantes, suma de subtotal, IGV y total, y total recaudado por medio de pago.
- **Fuente:** `SGP_COMPROBANTE_BS`, `SGP_MEDIO_PAGO_COMPROBANTE_BS`, `SGP_MEDIO_PAGO_BS`, `SGP_MOVIMIENTO_TRANSACCION_BS`, `SGP_CABECERA_TRANSACCION_BS`.
- **Reglas:** RN-01, RN-19.
- **Criterios de aceptación:**
  - Solo incluye ventas del punto de venta del usuario de la sesión.
  - La fecha inicial no puede ser mayor que la final; el rango máximo es configurable **[PENDIENTE P-19]**.
  - Los totales del reporte coinciden con la suma de las filas listadas.
  - Si no hay ventas en el rango, devuelve el reporte vacío con totales en 0 (no es un error).
  - El reporte puede exportarse **[PENDIENTE P-19]**.

---

## 5. Reglas de negocio

| Código | Regla |
|---|---|
| **RN-01** | Solo se usan registros activos (`chrSituacionRegistro = 'A'`) de clientes, artículos, tarifas, puntos de venta, canales, impresoras, bancos y usuarios. |
| **RN-02** | Los códigos de catálogo (`SGP_TABLA` / `SGP_DETALLE_TABLA`) solo son válidos si están activos (`chrSituacionRegistro = 'A'`). |
| **RN-03** | La tasa de IGV es **0.18**. La tarifa incluye el IGV, que se desglosa en cada línea (`ValorVenta = numSubTotal / 1.18`). |
| **RN-04** | Convención del procedimiento de referencia: el subtotal de la línea incluye el IGV (`numSubTotal = Cantidad × Tarifa = ValorVenta + numIGV`). En las cabeceras, `numSubTotal = Σ (numSubTotal − numIGV)` de las líneas (base imponible), `numMontoIGV = Σ numIGV` y `numMontoTotal = numSubTotal + numMontoIGV` (detalle en RF-06). |
| **RN-05** | Todos los importes se manejan con 4 decimales. Los totales de cabecera y comprobante son la suma exacta de sus líneas. |
| **RN-06** | Una tarifa es vigente si `dtmFecInicio` ≤ fecha de venta y la fecha no pasa del día de `dtmFecCaducidad` (incluido, como en el procedimiento de referencia). |
| **RN-07** | `numCantidad` es el tope del tramo: se aplica la tarifa vigente con el **menor `numCantidad` mayor o igual** a la cantidad vendida; si hay varias con el mismo tope, la de `dtmFecInicio` más reciente. Si la cantidad supera el tope más alto, no hay precio y la venta se rechaza (P-06). |
| **RN-08** | La cantidad de cada línea debe ser mayor que 0. |
| **RN-09** | El stock se lleva por **artículo + punto de venta** y es la suma de `PGA_TRANSACCION_KARDEX.Cantidad` para esa combinación, **sin filtrar por canal**: el canal es fijo (`01`) y los movimientos grabados por el procedimiento de referencia tienen `CodTarifaCanal` en NULL. Los códigos se comparan sin espacios. No existe tabla de saldos. |
| **RN-10** | No se permite vender más que el stock disponible **[PENDIENTE P-07]**. |
| **RN-11** | Cada línea vendida genera un movimiento en el kardex con `TipoTransaccion = 'VENTA'` y la cantidad en negativo, enlazado a la transacción (`CodTransaccion`) (RF-09). |
| **RN-12** | El importe total del comprobante es igual al de la venta que lo origina. |
| **RN-13** | La serie es el primer carácter del tipo de comprobante seguido del código del punto de venta. El número es el próximo correlativo guardado en `SGP_IMPRESORA.vchCorrelativo` para esa serie, con 7 dígitos; la impresora queda con el correlativo + 1. El incremento es atómico (RF-11). |
| **RN-14** | Para emitir factura, el cliente debe tener RUC; la boleta admite cualquier documento **[PENDIENTE P-08]**. |
| **RN-15** | Venta, kardex, comprobante y pago se graban en una **única transacción de base de datos**: o se graba todo o no se graba nada. Los procedimientos de referencia no abren una transacción; el sistema nuevo sí debe hacerlo. |
| **RN-16** | Cada venta se paga al contado con un único medio de pago por el total del comprobante **[PENDIENTE P-05, P-21]**. |
| **RN-17** | Nunca se eliminan registros físicamente; la baja es lógica mediante `chrSituacionRegistro`. |
| **RN-18** | Solo pueden iniciar sesión usuarios activos y dentro de su periodo de vigencia (`dtmFechaInicioAct` ≤ hoy ≤ `dtmFechaFinAct`). |
| **RN-19** | El operador solo vende y consulta en el punto de venta que tiene asignado en `SGP_USUARIO.vchCodPuntoVenta`. El sistema lo obtiene de esa tabla al iniciar sesión; nunca lo recibe en la petición. |
| **RN-20** | No pueden existir dos clientes activos con el mismo tipo y número de documento. La BD no tiene restricción única, así que la verifica el sistema. |
| **RN-21** | Todas las ventas, comprobantes y pagos son en soles con tipo de cambio 1. El código de moneda es `SOL` (catálogo 16, como `SGP_TARIFA_CANAL.vchMoneda`). |
| **RN-22** | *Eliminada en la versión 0.5*: tarjeta de débito (`3`) y de crédito (`4`) son medios de pago distintos (P-09). |

## 6. Requerimientos no funcionales

| Código | Requerimiento |
|---|---|
| **RNF-01** | La base de datos `BDVenta` se usa tal como existe: no se crean, renombran ni alteran tablas o columnas. |
| **RNF-02** | El sistema es una aplicación web ASP.NET Core MVC sobre .NET 10, escrita en C#, con vistas Razor y jQuery. Las operaciones que las vistas invocan por AJAX devuelven JSON y, ante error, ProblemDetails (RF-20). |
| **RNF-03** | Mensajes de error y documentación en español. |
| **RNF-04** | La cadena de conexión y cualquier credencial se configuran fuera del repositorio (User Secrets o variables de entorno). |
| **RNF-05** | Cada servicio tiene pruebas unitarias; el flujo completo de venta tiene pruebas de integración, incluida la reversión ante errores y la no duplicación de correlativos. |
| **RNF-06** | La comunicación con el sistema se hace solo por HTTPS. La sesión usa la autenticación por cookie de ASP.NET Core: cookie `HttpOnly`, `Secure` y `SameSite=Strict`, con expiración (P-18), que guarda el login, el nombre y el punto de venta, nunca la contraseña. Todo formulario y toda llamada AJAX que modifica datos envía el token antifalsificación de ASP.NET Core. Las claves de protección de datos que firman la cookie se configuran fuera del repositorio. |

## 7. Supuestos y decisiones pendientes

Mientras no se confirmen, la implementación usa el supuesto indicado.

| Código | Tema | Supuesto provisional |
|---|---|---|
| **P-01** | Datos de prueba: `BDVentas.sql` muestra el esquema de la BD. | Se realizó una carga masiva de los maestros y de la tabla de clientes en `Docs/database/`. |
| **P-02** | Formato de `SGP_USUARIO.vchPassword` (`varchar(16)`): ¿texto plano, hash o cifrado? Con 16 caracteres no cabe un hash moderno. | **Resuelto**: texto plano. Se compara en tiempo constante y nunca se registra en logs. No se modifica la columna (RNF-01). Riesgo de seguridad aceptado para la versión 1. |
| **P-03** | Descuentos y recargos por línea (`chrTipDscto0X`, `tipoDscto0X`). | Fuera de la versión 1: descuento y recargo = 0. |
| **P-04** | Relación venta ↔ comprobante (el modelo permite N:M). | 1 venta = 1 comprobante. |
| **P-05** | Forma de pago. | Solo contado: el pago cubre el total al registrar la venta. Crédito queda fuera de la versión 1. |
| **P-06** | Selección de tarifa por escala de `numCantidad`. | **Resuelto** en RN-07: menor escala que cubra la cantidad. |
| **P-07** | Stock negativo. | No se permite stock negativo (RN-10); el procedimiento de referencia no lo valida. El signo y los valores del kardex quedan **resueltos** por el procedimiento de referencia (RF-09). |
| **P-08** | Factura exige RUC. | RN-14. |
| **P-09** | Datos obligatorios por medio de pago. | Tarjeta, cheque y depósito exigen banco y número de operación. **Resuelto**: `vchMedioPago` = `1` efectivo, `2` cheque, `3` tarjeta de débito, `4` tarjeta de crédito, `5` boleta de depósito. Débito y crédito son medios distintos; `vchTipoTarjeta` queda en NULL (se elimina RN-22). |
| **P-10** | Valores de estado. | Comprobante y medio de pago: `vchEstado = '2'` (cancelado), **resuelto** por el procedimiento de referencia. **Resuelto**: `intEstado` de la cabecera de la venta = 1. |
| **P-11** | Redondeo y presentación de importes. | Los importes se calculan y graban con 4 decimales (RN-05), sin redondeo del monto a cobrar (`numMontoRedondeo` = 0). El comprobante y el reporte muestran los importes con 2 decimales solo como presentación. Por confirmar el método (se asume redondeo comercial: 0.005 sube) y si se redondea cada línea o solo los totales. |
| **P-12** | `SGP_CABECERA_TRANSACCION_BS.numCodComprobante` es `numeric(9,4)`. | **Resuelto**: se rellena con el código del comprobante, como en el procedimiento de referencia (RF-10). Riesgo: `numeric(9,4)` admite hasta 99999.9999, así que un código de comprobante mayor provocaría un error al grabar. Hay que verificar el código más alto actual en `SGP_COMPROBANTE_BS`. |
| **P-13** | `SGP_CABECERA_TRANSACCION_BS.vchCodAlmacen` es obligatorio (NOT NULL), aunque el stock no depende del almacén. | **Resuelto**: el punto de venta es el de `SGP_USUARIO`; se usa su primer almacén activo (menor `vchCodAlmacen`) en `SGP_PUNTO_VENTA_ALMACEN`. Sin almacén activo, la venta se rechaza. |
| **P-14** | Formato de la serie y del número de comprobante. | Número: 7 dígitos con ceros, **resuelto** por el procedimiento de referencia. Serie: **resuelto**, `F` (factura) o `B` (boleta) + código del punto de venta. |
| **P-15** | ¿El precio de `SGP_TARIFA.numPrecioUnidad` incluye IGV? ¿Qué se graba en `numMontoBruto` de la línea? | **Resuelto**: la tarifa **incluye** IGV; el IGV se desglosa (`ValorVenta = numSubTotal / 1.18`) y `numMontoBruto` = cantidad × tarifa (RF-06). Se aparta del procedimiento de referencia, que suma el IGV sobre la tarifa. |
| **P-16** | Moneda y tipo de cambio. | **Resuelto** en RN-21: siempre soles con tipo de cambio 1. |
| **P-17** | Contraseña expirada (`dtmFechaExpiraClave`). | Se rechaza el inicio de sesión; el cambio de contraseña queda fuera de la versión 1. Si la columna es NULL, no expira. |
| **P-18** | Duración de la sesión y bloqueo por intentos fallidos. | Cookie de sesión válida por 8 horas (un turno), sin renovación automática. Sin bloqueo por intentos en la versión 1 (no hay columna para contarlos). |
| **P-19** | Formato de exportación y rango máximo del reporte. | Consulta en pantalla; exportación a Excel/PDF por confirmar. Rango máximo de 31 días. |
| **P-20** | Códigos de `vchTipDocumento` y `vchTipPersona`, otros documentos (carné de extranjería, pasaporte) y valores de `vchTipoCliente`, `vchZonaVenta` y `vchEstado` al crear un cliente. | **Resuelto con los catálogos de la BD**: tipo de documento (catálogo 2) `1` RUC, `2` DNI, `3` otros, `4` carné de extranjería; tipo de persona (catálogo 5) `1` natural, `2` jurídica. Solo se registran DNI (8 dígitos) y RUC (11). RUC que empieza con 20 → jurídica (razón social); otro RUC → natural con negocio (nombres y apellidos). Las demás columnas quedan en NULL. |
| **P-21** | Pago mixto (varios medios de pago en una venta). | El procedimiento de referencia registra un solo medio de pago por el total, y así se define la versión 1 (RN-16). Por confirmar si el negocio necesita pago mixto (p. ej. parte en efectivo y parte con tarjeta). |
| **P-22** | Valores heredados discutibles de los procedimientos de referencia. | Tipo de cambio: los procedimientos usan 3.25 por defecto; se graba 1 porque todo es en soles (RN-21). `numMontoPagado` del comprobante: los procedimientos lo dejan en 0; se graba el monto pagado (RF-14). Por confirmar que estos cambios no afectan otros procesos que lean esos datos. |

## 8. Fuera de alcance (versión 1)

- Anulación de comprobantes y notas de crédito o débito.
- Ventas al crédito y cobranza posterior.
- Descuentos, recargos y redondeo del monto a cobrar (`numMontoRedondeo`). La presentación con 2 decimales sí está incluida (P-11).
- Ventas en moneda extranjera (RN-21).
- Modificación y baja de clientes (el alta sí está incluida, RF-03).
- Mantenimiento (alta, baja, modificación) de artículos, tarifas e impresoras.
- Pagos digitales (`PGA_ORDEN_PAGO_DIGITAL`).
- Venta de publicaciones o avisos (campos de aviso del detalle del comprobante).
- Integración con Baan y envío a SUNAT.
- Impresión de comprobantes.
- Gestión de usuarios, roles y permisos; cambio y recuperación de contraseña.
