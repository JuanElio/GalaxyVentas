/*
  Asigna artículos a categorías de venta para el tipo de orden y canal de la venta (datos de prueba).

  La pantalla de venta lista los artículos asignados en SGP_CATEGORIA_ARTICULO al tipo de orden
  @TipoOrden y al canal @Canal. Con los datos originales solo la categoría Libros tiene artículos
  para V41 / 30; este script agrega los artículos que ya pertenecen a otra categoría (con otro tipo
  de orden o canal) y que se pueden vender en el canal: activos, con tarifa vigente en @Canal y con
  tasa de IVA vigente en SGP_DETALLE_IVA.

  La clave de SGP_CATEGORIA_ARTICULO es (intCodCategoria, vchCodArticulo, vchCodTarifaCanal) y no
  incluye el tipo de orden: si la categoría ya tiene al artículo con el canal '30' (p. ej. con V08),
  la nueva fila se graba con ' 30' (la tabla ya mezcla ambos formatos y el sistema compara sin
  espacios). No se modifica ninguna fila existente.

  Uso:
    1. Ejecutar tal cual (@Confirmar = 0): muestra lo que insertaría y deshace todo (ROLLBACK).
    2. Revisar el resultado y volver a ejecutar con @Confirmar = 1 para grabar (COMMIT).
    3. Ejecutar CargaStockInicial.sql para cargar el stock de los artículos agregados.

  Es idempotente: no toca los artículos que ya tienen alguna fila para el tipo de orden y el canal
  (se quedan en su categoría actual). Para quitar las asignaciones (borrado lógico) ver el bloque al final.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TipoOrden varchar(3) = 'V41';   -- tipo de orden y canal de la venta (ParametrosVenta)
DECLARE @Canal     varchar(3) = '30';
DECLARE @Confirmar bit        = 0;       -- 0 = solo vista previa (ROLLBACK); 1 = grabar (COMMIT)

DECLARE @Ahora datetime = GETDATE();
DECLARE @Hoy   datetime = CAST(CAST(@Ahora AS date) AS datetime);

DECLARE @Asignados TABLE (intCodCategoria int, vchCodArticulo varchar(16), vchCodTarifaCanal varchar(3));

BEGIN TRANSACTION;

;WITH Candidatos AS (
    -- Artículos de una categoría activa, vendibles en el canal y aún sin ninguna fila para el tipo de
    -- orden + canal. Una sola categoría por artículo: la de menor código, la misma que usa la pantalla.
    SELECT MIN(ca.intCodCategoria) AS intCodCategoria, LTRIM(RTRIM(ca.vchCodArticulo)) AS CodArticulo
    FROM SGP_CATEGORIA_ARTICULO ca
    JOIN SGP_CATEGORIA_VENTA cv ON cv.intCodCategoria = ca.intCodCategoria AND cv.vchestado = 'A'
    JOIN SGP_ARTICULO a ON LTRIM(RTRIM(a.vchCodArticulo)) = LTRIM(RTRIM(ca.vchCodArticulo))
    WHERE ca.vchestado = 'A'
      AND a.chrSituacionRegistro = 'A'
      AND EXISTS (
          SELECT 1 FROM SGP_TARIFA t
          WHERE LTRIM(RTRIM(t.vchCodArticulo)) = LTRIM(RTRIM(a.vchCodArticulo))
            AND LTRIM(RTRIM(t.vchCodTarifaCanal)) = @Canal
            AND t.chrSituacionRegistro = 'A'
            AND t.numPrecioUnidad IS NOT NULL
            AND t.dtmFecInicio <= @Ahora
            AND t.dtmFecCaducidad >= @Hoy)
      AND EXISTS (
          SELECT 1 FROM SGP_DETALLE_IVA d
          WHERE LTRIM(RTRIM(d.vchCodIva)) = LTRIM(RTRIM(a.vchCodIva))
            AND d.chrSituacionRegistro = 'A'
            AND d.numValorIGV IS NOT NULL
            AND d.dtmFechaVigencia <= @Ahora)
      AND NOT EXISTS (
          SELECT 1 FROM SGP_CATEGORIA_ARTICULO x
          WHERE LTRIM(RTRIM(x.vchCodArticulo)) = LTRIM(RTRIM(ca.vchCodArticulo))
            AND LTRIM(RTRIM(x.vchCodTipoOrden)) = @TipoOrden
            AND LTRIM(RTRIM(x.vchCodTarifaCanal)) = @Canal)
    GROUP BY LTRIM(RTRIM(ca.vchCodArticulo))
)
INSERT INTO SGP_CATEGORIA_ARTICULO (intCodCategoria, vchCodArticulo, vchCodTipoOrden, vchCodTarifaCanal, vchestado)
OUTPUT inserted.intCodCategoria, inserted.vchCodArticulo, inserted.vchCodTarifaCanal INTO @Asignados
SELECT c.intCodCategoria, c.CodArticulo, @TipoOrden, f.Canal, 'A'
FROM Candidatos c
CROSS APPLY (
    -- Formato del canal libre en la clave: '30' o, si ya está ocupado, ' 30'
    SELECT TOP 1 v.Canal
    FROM (VALUES (1, @Canal), (2, ' ' + @Canal)) v (Orden, Canal)
    WHERE NOT EXISTS (
        SELECT 1 FROM SGP_CATEGORIA_ARTICULO x
        WHERE x.intCodCategoria = c.intCodCategoria
          AND LTRIM(RTRIM(x.vchCodArticulo)) = c.CodArticulo
          AND x.vchCodTarifaCanal = v.Canal)
    ORDER BY v.Orden
) f;

DECLARE @Insertados int = @@ROWCOUNT;

-- Vista previa: filas insertadas en esta ejecución
SELECT s.intCodCategoria, cv.vchDescripcion AS Categoria, s.vchCodArticulo, a.vchDescripcion AS Articulo,
       '[' + s.vchCodTarifaCanal + ']' AS Canal
FROM @Asignados s
JOIN SGP_CATEGORIA_VENTA cv ON cv.intCodCategoria = s.intCodCategoria
LEFT JOIN SGP_ARTICULO a ON LTRIM(RTRIM(a.vchCodArticulo)) = s.vchCodArticulo
ORDER BY s.intCodCategoria, s.vchCodArticulo;

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT CONCAT('Grabadas ', @Insertados, ' asignaciones de categoría para ', @TipoOrden, ' / ', @Canal, '.');
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT CONCAT('Vista previa: se insertarían ', @Insertados, ' asignaciones. No se grabó nada (@Confirmar = 0).');
END;

/*
-- Quitar las asignaciones de V41 / 30 que no son de Libros (borrado lógico, sin DELETE físico)
UPDATE SGP_CATEGORIA_ARTICULO
SET vchestado = 'I'
WHERE LTRIM(RTRIM(vchCodTipoOrden)) = 'V41' AND LTRIM(RTRIM(vchCodTarifaCanal)) = '30' AND intCodCategoria <> 3;
*/
