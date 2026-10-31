# Manual de usuario — GalaxyVentas

| | |
|---|---|
| **Dirigido a** | Operadores del punto de venta (vendedores y cajeros) |
| **Versión** | 1.0 — 30/10/2026 |

Este manual explica cómo usar GalaxyVentas para vender, emitir comprobantes, cobrar, consultar ventas, obtener el reporte del día y cambiar su contraseña.

## Contenido

1. [Antes de empezar](#1-antes-de-empezar)
2. [Iniciar y cerrar sesión](#2-iniciar-y-cerrar-sesión)
3. [El menú](#3-el-menú)
4. [Registrar una venta](#4-registrar-una-venta)
5. [El comprobante](#5-el-comprobante)
6. [Consultas](#6-consultas)
7. [Reporte de ventas](#7-reporte-de-ventas)
8. [Mis datos y cambio de contraseña](#8-mis-datos-y-cambio-de-contraseña)
9. [Mensajes frecuentes](#9-mensajes-frecuentes)
10. [Preguntas frecuentes](#10-preguntas-frecuentes)

---

## 1. Antes de empezar

- Necesita un **usuario y contraseña** entregados por el administrador del sistema.
- Su usuario está asignado a **un punto de venta**: todas sus ventas, consultas y reportes son de ese punto de venta. No puede elegir otro.
- Use un navegador actualizado (Chrome, Edge o Firefox). La dirección del sistema se la indica el administrador.
- La sesión dura **8 horas**. Después el sistema le pedirá ingresar de nuevo.

## 2. Iniciar y cerrar sesión

### Iniciar sesión

1. Abra la dirección del sistema. Aparece la pantalla **Bienvenido**.
2. Escriba su **Usuario** y su **Contraseña**. El botón con forma de ojo muestra u oculta la contraseña.
3. Pulse **Ingresar**.

Si los datos son correctos, el sistema abre la pantalla **Registrar venta**.

> La contraseña distingue mayúsculas y minúsculas.

Si no puede ingresar, revise la sección [Mensajes frecuentes](#9-mensajes-frecuentes).

### Cerrar sesión

Pulse **Cerrar sesión** al pie del menú lateral. Hágalo siempre que deje el equipo, sobre todo si es compartido.

## 3. El menú

El menú lateral está a la izquierda (en el celular, se abre con el botón ☰ de la parte superior):

| Opción | Para qué sirve |
|---|---|
| **Inicio** | Página de bienvenida con acceso directo a la venta. |
| **Registrar venta** | Atender a un cliente y emitir su comprobante. |
| **Consultas** | Buscar una venta o un comprobante ya emitido. |
| **Reporte de ventas** | Ver las ventas de un periodo y sus totales. |
| **Mis datos** | Ver sus datos de usuario y cambiar su contraseña. |

Al pie del menú se ven su nombre, su usuario y su punto de venta.

## 4. Registrar una venta

La pantalla **Registrar venta** está dividida en secciones numeradas. A la derecha (o al final, en pantallas pequeñas) está el **Resumen** con los totales y el botón **Registrar venta**.

En la parte superior se muestran los datos fijos de la venta, que no se pueden cambiar: punto de venta, canal, moneda (soles), tipo de orden, almacén y usuario.

### 4.1 Paso 1 — Cliente

1. En **Documento (DNI / RUC) o nombre / razón social** escriba:
   - el número de documento (solo dígitos), por ejemplo `45678912` o `20512345678`, **o**
   - el nombre o la razón social, al menos 3 letras; las palabras pueden ir en cualquier orden (por ejemplo `quispe rosa`).
2. Pulse **Buscar**.
3. Según el resultado:
   - **Un cliente:** se muestra su ficha (código, documento, nombre y dirección). Continúe con el paso 2.
   - **Varios clientes:** se abre la ventana **Seleccione el cliente**. Elija el correcto revisando documento y nombre.
   - **Ninguno:** registre el cliente (ver 4.2).

### 4.2 Registrar un cliente nuevo

Si el cliente no existe:

1. Abra **Registrar cliente nuevo**.
2. Elija el **Tipo de documento** (DNI o RUC) y escriba el **Número de documento**:
   - DNI: 8 dígitos.
   - RUC: 11 dígitos.
3. El **Tipo de persona** se completa solo: RUC que empieza con 20 es persona jurídica; los demás, persona natural.
4. Complete:
   - Persona natural: **Nombres** y **Apellidos**.
   - Persona jurídica: **Razón social**.
   - **Dirección de facturación** (obligatoria para RUC).
   - **Teléfono** y **Correo** (opcionales).
5. Pulse **Registrar cliente**. El cliente queda seleccionado para la venta.

Si el documento ya está registrado, el sistema lo indica: búsquelo con **Buscar**.

### 4.3 Paso 2 — Artículos

La lista muestra los artículos que se pueden vender, **agrupados por categoría** (por ejemplo, *Bienes* y *Libros*). Por cada artículo verá:

| Columna | Significado |
|---|---|
| Código / Descripción / Und. | Identificación y unidad de medida. |
| **Precio (con IGV)** | Precio de una unidad. Con más unidades el precio puede cambiar según la escala de la tarifa. |
| **IGV** | Porcentaje de IGV del artículo, o **Exonerado** (por ejemplo, los libros). |
| **Stock** | Unidades disponibles en su punto de venta. En amarillo si quedan menos de 10; en rojo si no hay. |

Para agregar un artículo:

1. Si lo necesita, filtre por **Categoría** o escriba parte del código o de la descripción en **Buscar por código o descripción**.
2. Escriba la cantidad en **Cant.** (número entero mayor que 0).
3. Pulse **Agregar**.

Si agrega un artículo que ya está en el detalle, se suma la cantidad. Los artículos sin stock no se pueden agregar.

### 4.4 Paso 3 — Detalle de la venta

El detalle muestra cada línea con su **Tarifa**, **Valor venta** (sin IGV), **IGV** y **Subtotal (con IGV)**. El sistema calcula todos los importes: usted no ingresa montos.

- Puede cambiar la cantidad de una línea; los importes se recalculan.
- Puede quitar una línea con el botón **×**.
- Si una línea aparece **observada** (por ejemplo, «Stock insuficiente»), corríjala antes de registrar la venta.

### 4.5 Paso 4 — Comprobante

1. Elija el **Tipo de comprobante**: **Boleta** o **Factura**.
   - La **factura** solo se emite a clientes con **RUC**.
2. La **Serie y número** muestran el número que tendrá el comprobante (por ejemplo `B001-0000845`). Es informativo: el número definitivo se asigna al registrar la venta.

### 4.6 Paso 5 — Medio de pago

Cada venta se paga con **un solo medio de pago por el total**. El **Monto** se completa solo con el total de la venta.

| Medio | Datos adicionales |
|---|---|
| Efectivo | Ninguno |
| Cheque | **Banco** y **N.º de operación** |
| Tarjeta de débito | **Banco** y **N.º de operación** |
| Tarjeta de crédito | **Banco** y **N.º de operación** |
| Boleta de depósito | **Banco** y **N.º de operación** |

### 4.7 Registrar la venta

1. Revise el **Resumen**: operación gravada (base imponible), IGV, total y medio de pago.
2. Pulse **Registrar venta**.
3. El sistema graba la venta, emite el comprobante, descuenta el stock y registra el pago, todo junto. Si algo falla, **no se graba nada** y se muestra el motivo.
4. Si todo es correcto, se abre el comprobante con el aviso **Venta registrada**.

## 5. El comprobante

El comprobante muestra:

- Serie y número, fecha de emisión y datos del cliente.
- **Detalle** de los ítems con precio unitario, valor venta, IGV y subtotal.
- Totales: operación gravada, descuentos, IGV y total.
- **Medio de pago** con banco, número de operación y monto.
- **Trazabilidad**: número de transacción, código de comprobante, canal, almacén, tipo de orden y usuario que lo registró.

Pulse **Imprimir** para imprimirlo o guardarlo como PDF desde el navegador.

> Los importes se graban con 4 decimales y se muestran redondeados a 2.

## 6. Consultas

La opción **Consultas** permite encontrar una venta ya registrada en su punto de venta.

- **Buscar venta:** escriba el **N.º de transacción** y pulse **Buscar venta**.
- **Buscar comprobante:** elija el **Tipo** (Boleta o Factura), escriba el **Número** (basta el número sin ceros, por ejemplo `845`) y pulse **Buscar comprobante**.
- **Últimas ventas del punto de venta:** la tabla inferior muestra las 20 ventas más recientes; pulse **Ver** para abrir su comprobante.

Si la venta no existe o es de otro punto de venta, el sistema lo indica.

## 7. Reporte de ventas

1. Abra **Reporte de ventas**. Al entrar se muestran las ventas del día.
2. En **Filtros** indique:
   - **Fecha desde** y **Fecha hasta** (obligatorias; como máximo 31 días).
   - Opcionalmente, **Tipo de comprobante**, **Medio de pago** y **Usuario**.
3. Pulse **Generar reporte**.

El resultado muestra cada venta (fecha, comprobante, cliente, subtotal, IGV, total, medio de pago, estado y usuario) y los totales: cantidad de comprobantes, sumas de subtotal, IGV y total, y **Recaudado por medio de pago**, útil para el cuadre de caja.

Pulse **Imprimir** para imprimirlo; los filtros no se imprimen.

## 8. Mis datos y cambio de contraseña

### Ver sus datos

Abra **Mis datos**. Verá su usuario, nombre, documento, correo, teléfono, punto de venta, vigencia de su usuario y la fecha de vencimiento de su contraseña. Si algún dato es incorrecto, pida la corrección al administrador.

### Cambiar su contraseña

1. En **Cambiar contraseña** escriba su **Contraseña actual**.
2. Escriba la **Nueva contraseña**. Debe:
   - tener entre 6 y 16 caracteres,
   - incluir al menos una letra y un número,
   - no tener espacios,
   - ser distinta de la actual.
3. Repítala en **Confirmar nueva contraseña**.
4. Pulse **Cambiar contraseña**. Verá el mensaje «Su contraseña se cambió correctamente.».

Desde ese momento debe ingresar con la nueva contraseña.

## 9. Mensajes frecuentes

| Mensaje | Qué significa | Qué hacer |
|---|---|---|
| Usuario o contraseña incorrectos. | Algún dato no coincide, o el usuario está inactivo o fuera de su vigencia. | Revise mayúsculas y minúsculas. Si persiste, consulte al administrador. |
| Su contraseña expiró… | La contraseña venció. | Pida al administrador que la renueve. |
| El usuario no tiene un punto de venta asignado. | Su usuario no tiene punto de venta. | Consulte al administrador. |
| La sesión no está iniciada o expiró. | Pasaron 8 horas o se cerró la sesión. | Vuelva a ingresar. |
| Seleccione el cliente. / Agregue al menos un artículo. | Falta un dato de la venta. | Complete el paso indicado. |
| Stock insuficiente: disponible X, solicitado Y. | No hay unidades suficientes en su punto de venta. | Reduzca la cantidad. |
| No hay tarifa vigente para vender N unidades de… | La cantidad supera la escala de precios del artículo. | Reduzca la cantidad o consulte al administrador. |
| Para emitir una factura el cliente debe tener RUC. | Se eligió factura para un cliente con DNI. | Emita boleta o seleccione un cliente con RUC. |
| Seleccione el banco. / Ingrese el número de operación. | El medio de pago elegido los exige. | Complete ambos datos. |
| El stock del artículo … está siendo usado por otra venta. | Otra caja está vendiendo el mismo artículo en ese momento. | Espere unos segundos y vuelva a registrar. |
| No existe una impresora activa para la serie … | La serie del comprobante no está configurada. | Consulte al administrador. |
| La contraseña actual no es correcta. | Al cambiar la contraseña, la actual no coincide. | Escríbala de nuevo. |
| El rango de fechas no puede superar 31 días. | El reporte tiene un rango demasiado amplio. | Divida el periodo. |

## 10. Preguntas frecuentes

**¿Puedo vender en otro punto de venta?**
No. Solo opera en el punto de venta asignado a su usuario.

**¿Puedo cobrar con dos medios de pago (por ejemplo, parte en efectivo y parte con tarjeta)?**
No en esta versión: cada venta se paga con un solo medio por el total.

**¿Puedo anular un comprobante?**
No desde este sistema. La anulación y las notas de crédito están fuera de alcance.

**¿Por qué un artículo no aparece en la lista?**
Solo se listan artículos activos, con categoría de venta, tarifa vigente e IGV configurado. Si falta alguno, consulte al administrador.

**¿Por qué el precio cambió al aumentar la cantidad?**
Algunos artículos tienen escalas de precio según la cantidad vendida; el sistema aplica la que corresponde.

**¿Qué pasa si se corta la conexión mientras registro la venta?**
La venta se graba completa o no se graba. Antes de repetirla, búsquela en **Consultas** (por ejemplo en *Últimas ventas*) para no duplicarla.
