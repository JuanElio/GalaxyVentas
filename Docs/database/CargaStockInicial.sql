/*
  Carga inicial de stock en PGA_TRANSACCION_KARDEX (datos de prueba).

  PGA_TRANSACCION_KARDEX está vacío y el stock es la suma de sus cantidades por
  artículo + punto de venta (RN-09), así que sin esta carga no se puede vender nada.

  Qué inserta: un ingreso de @Cantidad unidades por cada combinación de
    - artículo activo asignado a una categoría activa (SGP_CATEGORIA_ARTICULO) para el tipo de
      orden @TipoOrden y el canal @Canal, con tarifa vigente en ese canal (los que muestra la
      pantalla de venta), y
    - punto de venta activo que tiene al menos un usuario activo y vigente
      (o solo @PuntoVenta si se indica).

  Uso:
    1. Ejecutar tal cual (@Confirmar = 0): muestra lo que insertaría y deshace todo (ROLLBACK).
    2. Revisar el resultado y volver a ejecutar con @Confirmar = 1 para grabar (COMMIT).

  Es idempotente: no vuelve a cargar una combinación que ya tenga un movimiento de carga inicial.
  Los movimientos se identifican por Proceso = 'CARGA INICIAL'. Para anularlos (borrado lógico,
  sin DELETE físico, RN-17) ver el bloque comentado al final.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Cantidad   numeric(18, 4) = 100;     -- unidades por artículo y punto de venta
DECLARE @PuntoVenta varchar(4)     = NULL;    -- p. ej. '001' para cargar solo ese punto de venta
DECLARE @Confirmar  bit            = 0;       -- 0 = solo vista previa (ROLLBACK); 1 = grabar (COMMIT)
DECLARE @TipoOrden  varchar(3)     = 'V41';   -- tipo de orden y canal de la venta (ParametrosVenta)
DECLARE @Canal      varchar(3)     = '30';

DECLARE @Proceso varchar(50) = 'CARGA INICIAL';
DECLARE @Creador varchar(50) = 'carga_inicial';
DECLARE @Ahora   datetime    = GETDATE();
DECLARE @Hoy     datetime    = CAST(CAST(@Ahora AS date) AS datetime);

BEGIN TRANSACTION;

;WITH Articulos AS (
    -- Mismo criterio que la pantalla: activo, con categoría de venta y con tarifa vigente en el canal (RN-06)
    SELECT DISTINCT LTRIM(RTRIM(a.vchCodArticulo)) AS CodArticulo
    FROM SGP_ARTICULO a
    JOIN SGP_CATEGORIA_ARTICULO ca ON LTRIM(RTRIM(ca.vchCodArticulo)) = LTRIM(RTRIM(a.vchCodArticulo))
    JOIN SGP_CATEGORIA_VENTA cv ON cv.intCodCategoria = ca.intCodCategoria
    JOIN SGP_TARIFA t ON LTRIM(RTRIM(t.vchCodArticulo)) = LTRIM(RTRIM(a.vchCodArticulo))
    WHERE a.chrSituacionRegistro = 'A'
      AND ca.vchestado = 'A' AND cv.vchestado = 'A'
      AND LTRIM(RTRIM(ca.vchCodTipoOrden)) = @TipoOrden
      AND LTRIM(RTRIM(ca.vchCodTarifaCanal)) = @Canal
      AND LTRIM(RTRIM(t.vchCodTarifaCanal)) = @Canal
      AND t.chrSituacionRegistro = 'A'
      AND t.numPrecioUnidad IS NOT NULL
      AND t.dtmFecInicio <= @Ahora
      AND t.dtmFecCaducidad >= @Hoy
),
PuntosVenta AS (
    -- Se guarda el código tal como está en SGP_PUNTO_VENTA (con espacio inicial, p. ej. ' 001')
    SELECT p.vchCodPuntoVenta AS CodPuntoVenta
    FROM SGP_PUNTO_VENTA p
    WHERE p.chrSituacionRegistro = 'A'
      AND (@PuntoVenta IS NULL OR LTRIM(RTRIM(p.vchCodPuntoVenta)) = LTRIM(RTRIM(@PuntoVenta)))
      AND EXISTS (
          SELECT 1
          FROM SGP_USUARIO u
          WHERE LTRIM(RTRIM(u.vchCodPuntoVenta)) = LTRIM(RTRIM(p.vchCodPuntoVenta))
            AND u.chrSituacionRegistro = 'A'
            AND @Hoy BETWEEN CAST(CAST(u.dtmFechaInicioAct AS date) AS datetime)
                         AND CAST(CAST(u.dtmFechaFinAct AS date) AS datetime))
)
INSERT INTO PGA_TRANSACCION_KARDEX (
    CodArticulo, CodTarifaCanal, CodPuntoVenta, TipoTransaccion, FechaTransaccion, Cantidad,
    CodTransaccion, Descripcion, Estado, Proceso, GuidRegistro, SituacionRegistro, Creador, FechaCreacion)
SELECT
    a.CodArticulo, @Canal, p.CodPuntoVenta, 'INGRESO', @Hoy, @Cantidad,
    NULL, 'Carga inicial de stock', '', @Proceso, CONVERT(varchar(100), NEWID()), 'A', @Creador, @Ahora
FROM Articulos a
CROSS JOIN PuntosVenta p
WHERE NOT EXISTS (
    SELECT 1
    FROM PGA_TRANSACCION_KARDEX k
    WHERE k.Proceso = @Proceso
      AND k.SituacionRegistro = 'A'
      AND LTRIM(RTRIM(k.CodArticulo)) = a.CodArticulo
      AND LTRIM(RTRIM(k.CodPuntoVenta)) = LTRIM(RTRIM(p.CodPuntoVenta)));

DECLARE @Insertados int = @@ROWCOUNT;

-- Vista previa: resumen por punto de venta de lo insertado en esta ejecución
SELECT
    LTRIM(RTRIM(k.CodPuntoVenta)) AS PuntoVenta,
    MAX(p.vchDescripcion)          AS Descripcion,
    COUNT(*)                       AS Articulos,
    SUM(k.Cantidad)                AS Unidades
FROM PGA_TRANSACCION_KARDEX k
LEFT JOIN SGP_PUNTO_VENTA p ON p.vchCodPuntoVenta = k.CodPuntoVenta
WHERE k.Proceso = @Proceso AND k.FechaCreacion = @Ahora
GROUP BY LTRIM(RTRIM(k.CodPuntoVenta))
ORDER BY PuntoVenta;

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT CONCAT('Grabados ', @Insertados, ' movimientos de carga inicial.');
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT CONCAT('Vista previa: se insertarían ', @Insertados, ' movimientos. No se grabó nada (@Confirmar = 0).');
END;

/*
-- Anular la carga inicial (borrado lógico): el stock vuelve a calcularse sin estos movimientos
UPDATE PGA_TRANSACCION_KARDEX
SET SituacionRegistro = 'I', Modificador = 'carga_inicial', FechaModificacion = GETDATE()
WHERE Proceso = 'CARGA INICIAL' AND SituacionRegistro = 'A';
*/
