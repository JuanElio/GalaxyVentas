

CREATE PROCEDURE [dbo].[pga_tabla_listar_ubigeo_departamento]    
AS    
BEGIN    
     
 SET NOCOUNT ON    
    
 SELECT     
  CodUbigeo = vchCodUbigeo,     
  Descripcion  = vchDescripcion     
 FROM SGP_UBIGEO    
 WHERE chrSituacionRegistro = 'A'    
 -- AND SUBSTRING(vchCodUbigeo,1,1) <> '9' -- Exterior    
 AND SUBSTRING(vchCodUbigeo,3,4) = '0000'    
 ORDER BY vchDescripcion;    
END    

GO

  
CREATE PROCEDURE [dbo].[pga_tabla_listar_ubigeo_provincia]    
@vchCodUbigeo varchar(6)    
AS    
BEGIN    
     
 SET NOCOUNT ON    
    
 SELECT     
  CodUbigeo = vchCodUbigeo,     
  Descripcion  = vchDescripcion     
 FROM SGP_UBIGEO    
 WHERE chrSituacionRegistro = 'A'    
 -- AND SUBSTRING(vchCodUbigeo,1,1) <> '9' -- Exterior    
 AND SUBSTRING(vchCodUbigeo,1,2) = SUBSTRING(@vchCodUbigeo, 1, 2)    
 AND SUBSTRING(vchCodUbigeo,5,2) = '00'    
 AND vchCodUbigeo <> @vchCodUbigeo    
 ORDER BY vchDescripcion;    
    
END  

GO

CREATE PROCEDURE [dbo].[pga_tabla_listar_ubigeo_distrito]    
@vchCodUbigeo varchar(6) -- UbigeoProvincia    
AS    
BEGIN    
     
 SET NOCOUNT ON    
    
 SELECT     
  CodUbigeo = vchCodUbigeo,     
  Descripcion  = vchDescripcion     
 FROM SGP_UBIGEO    
 WHERE chrSituacionRegistro = 'A'    
 -- AND SUBSTRING(vchCodUbigeo,1,1) <> '9' -- Exterior    
 AND SUBSTRING(vchCodUbigeo, 1, 4) = SUBSTRING(@vchCodUbigeo, 1, 4)    
 AND vchCodUbigeo <> @vchCodUbigeo    
 ORDER BY vchDescripcion;    
    
END  

GO
CREATE PROCEDURE [dbo].[pga_tabla_listar_zona_venta_por_punto_venta]    
@vchCodPuntoVenta varchar(4)    
AS        
BEGIN        
        
 SET NOCOUNT ON;        
        
 SELECT         
  zna.vchZonaVenta as CodZonaVenta,         
  ltrim(rtrim(zna.vchDescripcion)) as Descripcion,        
  zna.vchCodigoBaan  as CodigoBaan        
  -- zna.chrSituacionRegistro, zna.vchCreador, zna.dtmFechaCreacion, zna.vchModificador, zna.dtmFechaModificacion, zna.vchCodigoBaan         
 FROM SGP_ZONA_VENTA zna        
 WHERE exists (        
  SELECT 1         
  FROM SGP_PUNTO_VENTA pvta        
  WHERE pvta.chrSituacionRegistro = 'A'        
  AND (pvta.chrFlgOperador = 'S' and pvta.vchCodPuntoVenta = @vchCodPuntoVenta)        
  AND pvta.vchZonaVenta = zna.vchZonaVenta        
 )        
 AND zna.chrSituacionRegistro = 'A'      
 ORDER BY zna.vchZonaVenta;        
        
END 
GO
CREATE PROCEDURE [dbo].[pga_almacen_listar_por_punto_venta]        
@vchCodPuntoVenta char(4)        
AS            
BEGIN      
       
 SET NOCOUNT ON            
        
 SELECT a.vchCodAlmacen , a.vchDescripcion as vchDescripcion        
 FROM SGP_ALMACEN a       
  LEFT OUTER JOIN SGP_PUNTO_VENTA_ALMACEN b ON (a.vchCodAlmacen = b.vchCodAlmacen)       
 WHERE  b.vchCodPuntoVenta = @vchCodPuntoVenta       
 and  a.chrSituacionRegistro = 'A'           
 ORDER BY a.vchDescripcion          
          
END

GO

CREATE PROCEDURE [dbo].[pga_tabla_listar_tipo_liquidacion]  
@vchCodTipoOrden varchar(4)        
AS        
BEGIN        
        
SET NOCOUNT ON;  
        
SELECT intCodTipoLiquidacion, vchDescripcion   
FROM SGP_TIPO_LIQUIDACION   
WHERE intCodTipoLiquidacion in   
 (SELECT intCodTipoLiquidacion   
  FROM SGP_TIPO_ORDEN   
  WHERE vchCodTipoOrden =  @vchCodTipoOrden)    
AND chrSituacionRegistro = 'A'    
      
END 

GO

CREATE PROCEDURE [dbo].[pga_cliente_obtener_por_clave]              
@numCodCliente INT                      
AS                      
BEGIN                      
                      
 SET NOCOUNT ON;                      
               
 SELECT               
     --cli.numCodCliente              
  CAST(cli.numCodCliente as INT) as numCodCliente                 
    ,cli.vchCodRuta              
    ,cli.vchNombres              
    ,cli.vchApellidos              
    ,cli.vchRazonSocial              
    ,cli.vchTipPersona              
    ,cli.vchTipDocumento              
    ,cli.vchNumDocumento                  
    ,cli.vchZonaVenta              
    ,cli.vchTipoCliente              
   -- ,cli.vchCodigoBaan              
 ,case                    
    when isnull(cli.vchCodigoBaan, '000000') = '000000' then                    
  ''                    
    else                    
  cli.vchCodigoBaan                    
  end vchCodigoBaan            
    ,cli.vchTelefono              
    ,cli.vchTelefax              
    ,cli.vchEmail              
    ,isnull(chrFlgClienteFrecuente, 'N') chrFlgClienteFrecuente               
 ,cli.chrFlgConvenio                  
                
 ,cli.vchDirecFacturacion              
    ,cli.vchFacUbigeo              
    ,cli.vchDirecEntrega              
    ,cli.vchEntUbigeo              
            
    ,cli.vchEstado                  
    ,cli.chrSituacionRegistro                 
    ,cli.vchCreador                
    ,cli.dtmFechaCreacion               
    ,cli.vchModificador                    
    ,cli.dtmFechaModificacion                  
    --,CAST(cli.numCodigoUsuario AS INT)  as CodUsuario                      
    --,CAST(cli.numCodClienteOperador AS INT) as CodClienteOperador                      
    --,CAST(cli.numCodClienteWeb AS INT) as CodClienteWeb                      
    --,cli.numCodClienteCentral as CodClienteCentral                      
    --,cli.chrClienteEDP as ClienteEDP                      
    ,zna.vchDescripcion as vchZonaVentaDescripcion                      
    ,t1.vchDescripcion as vchTipoPersonaDescripcion                 
    ,t2.vchDescripcion as vchTipoDocumentoDescripcion                 
 /*  
 ,CASE                 
  WHEN cli.vchTipPersona = '1' THEN                
   LTRIM(RTRIM(ISNULL(cli.vchApellidos,'') + ' ' + ISNULL(cli.vchNombres,'')))                
  ELSE                 
   RTRIM(LTRIM(ISNULL(cli.vchRazonSocial,'')))                     
 END as vchRazonSocialNombre     
 */  
 ,RTRIM(LTRIM(  
 ISNULL(cli.vchApellidos,'') + ' ' + ISNULL(cli.vchNombres,'') + ' ' + ISNULL(cli.vchRazonSocial, '')    
   )) as vchRazonSocialNombre   
 ,CAST (                
  CASE                 
   WHEN cli.chrFlgClienteFrecuente = 'S' THEN                
   1                
   ELSE                
   0                
  END as bit) as bitClienteFrecuente                
  ,case                 
   when ISNULL(cli.vchFacUbigeo, '') <> '' then                
    prov.vchDescripcion + ' - ' + prov.vchDescripcion + ' - ' + dist.vchDescripcion                
   else                
    ''                
  end as vchLocalidad,          
  isnull(cli.vchApruebaMorosidad,'0') as vchApruebaMorosidad,      
  t3.vchDescripcion as vchEstadoDescripcion      
  FROM SGP_CLIENTE cli                      
   LEFT JOIN SGP_ZONA_VENTA  zna on (cli.vchZonaVenta = zna.vchZonaVenta)                
   LEFT JOIN SGP_DETALLE_TABLA t1 on (cli.vchTipPersona = t1.vchValor and t1.numCodigo = 5 and t1.chrSituacionRegistro = 'A')                
   LEFT JOIN SGP_DETALLE_TABLA t2 on (cli.vchTipDocumento = t2.vchValor and t2.numCodigo = 2 and t2.chrSituacionRegistro = 'A')        
   LEFT JOIN SGP_DETALLE_TABLA t3 on (cli.vchEstado = t3.vchValor and t3.numCodigo = 33 and t3.chrSituacionRegistro = 'A')          
   left join SGP_UBIGEO depa on (substring(cli.vchFacUbigeo, 1, 2 ) + '0000' = depa.vchCodUbigeo )                
   left join SGP_UBIGEO prov on (substring(cli.vchFacUbigeo, 1, 4 ) + '00' = prov.vchCodUbigeo )                
   left join SGP_UBIGEO dist on (cli.vchFacUbigeo = dist.vchCodUbigeo )               
  WHERE 1=1              
  AND cli.chrSituacionRegistro = 'A'                      
  AND cli.numCodCliente = @numCodCliente                      
                      
END 

GO

CREATE PROCEDURE [dbo].[pga_cliente_listar]      
@vchCodZonaVenta VARCHAR(10),                      
@vchTipoDocumento VARCHAR(4),                      
@vchNumDocumento VARCHAR(20),                      
@vchRazonSocial VARCHAR(300),                      
@vchCodigoBaan VARCHAR(6)                      
AS                      
DECLARE @SQL VARCHAR(MAX) = ''                      
BEGIN                      
                      
 SET NOCOUNT ON;                      
                       
 SET @SQL = 'SELECT TOP 500                       
     CAST(cli.numCodCliente as INT) as numCodCliente                      
    ,cli.vchCodRuta                       
    ,cli.vchNombres            
    ,cli.vchApellidos                      
    ,cli.vchRazonSocial            
    ,cli.vchTipPersona            
    ,cli.vchTipDocumento            
    ,cli.vchNumDocumento            
    ,cli.vchZonaVenta            
    ,cli.vchTipoCliente            
    ,cli.vchCodigoBaan                      
    ,cli.vchTelefono            
    ,cli.vchTelefax            
    ,cli.vchEmail            
 ,isnull(cli.chrFlgClienteFrecuente, ''N'') chrFlgClienteFrecuente              
 ,cli.chrFlgConvenio            
    ,cli.vchDirecFacturacion            
    ,cli.vchFacUbigeo            
    ,cli.vchDirecEntrega            
    ,cli.vchEntUbigeo            
    ,cli.vchEstado            
    ,cli.chrSituacionRegistro                      
    ,cli.vchCreador                      
    ,cli.dtmFechaCreacion                      
    ,cli.vchModificador                      
    ,cli.dtmFechaModificacion                      
    ,zna.vchDescripcion as vchZonaVentaDescripcion                  
    ,t1.vchDescripcion as vchTipoPersonaDescripcion            
    ,t2.vchDescripcion as vchTipoDocumentoDescripcion                
 ,vchRazonSocialNombre = RTRIM(LTRIM(ISNULL(cli.vchApellidos, '''') + '' '' + ISNULL(cli.vchNombres, '''') + '' '' + ISNULL(cli.vchRazonSocial, '''')))  
 ,CAST (                  
  CASE                   
   WHEN cli.chrFlgClienteFrecuente = ''S'' THEN 1                  
  ELSE  0                  
  END as bit) as bitClienteFrecuente             
    ,CASE                   
  WHEN ISNULL(cli.vchFacUbigeo, '''') <> '''' then                  
   prov.vchDescripcion + '' - '' + prov.vchDescripcion + '' - '' + dist.vchDescripcion                  
  ELSE                   
   ''''                  
   END  as vchLocalidad            
  ,isnull(cli.vchApruebaMorosidad,''0'') as vchApruebaMorosidad          
  ,t3.vchDescripcion as vchEstadoDescripcion      
  FROM SGP_CLIENTE cli                        
   LEFT JOIN SGP_ZONA_VENTA  zna on (cli.vchZonaVenta = zna.vchZonaVenta)                  
   LEFT JOIN SGP_DETALLE_TABLA t1 on (cli.vchTipPersona = t1.vchValor and t1.numCodigo = 5 and t1.chrSituacionRegistro = ''A'')                  
   LEFT JOIN SGP_DETALLE_TABLA t2 on (cli.vchTipDocumento = t2.vchValor and t2.numCodigo = 2 and t2.chrSituacionRegistro = ''A'')                  
   LEFT JOIN SGP_DETALLE_TABLA t3 on (cli.vchEstado = t3.vchValor and t3.numCodigo = 33 and t3.chrSituacionRegistro = ''A'')                  
   LEFT JOIN SGP_UBIGEO depa on (substring(cli.vchFacUbigeo, 1, 2 ) + ''0000'' = depa.vchCodUbigeo )                  
   LEFT JOIN SGP_UBIGEO prov on (substring(cli.vchFacUbigeo, 1, 4 ) + ''00'' = prov.vchCodUbigeo )                  
   LEFT JOIN SGP_UBIGEO dist on (cli.vchFacUbigeo = dist.vchCodUbigeo )                 
   WHERE cli.chrSituacionRegistro = ''A'' ' + CHAR(13) + CHAR(10);                      
                      
 IF ( ISNULL(@vchCodZonaVenta,'') <> '')                      
  SET @SQL +=  'AND cli.vchZonaVenta = ''' + @vchCodZonaVenta + ''' ';                      
                      
 IF ( ISNULL(@vchTipoDocumento,'') <> '')                      
  SET @SQL +=  'AND cli.vchTipDocumento = ''' + @vchTipoDocumento + ''' ';                      
                      
 IF ( ISNULL(@vchNumDocumento,'') <> '')                      
  SET @SQL +=  'AND cli.vchNumDocumento LIKE ''' + @vchNumDocumento + '%'' ';                      
                      
 IF ( ISNULL(@vchCodigoBaan,'') <> '')                      
  SET @SQL +=  'AND cli.vchCodigoBaan = ''' + @vchCodigoBaan + ''' ';                      
                      
 IF ( ISNULL(@vchRazonSocial,'') <> '')                      
  SET @SQL +=  'AND ( ' +                      
      '(cli.vchRazonSocial LIKE ''%' + @vchRazonSocial + '%'') OR ' +                      
      '(cli.vchNombres LIKE ''%' + @vchRazonSocial + '%'') OR ' +                   
      '(cli.vchApellidos LIKE ''%' + @vchRazonSocial + '%'') ' +                      
       ') '                      
                      
                      
 SET @SQL += ' ORDER BY cli.dtmFechaCreacion DESC ';                      
                      
 -- PRINT @SQL                      
                      
 EXEC(@SQL)                      
                      
                       
                      
 --TRUNCATE TABLE #Tabla                    
                      
 -- select len(@SQL)                      
 /*                      
 SELECT TOP 500                       
     cli.numCodCliente as CodCliente                      
    ,cli.vchCodRuta as CodRuta                      
    ,cli.vchNombres as Nombres                      
    ,cli.vchApellidos as Apellidos                      
    ,cli.vchRazonSocial as RazonSocial                      
    ,cli.vchTipPersona as TipoPersona                      
    ,cli.vchDirecFacturacion as DirecFacturacion                      
    ,cli.vchFacUbigeo as FacUbigeo                      
    ,cli.vchDirecEntrega as DirecEntrega                      
    ,cli.vchEntUbigeo as EntUbigeo                      
    ,cli.vchTipoCliente as TipoCliente                      
    ,cli.vchCodigoBaan as CodigoBaan                      
    ,cli.vchTelefono as Telefono                      
    ,cli.vchTelefax as Telefax                      
    ,cli.vchEmail as Email                      
    ,cli.vchTipDocumento as TipoDocumento                      
    ,cli.vchNumDocumento as NumeroDocumento                      
    ,cli.chrSituacionRegistro as SituacionRegistro                      
    ,cli.vchCreador as Creador                      
    ,cli.dtmFechaCreacion as FechaCreacion                      
    ,cli.vchModificador as Modificador                      
    ,cli.dtmFechaModificacion as FechaModificacion                      
    ,cli.vchZonaVenta as CodZonaVenta                      
    ,cli.vchEstado as Estado                      
    ,cli.numCodigoUsuario as CodigoUsuario                      
    ,cli.numCodClienteOperador as CodClientOperador                      
    ,cli.numCodClienteWeb as CodClienteWeb                      
    ,cli.chrFlgConvenio as FlgConvenio                      
    ,cli.chrFlgClienteFrecuente as FlgClienteFrecuente                      
    ,cli.numCodClienteCentral as CodClienteCentral                      
    ,cli.chrClienteEDP as ClienteEDP                      
    ,zna.vchDescripcion as ZonaVentaDescripcion                      
    ,t1.vchDescripcion as TipoPersonaDescripcion                      
    ,t2.vchDescripcion as TipoDocumentoDescripcion                      
    ,CASE                       
    WHEN cli.vchTipPersona = '2' THEN                      
     RTRIM(LTRIM(ISNULL(cli.vchRazonSocial,'')))                      
    ELSE                       
     LTRIM(RTRIM(ISNULL(cli.vchApellidos,'') + ' ' + ISNULL(cli.vchNombres,'')))                      
     END as RazonSocialNombre                      
     ,CAST (                      
   CASE                       
    WHEN cli.chrFlgClienteFrecuente = 'S' THEN                      
     1                      
    ELSE                      
     0                      
    END as bit) as ClienteFrecuente                      
   FROM SGP_CLIENTE cli                      
  LEFT JOIN SGP_ZONA_VENTA  zna on (cli.vchZonaVenta = zna.vchZonaVenta)                      
  LEFT JOIN SGP_DETALLE_TABLA t1 on (cli.vchTipPersona = t1.vchValor and t1.numCodigo = 5 and t1.chrSituacionRegistro = 'A')                      
  LEFT JOIN SGP_DETALLE_TABLA t2 on (cli.vchTipDocumento = t2.vchValor and t2.numCodigo = 2 and t2.chrSituacionRegistro = 'A')                      
 WHERE cli.chrSituacionRegistro = 'A'                      
 AND (zna.vchZonaVenta = @CodZonaVenta OR @CodZonaVenta IS NULL)                      
 AND (cli.vchTipDocumento = @TipoDocumento OR @TipoDocumento IS NULL)                      
 AND (cli.vchNumDocumento LIKE '%' + @NumDocumento + '%' OR @NumDocumento IS NULL)                      
 AND (cli.vchCodigoBaan = @CodigoBaan OR @CodigoBaan IS NULL)                      
 AND (CASE                       
    WHEN cli.vchTipPersona = '2' THEN                      
     RTRIM(LTRIM(ISNULL(cli.vchRazonSocial,'')))                      
    ELSE                       
     LTRIM(RTRIM(ISNULL(cli.vchApellidos,'') + ' ' + ISNULL(cli.vchNombres,'')))                      
   END LIKE '%' + @RazonSocial + '%'  OR @RazonSocial IS NULL)                      
                      
 ORDER BY cli.dtmFechaCreacion DESC                      
 */                      
                      
END 

GO

CREATE PROCEDURE [dbo].[pga_listar_categoria_articulos]                     
(              
@intCodCategoria int  ,              
@vchCodArticulo varchar(16) ,        
@vchCodTipoOrden varchar(3),                                          
@vchCodTarifaCanal varchar(3)       
)              
AS                        
BEGIN                        
                      
 SET NOCOUNT ON                        
                        
 SELECT     
 a.vchCodArticulo,     
 a.vchDescripcion,     
 a.vchCodIva,    
 b.vchCodTarifaCanal,     
 CAST(c.numPrecioUnidad as numeric(15,2) ) as numPrecioUnidad,    
 d.numValorIGV as 'vchValorIva',    
 b.intCodCategoria,     
 b.vchCodTipoOrden,     
 e.vchDescripcion as descripcionCategoria    
 FROM     
 SGP_ARTICULO a,     
 SGP_CATEGORIA_ARTICULO b  ,       
 SGP_TARIFA c,     
 SGP_DETALLE_IVA d  ,     
 SGP_CATEGORIA_VENTA e              
 WHERE       
 LTRIM(RTRIM(a.vchCodArticulo)) = LTRIM(RTRIM(b.vchCodArticulo))           
 AND ltrim(rtrim(a.vchCodArticulo)) = LTRIM(RTRIM(c.vchCodArticulo))                
 AND a.chrSituacionRegistro='A'  AND b.vchestado='A'                  
 AND  c.chrSituacionRegistro = 'A'                         
 AND GETDATE() BETWEEN c.dtmFecInicio AND DATEADD(dd,1, c.dtmFecCaducidad)                
 AND LTRIM(RTRIM(a.vchCodIva)) = LTRIM(RTRIM(d.vchCodIva))                 
 AND LTRIM(RTRIM(c.vchCodTarifaCanal)) = LTRIM(RTRIM(b.vchCodTarifaCanal))                 
 and ( b.intCodCategoria = @intCodCategoria  OR  @intCodCategoria = 0  )        
 AND (LTRIM(RTRIM(a.vchCodArticulo)) = @vchCodArticulo or @vchCodArticulo='' )            
 AND b.intCodCategoria = e.intCodCategoria AND e.vchestado='A'      
 AND (b.vchCodTipoOrden=@vchCodTipoOrden OR @vchCodTipoOrden='')     
 AND ( rtrim(ltrim(b.vchCodTarifaCanal)) = rtrim(ltrim(@vchCodTarifaCanal)) OR @vchCodTarifaCanal='')     
 ORDER BY     
 b.intCodCategoria,     
 a.vchCodArticulo                   
                          
END 

GO
CREATE PROCEDURE [dbo].[pga_obtener_descripcion_iva]        
 @vchCodIva VARCHAR(10)    
AS    
BEGIN    
 SELECT vchDescripcion FROM SGP_IVA WHERE ltrim(rtrim(vchCodIva)) = @vchCodIva    
END 

GO

CREATE PROCEDURE [dbo].[pga_obtener_impuesto_iva]        
@vchCodIva VARCHAR(10)                                 
AS                       
DECLARE @valor numeric(10,2)                      
BEGIN                                                 
  SELECT numValorIGV   
  FROM SGP_DETALLE_IVA   
  WHERE chrSituacionRegistro = 'A'   
  and ltrim(rtrim(vchCodIva)) = ltrim(rtrim(@vchCodIva))   
  and dtmFechaVigencia=(SELECT MAX(dtmFechaVigencia) FROM SGP_DETALLE_IVA WHERE chrSituacionRegistro = 'A' and ltrim(rtrim(vchCodIva)) = ltrim(rtrim(@vchCodIva)))      
END 
GO

CREATE PROCEDURE [dbo].[pga_cabecera_transaccion_hemeroteca_agregar]                
@vchTipoOrden varchar(3),                
@vchCodPuntoVenta varchar(4),                
@numCodCliente numeric,                
@intEstado int,                
@vchFormaPago varchar(3),                
@vchTipComprobante varchar(2),                
@numMontoBruto numeric(15,4),                
@numMontoDescuento numeric(15,4),                
@numMontoRecargo numeric(15,4),                
@numSubTotal numeric(15,4),                
@numMontoIGV numeric(15,4),                
@numMontoTotal numeric(15,4),                
@vchOrdenVenta varchar(32),                
@numTipoCambio numeric(9,4),                
@numCodComprobante numeric(9,4),                
@vchCodAlmacen varchar(8),                
@vchCodTarifaCanal varchar(20),                
@vchCodMoneda varchar(6),                
@dtmFechaVenta datetime,                
@vchDocReferencia varchar(128),                
@vchCodigoBaan varchar(6),                
@vchCodClienteBaan varchar(10),              
@vchTipLiquidacion varchar(2),             
@vchGuidRegistro varchar(100),             
@vchCreador varchar(16),                
@numCodTransaccion int output            
AS                  
BEGIN            
            
 SET NOCOUNT ON                  
                  
 DECLARE @numCurrentError int;          
     
  SELECT @vchTipLiquidacion = convert(varchar(1),isnull(intCodTipoLiquidacion ,'')  )    
  FROM SGP_TIPO_ORDEN       
  WHERE vchCodTipoOrden =  @vchTipoOrden     
    
   select @vchCodTarifaCanal = ltrim(RTRIM(@vchCodTarifaCanal))  
    
  if(len(@vchCodTarifaCanal)= 2)  
  begin  
     select @vchCodTarifaCanal = ' '+ltrim(RTRIM(@vchCodTarifaCanal))  
  end  
  else  
   begin  
     select @vchCodTarifaCanal = ltrim(RTRIM(@vchCodTarifaCanal))  
   end   
  
    
            
 INSERT INTO SGP_CABECERA_TRANSACCION_BS                  
 (                
  vchTipoOrden,                
  vchCodPuntoVenta,                
  numCodCliente,                
  intEstado,                
  vchFormaPago,                
  vchTipComprobante,                
  numMontoBruto,                
  numMontoDescuento,                
  numMontoRecargo,                
  numSubTotal,                
  numMontoIGV,                
  numMontoTotal,                
  vchOrdenVenta,                
  numTipoCambio,                
  numCodComprobante,                
  vchCodAlmacen,                
  vchCodTarifaCanal,                
  vchCodMoneda,                
  dtmFechaVenta,                
  vchDocReferencia,                
  vchCodigoBaan,              
  vchCodClienteBaan,           
            
  numCodTransaccionOperador,          
  chrFlgTransferido,          
  numOrdenVenta,          
          
  vchTipLiquidacion,              
  vchTipBase,          
  numCodAgencia,          
  chrFlagClienteAgencia,          
          
  chrSituacionRegistro,                
  vchCreador,                
  dtmFechaCreacion,                
  vchModificador,                
  dtmFechaModificacion,            
  vchGuidRegistro            
 )                  
 VALUES                  
 (                   
  @vchTipoOrden,                
  @vchCodPuntoVenta,                
  @numCodCliente,                
  @intEstado,                
  @vchFormaPago,                
  @vchTipComprobante,                
  @numMontoBruto,                
  @numMontoDescuento,                
  @numMontoRecargo,                
  @numSubTotal,                
  @numMontoIGV,                
  @numMontoTotal,                
  @vchOrdenVenta,                
  @numTipoCambio,                
  @numCodComprobante,                
  @vchCodAlmacen,                
  @vchCodTarifaCanal,                
  @vchCodMoneda,                
  @dtmFechaVenta,                
  @vchDocReferencia,                
  @vchCodigoBaan,              
  @vchCodClienteBaan,              
          
  NULL, -- numCodTransaccionOperador          
  NULL, -- chrFlgTransferido          
  NULL, -- numOrdenVenta          
          
  @vchTipLiquidacion,           
  '', -- vchTipBase          
  NULL, -- numCodAgencia          
  '0', -- chrFlagClienteAgencia          
          
  'I',      -- chrSituacionRegistro          
  @vchCreador,            
  GetDate(),                
  NULL,  -- vchModificador          
  NULL,  -- dtmFechaModificacion          
  @vchGuidRegistro            
 )                  
                  
 SET NOCOUNT OFF                  
                  
 SELECT @numCurrentError = @@Error                  
             
 IF @numCurrentError != 0                 BEGIN                  
  RETURN @numCurrentError                  
 END                  
 ELSE                  
 BEGIN                  
  SELECT @numCodTransaccion = SCOPE_IDENTITY()                  
  -- return 0 to indicate success, otherwise the raised error will be returned                  
  RETURN 0                  
 END            
            
END 

Go
CREATE PROCEDURE [dbo].[pga_mediopago_tipo_tarjeta_listar]        
AS        
BEGIN        
      
 SET NOCOUNT ON        
        
 SELECT         
  vchValor, vchDescripcion         
 FROM         
  SGP_DETALLE_TABLA        
 WHERE        
  numCodigo=114 and       
  chrSituacionRegistro = 'A' and       
  vchValor !=0;        
        
END;

GO

  
CREATE PROCEDURE [dbo].[pga_hemeroteca_comprobante_cancelar]          
(             
 @numCodTransaccion numeric,          
 @vchMedioPago varchar (2),          
 @vchCodBanco varchar(12),          
 @vchTipoTarjeta varchar(2),          
 @vchCreador varchar(16),          
 @vchNumDocumento varchar (40)  ,      
 @numPedidoQR    varchar(30),    
 @vchComprobante varchar(20) OUTPUT          
)                                
AS                              
                              
DECLARE @numCodMediPago numeric                                                 
DECLARE @vchCodMoneda varchar (3)                             
DECLARE @vchCodCuentaCorriente varchar(3)                                 
DECLARE @numTipoCambio numeric (9,4)                                                   
DECLARE @vchEstado varchar (2)                                 
DECLARE @numMonto numeric (15,4)                    
DECLARE @vchCodSucursal varchar (32)                               
DECLARE @dtmFecDeposito datetime                                      
DECLARE @vchCodCiudad varchar (32)                                 
DECLARE @chrSituacionRegistro char (1)                                                           
DECLARE @dtmFechaCreacion datetime                                     
DECLARE @vchModificador varchar (16)                               
DECLARE @dtmFechaModificacion datetime--                         
DECLARE @numCodComprobante numeric--                    
DECLARE @numBDMonto numeric (15,4)--          
DECLARE @vchReferencia varchar(80)--            
DECLARE @chrFlagPendienteAjuste varchar(1)--               
DECLARE @vchTipoCodigo varchar(3)--           
DECLARE @vchNumdocCodigo varchar(30) --          
DECLARE @numLinea int--              
DECLARE @numMontoOrigen numeric(15,4) --          
DECLARE @correlativoAnt numeric --                 
        
--Declare @vchComprobante varchar(20)          
Declare @dtmfechaemision datetime          
                            
BEGIN                
        
set @dtmfechaemision = getdate();        
exec pga_hemeroteca_emitir_comprobante @numCodTransaccion, @dtmfechaemision, '', @vchCreador, @vchComprobante  OUTPUT          
        
        
        
--@numCodTransaccion numeric(18),                                        
--@dtmFechaEmision datetime,                                    
--@vchObservaciones varchar(255),                                        
--@vchCreador varchar(20) ,                            
--@vchComprobante varchar(20) OUTPUT            
        
 SELECT          
  @numCodComprobante = numCodComprobante,          
  @vchCodMoneda = vchCodMoneda,          
  @numMonto = numMontoTotal,          
  @numMontoOrigen = numMontoTotal          
 FROM SGP_CABECERA_TRANSACCION_BS           
 WHERE numCodTransaccion = @numCodTransaccion;          
          
 IF @vchCodBanco = '' SELECT @vchCodBanco = NULL                            
 IF @chrSituacionRegistro IS NULL SELECT @chrSituacionRegistro = 'A'          
 IF @dtmFechaCreacion IS NULL SELECT @dtmFechaCreacion = GETDATE()          
 IF @numLinea IS NULL SELECT @numLinea = 0          
 IF @numTipoCambio IS NULL SELECT @numTipoCambio = 3.25          
 IF @vchTipoCodigo IS NULL SELECT @vchTipoCodigo = ''          
 IF @vchNumdocCodigo IS NULL SELECT @vchNumdocCodigo = ''          
 IF @chrFlagPendienteAjuste IS NULL SELECT @chrFlagPendienteAjuste = 'N'          
                         
 INSERT [SGP_MEDIO_PAGO_BS]                            
 (                                            
  [numCodTransaccion]          ,                            
  [vchCodBanco]                ,                            
  [vchMedioPago]               ,                            
  [vchCodMoneda]               ,                            
  [vchCodCuentaCorriente]      ,                            
  [numTipoCambio]              ,                            
  [vchNumDocumento]            ,                            
  [vchEstado]                  ,                            
  [numMonto]           ,                            
  [chrSituacionRegistro]       ,                            
  [vchCreador]                 ,                            
  [dtmFechaCreacion]           ,                            
  [vchModificador]             ,                            
  [dtmFechaModificacion]       ,                            
  [vchCodSucursal]  ,                            
  [dtmFecDeposito]             ,                            
  [vchCodCiudad]               ,            
  [vchTipoTarjeta]    ,            
  [numBDMonto]     ,          
  [vchReferencia]        ,            
  [chrFlagPendienteAjuste]     ,            
  [vchTipoCodigo]              ,            
  [vchNumdocCodigo]            ,            
  [numLinea]                   ,            
  [numMontoOrigen]             ,            
  [correlativoAnt]             ,    
  vchOrigen    
 )                             
 VALUES                            
 (                              
  @numCodTransaccion           ,                            
  @vchCodBanco                 ,                            
  @vchMedioPago                ,                   
  @vchCodMoneda     ,                 
  @vchCodCuentaCorriente       ,                
  @numTipoCambio               ,                            
  @vchNumDocumento             ,                            
  '2'                          ,                            
  @numMonto            ,             
  @chrSituacionRegistro        ,                            
  @vchCreador                  ,                            
  GETDATE()      ,                            
  @vchModificador              ,                            
  @dtmFechaModificacion        ,                            
  @vchCodSucursal              ,                            
  @dtmFecDeposito              ,                            
  @vchCodCiudad                ,            
  @vchTipoTarjeta              ,            
  @numBDMonto                  ,            
  @vchReferencia               ,            
  @chrFlagPendienteAjuste      ,            
  @vchTipoCodigo               ,            
  @vchNumdocCodigo             ,            
  @numLinea                    ,            
  @numMontoOrigen              ,            
  @correlativoAnt              ,    
  'PGA'    
 )                            
                                          
 SET @numCodMediPago = @@Identity                     
                  
 --UPDATE SGP_COMPROBANTE_BS            
 --SET vchFlagMedioPago='S' , chrSituacionRegistro ='A'        
 --WHERE numCodComprobante = @numCodComprobante            
          
 INSERT INTO SGP_MEDIO_PAGO_COMPROBANTE_BS            
 (              
  numCodComprobante,              
  numCodMediPago,              
  numMontoPago,              
  chrSituacionRegistro,              
  vchCreador,              
  dtmFechaCreacion,              
  vchModificador,              
  dtmFechaModificacion              
 )              
 VALUES              
 (              
  @numCodComprobante,              
  @numCodMediPago,              
  @numMonto,              
  @chrSituacionRegistro,              
  @vchCreador,              
  GetDate(),              
  @vchModificador,              
  GetDate()              
 )          
      
 Update SGP_COMPROBANTE_BS set vchFlagMedioPago ='S' where numCodComprobante = @numCodComprobante      
        
      
 select @vchComprobante        
        
END   

GO

  
CREATE PROCEDURE [dbo].[pga_hemeroteca_emitir_comprobante]                                    
@numCodTransaccion numeric(18),                                    
@dtmFechaEmision datetime,                                
@vchObservaciones varchar(255),                                    
@vchCreador varchar(20) ,                        
@vchComprobante varchar(20) OUTPUT                                
AS                                                                                                      
BEGIN                                                                                                      
                                                                                        
DECLARE @dtmFechaCreacion datetime;                                                                                              
DECLARE @chrSituacionRegistro char(1);                                                                                                      
DECLARE @vchOrigen varchar(10);                                                                                                      
DECLARE @vchCondicionPago varchar(1);                                                                                                      
DECLARE @vchMensajeError varchar(300);                                           
                                      
-- Datos de Cabecera                                                                                          
DECLARE                      
 @numCodCliente int,                      
 @numCodAgencia int,                
 @numCodClienteBaan int,                
 @vchCodPuntoVenta varchar(4),                      
 @vchFormaPago varchar(3),                      
 @vchTipDocumento varchar(2),                      
 @vchNumDocumento varchar(50),                      
 @vchTipComprobante varchar(1),                      
 @vchEstado varchar(20)                      
                                      
-- Datos de Linea                                                                                          
DECLARE                                                                                           
 --@intCodOrdenPublicacionLinea int,                                                                                                       
 --@intCodClasificacion int,                                                                                                       
 @vchUnidadMedida varchar(3),                                                                                                       
 --@numPalabra int,                                                                                                       
 --@numPagina int,                                                                                                       
 --@numAncho int,                                                                                                       
 --@numAlto int,                                                                                                       
 --@vchFechaPublicacion varchar(5000),                                                                                                      
 --@intLocalidad int,                                                                                               
 @vchCodArticulo varchar(16),                                            
 @vchCodTarifaCanal varchar(3),                                             
 --@intNumVeces int,                                                                         
 @numCantidad int,                                                               
 @numTarifa numeric(15,4),                                                          
 @numMontoBruto numeric(15,4),                                                     
 @numMontoRecargo numeric(15,4),                                                                                                    
 @numSubTotal numeric(15,4),                                               
 @numIGV numeric(15,4),                                                                
 @vchTituloAviso varchar(128);                                                      
                                           
DECLARE                                      
 @vchNomComprobante varchar(64),                                                    
 @vchDirComprobante varchar(128),                              
 @vchTipoDocumentoCliente varchar(2),                                                             
 @vchNumDocumentoCliente varchar(16);                                                                                      
                                                                                          
-- Variables                                                                               
DECLARE                                                       
 @numMontoIGV numeric(15,4),                                                                                          
 @numMontoTotal numeric(15,4),                                                                                          
 @numTotalNetoContado numeric(15,4),                                                                                          
 @numTotalNetoCredito numeric(15,4),                                                                                          
 @numMontoDescuento numeric(15,4),                                                                                          
 @numTipoCambio numeric(15,3),                      
 @numMontoPagado numeric(15,3);                                                                                          
                                                                                            
--DECLARE @newCodTransaccion int;                                                                                          
DECLARE @newCodComprobante int;                                                                                          
DECLARE @numPorcentajeIGV numeric(15,2);                                                                                                
DECLARE @numPorcentajeRecargo numeric(15,2);                                                                                     
                                                                              
DECLARE @vchTipoOrden varchar(3);                                                                                           
DECLARE @numLinea INT;                                                                                          
DECLARE @vchFlag varchar(1);                                                               
                                                        
DECLARE @vchNumSerie varchar(4);                                                        
DECLARE @vchNumComprobante varchar(7);                                                        
DECLARE @vchNumComprobanteManual varchar(7);                                                      
DECLARE @chrFlagComprobanteManual varchar(1);                                                      
                                                                                     
                                                 
--DECLARE @vchFecPublicacion varchar(1000);                                                                          
DECLARE @numDias int;                                       
DECLARE @numMontoBruto2 numeric(18,4);                                    
DECLARE @vchNomArticulo varchar(250);                                    
                                  
DECLARE @vchCorrelativo varchar(10);                                  
DECLARE @vchZonaVenta varchar(100);                      
                      
SET @numTipoCambio = 3.25;                                                                                    
SET @numPorcentajeIGV = 0.18;                 
SET @vchTipoOrden = 'V01';                                                                                        
SET @numMontoDescuento = 0;                                                                                  
SET @numPorcentajeRecargo = 0.2;        
                                                                                    
 -- SET NOCOUNT ON added to prevent extra result sets from                                                                                                      
 -- interfering with SELECT statements.                                                  
 SET NOCOUNT ON;                                       
                                      
 SET @dtmFechaCreacion = GETDATE();                                                         
 SET @chrSituacionRegistro = 'A';                                                     
 SET @vchOrigen = 'PGABBOO';                                                                                                      
 SET @vchCondicionPago = '1'; -- PAGADO                                                                            
 --SET @vchTipDispositivo = '26'; -- BBOO                          
                                                                       
 UPDATE SGP_DETALLE_TRANSACCION_BS                      
 SET SGP_DETALLE_TRANSACCION_BS.vchUnidadMedida = SGP_ARTICULO.vchUnidadMedida                      
 FROM SGP_DETALLE_TRANSACCION_BS, SGP_ARTICULO                      
 WHERE SGP_DETALLE_TRANSACCION_BS.numCodTransaccion = @numCodTransaccion                      
 AND SGP_DETALLE_TRANSACCION_BS.vchCodArticulo = SGP_ARTICULO.vchCodArticulo                      
 AND SGP_ARTICULO.chrSituacionRegistro = 'A';                      
                                                                                                      
 SELECT                                     
  @numCodCliente  = ct.numCodCliente,                                
  @numCodAgencia = numCodAgencia,                                
  @vchCodPuntoVenta = vchCodPuntoVenta,                                    
  @vchTipComprobante = vchTipComprobante,                                    
  @vchFormaPago = vchFormaPago,                                    
  @vchTipoOrden = vchTipoOrden,                
  @numCodClienteBaan = cli.vchCodigoBaan                
 FROM SGP_CABECERA_TRANSACCION_BS ct                
 LEFT JOIN SGP_CLIENTE cli ON cli.numCodCliente = ct.numCodCliente                 
 WHERE numCodTransaccion = @numCodTransaccion;                                
                 
IF EXISTS (SELECT 1 FROM SGP_CLIENTE WHERE vchCodigoBaan = CONVERT(varchar(20),@numCodClienteBaan))                
BEGIN                
 UPDATE SGP_CABECERA_TRANSACCION_BS                
 SET vchCodClienteBaan = @numCodClienteBaan                
 WHERE numCodTransaccion = @numCodTransaccion;                
END                
                
 SELECT @vchZonaVenta = T1.vchZonaVenta + '-' + T2.vchDescripcion                      
 FROM SGP_PUNTO_VENTA T1,  SGP_ZONA_VENTA T2                      
 WHERE T1.vchCodPuntoVenta = @vchCodPuntoVenta                      
 AND T1.vchZonaVenta = T2.vchZonaVenta;                      
                      
 IF LEN(@vchZonaVenta) > 50                      
 SET @vchZonaVenta = SUBSTRING(@vchZonaVenta, 0, 50);                      
                      
                      
 SELECT                                                                 
  @vchNomComprobante = CASE                                                                                                              
  WHEN t1.vchTipPersona = '1' THEN                                                                  
  LTRIM(RTRIM(ISNULL(t1.vchApellidos,'') + ' ' + ISNULL(t1.vchNombres,'') ))                                                      
  ELSE                                                                                                              
  LTRIM(RTRIM(ISNULL(t1.vchRazonSocial, '')))                                                   
  END,                                                                                                              
  @vchDirComprobante = RTRIM(ISNULL(t1.vchDirecFacturacion,'')),                                     
  @vchTipoDocumentoCliente = t1.vchTipDocumento,                                                                                                              
  @vchNumDocumentoCliente = t1.vchNumDocumento                                                                                                              
 FROM SGP_CLIENTE t1                                                                                                              
  LEFT JOIN SGP_UBIGEO t2 on (t1.vchFacUbigeo = t2.vchCodUbigeo )                                                                                                                
  LEFT JOIN SGP_UBIGEO t3 on (SUBSTRING(t1.vchFacUbigeo, 1,2) + '0000' = t3.vchCodUbigeo )                                                                                                              
  LEFT JOIN SGP_UBIGEO t4 on (SUBSTRING(t1.vchFacUbigeo, 1,4) + '00' = t4.vchCodUbigeo )                                                                             
 WHERE t1.numCodCliente = @numCodCliente;                                       
                                 
 SELECT                                      
  @numMontoBruto = sum(numSubTotal),                                      
  @numMontoDescuento = 0,                        
  @numMontoRecargo = 0,                                   
  @numSubTotal  = sum(numSubTotal - numIGV),                                      
  @numIGV = sum(numIGV),                                 
  @numMontoPagado = 0                      
 FROM SGP_DETALLE_TRANSACCION_BS                                  
 WHERE numCodTransaccion = @numCodTransaccion;                                     
                            
 SET @vchNumSerie = @vchTipComprobante + RTRIM(LTRIM(@vchCodPuntoVenta));                                    
                                    
  -------------                                  
  -- Correlativo                                  
  -------------                                  
 DECLARE @numCorrelativo int;                                    
                                  
 SELECT @vchCorrelativo = ISNULL(vchCorrelativo, '1' )                                          
 FROM SGP_IMPRESORA                                          
 WHERE vchNumSerie = @vchNumSerie;                                     
                                  
 SET @numCorrelativo = CONVERT(int, @vchCorrelativo);                                       
                                  
 UPDATE SGP_IMPRESORA                                          
 SET vchCorrelativo = CONVERT(VARCHAR(7), @numCorrelativo + 1)                                          
 WHERE vchNumSerie = @vchNumSerie;                          
                                  
 SET @vchNumComprobante = Right('0000000'+ CONVERT(VARCHAR(7), @numCorrelativo), 7);                                     
  ------------                                  
 INSERT INTO SGP_COMPROBANTE_BS (                             
  numCodCliente,                             
  vchCodPuntoVenta,                            
  vchZonaVenta,                            
  numCodAgencia ,                             
  vchNomComprobante,                             
  vchDirComprobante,                             
  vchFormaPago,                             
  vchTipoComprobante,                             
  vchNumSerie,                             
  vchNumComprobante,                                    
  dtmFecEmision ,                            
  chrFlagFacturado ,                             
  chrFlagMigrado ,                                    
  numMontoBruto ,                             
  numMontoDescuento,                             
  numMontoRecargo ,                             
  numSubTotal  ,                             
  numMontoIGV  ,                             
  numMontoTotal  ,                             
  numMontoPagado,                                    
  vchEstado     ,                             
  vchObservaciones ,                             
                              
  vchTipoDocumento ,                             
  vchNumDocumento ,                             
  vchCodOPublicacion, vchCodTransaccion ,                       
  vchOrdenVenta,                      
  numTipoCambio,                        
  chrFlagClienteAgencia,                      
  chrSituacionRegistro, vchCreador , dtmFechaCreacion                 
 )                                                                                                
 VALUES(                                                                           
  @numCodCliente,                                
  @vchCodPuntoVenta,                            
  @vchZonaVenta,                            
  @numCodAgencia,                                
  @vchNomComprobante, -- vchNomComprobante                                                                                                    
  @vchDirComprobante, -- vchDirComprobante                                                                                          
  @vchFormaPago,                                                                                           
  @vchTipComprobante,                                                                                                    
  @vchNumSerie, -- vchNumSerie                                                                              
  @vchNumComprobante, -- vchNumComprobante                                                                                                    
                                                                                                    
  @dtmFechaEmision, -- dtmFecEmision                                                                                      
  'S', -- chrFlagFacturado                                                                    
  'N', -- chrFlagMigrado                                                                                     
                                                                                   
  @numMontoBruto, -- numMontoBruto                                               
  @numMontoDescuento, -- numMontoDescuento                                                                                                    
  @numMontoRecargo, -- numMontoRecargo                                                                                           
  @numSubTotal, -- numSubTotal                                                                                                    
  @numIGV, -- numMontoIGV                                                                                                    
  @numSubTotal + @numIGV, -- numMontoTotal                                                              
  @numMontoPagado, -- numMontoPagado                                                                                       
                                                                           
  '2', -- vchEstado CANCELADO                                                                                     
  @vchObservaciones,  -- vchObservaciones                                                                                                    
  @vchTipoDocumentoCliente, -- vchTipoDocumento                                                                                                 
  @vchNumDocumentoCliente, -- vchNumDocumento                                                                                                    
  NULL, -- CONVERT(varchar(10), @newCodOPublicacion), -- vchCodOPublicacion                                                            
  CONVERT(varchar(10), @numCodTransaccion), -- vchCodTransaccion                                        
  0, -- vchOrdenVenta                      
  1, -- numTipoCambio                           
  '0', -- chrFlagClienteAgencia                      
  --'SOL', -- vchCodMoneda                                                 
  --NULL, -- @intTipoTablaG, -- intTipoTablaG                                                                                 
  --NULL, -- @intTipoBaseLegalG, -- intTipoBaseLegalG                                                                     
  --ISNULL(@vchFlag, 'N'), -- vchFlag                                                                                                     
                                                                                  
  @chrSituacionRegistro,                                                      
  @vchCreador,                                                                                               
  @dtmFechaCreacion                                                                                           
                                                                                                    
 );                                                
                                    
 -- ------------------------                           
 SET @newCodComprobante = SCOPE_IDENTITY();                                                                                                
                    
 DECLARE cr_detalle cursor for                                                                                                 
    SELECT                                                                                                 
     TRX.numLinTransaccion,                                    
     TRX.vchCodArticulo,                                    
     --TRX.numDias,                                    
     TRX.numCantidad,                                
     TRX.numTarifa,                                    
     TRX.numMontoBruto,                                    
     --TRX.numMontoDescuento,                                    
     --TRX.numMontoRecargo,                                    
     TRX.numSubTotal,                                    
     TRX.numIGV,                                    
     TRX.vchUnidadMedida,                                    
     --TRX.numPagina,                                    
     --TRX.numPalabra,                                    
     --TRX.numAncho,                                    
     --TRX.numAlto,                                
     --TRX.vchTituloAviso,                                    
     ART.vchDescripcion vchNomArticulo                                    
    FROM SGP_DETALLE_TRANSACCION_BS TRX                                    
  LEFT JOIN SGP_ARTICULO ART ON ( RTRIM(Ltrim(TRX.vchCodArticulo)) = RTRIM(Ltrim(ART.vchCodArticulo)))                                    
    WHERE TRX.numCodTransaccion = @numCodTransaccion;                                    
                                                                              
 OPEN cr_detalle                                      
 FETCH NEXT FROM cr_detalle INTO                                                                        
  @numLinea,                                                                                         
  @vchCodArticulo,                                                                                        
  --@numDias,                                                                                    
  @numCantidad,                                                                                         
  @numTarifa,                                                                                             
  @numMontoBruto,                                                                                             
  --@numMontoDescuento,                                     
  --@numMontoRecargo,                                                                                          
  @numSubTotal,                                                         
  @numIGV,                                                                                                     
  @vchUnidadMedida,                                                                                             
  -- @numPagina,                                       
  --@numPalabra,                                                                                                 
  --@numAncho,                             
  --@numAlto,                                                                                  
  --@vchTituloAviso,                                    
  @vchNomArticulo                                    
 WHILE @@fetch_status = 0                                                                                      
 BEGIN                            
  -- ----------------                                   
  SET @numMontoBruto2 = @numSubTotal + @numMontoRecargo - @numMontoDescuento;                                                                              
                                      
  INSERT INTO SGP_DETALLE_COMPROBANTE_BS  (                                   
     numCodComprobante,                                                                                                           
     numItem,                                                
     vchCodArticulo,                                                   
     vchNomArticulo,                                                                                                          
     vchTituloAviso,                                 
     numVeces,                                                                                                          
                                                                                                    
     numCantidad,                                                                                                          
     numPrecioUnitario,                                                                                               
     numMontoBruto,                                                                                               
     numMontoDescuento,                                                         
     numMontoRecargo,                                                                                     
     numSubTotal,                                                                                                        
     numIGV,                                                                                   
                                                                                                    
     vchUnidadMedida,                                                                                                                
     numPagina,                                                                                     
     numPalabra,                                                                                                          
     numAncho,                                                                                                          
     numAlto,                        
  numEjemplar,                      
  numDia,                      
     vchFecPublicacion,                                                                                         
                                                                     
     chrSituacionRegistro,                                                                                                    
     vchCreador,                                                                                     
     dtmFechaCreacion                                        
  )                                                                                       
  VALUES                                                                                            
  (                          
   @newCodComprobante,                                                                                            
   @numLinea,                                      
   @vchCodArticulo,                                                                                             
   @vchNomArticulo,                                          
   NULL, -- vchTituloAviso,                                                                                        
   0, --  numVeces         
                                                                            
   @numCantidad,                                                                                             
   @numTarifa,                                                                                             
   @numMontoBruto2,     -- numMontoBruto                                                                                       
   @numMontoDescuento,                                                                                             
   @numMontoRecargo,                                                                                             
   @numSubTotal,  -- numSubTotal                                       
   @numIGV,                                                                           
   @vchUnidadMedida,                            
   0, -- numPagina,                                                                                                 
   0, -- numPalabra,                                                                                                 
0, -- numAncho,                                                                                                 
   0, -- numAlto,                           
   0, -- numEjemplar                      
   0, -- numDia                      
   NULL,  -- vchFecPublicacion,                                                                
                                                                
   @chrSituacionRegistro,                                                                                               
   @vchCreador,                                                                                               
   @dtmFechaCreacion                                                                         
  );                                                                                            
                                                                          
  INSERT INTO SGP_MOVIMIENTO_TRANSACCION_BS  (                                                                                   
   numCodComprobante, numItem, numCodTransaccion, numLinTransaccion,                                                                                        
   chrSituacionRegistro, vchCreador, dtmFechaCreacion                                                                                        
  )                                                                                               
  VALUES (                                                                                            
   @newCodComprobante, @numLinea, @numCodTransaccion, @numLinea,                 
   @chrSituacionRegistro, @vchCreador, @dtmFechaCreacion                                                                                                    
  )                                                                   
              
  INSERT INTO PGA_TRANSACCION_KARDEX (            
 CodArticulo, CodTarifaCanal, CodPuntoVenta, TipoTransaccion, FechaTransaccion, Cantidad, CodTransaccion, Descripcion,             
 Estado, Proceso, GuidRegistro, SituacionRegistro, Creador, FechaCreacion)            
  VALUES (            
 @vchCodArticulo, @vchCodTarifaCanal, @vchCodPuntoVenta, 'VENTA',           
 DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0), -1*@numCantidad, @numCodTransaccion,           
 'Salida por venta ' + @vchNumSerie + '-' + @vchNumComprobante,             
 '', '',  NEWID(), @chrSituacionRegistro, @vchCreador, @dtmFechaCreacion)            
                 
   ---------------                                                                                            
   FETCH NEXT FROM cr_detalle INTO                                                                                             
   @numLinea,                                                                                                 
   @vchCodArticulo,                                                                              
   --@numDias,                               
   @numCantidad,                                            
   @numTarifa,                                                                                             
   @numMontoBruto,                                                                                             
   --@numMontoDescuento,                                                                                           
   --@numMontoRecargo,                                                                                
   @numSubTotal,                                                                                             
   @numIGV,                                                                                                     
   @vchUnidadMedida,                                                                         
   --@numPagina,                                                                                                 
   --@numPalabra,                                           
   --@numAncho,                                                                                                 
   --@numAlto,                                                                                        
   --@vchTituloAviso,                                    
 @vchNomArticulo                                    
   END                                                                                                      
   CLOSE cr_detalle                                                                                  
   DEALLOCATE cr_detalle                                               
                                    
 -- ------------------------                                    
                                                      
 UPDATE SGP_CABECERA_TRANSACCION_BS                    
 SET numCodComprobante = @newCodComprobante  , chrSituacionRegistro='A'                  
 WHERE numCodTransaccion = @numCodTransaccion;                    
                                                                                        
                                    
 SELECT @vchComprobante =  @vchNumSerie + '-'+ @vchNumComprobante;                                
                                                                 
                                               
 -- ----------------------------------------                                               
                          
END  

GO

CREATE PROCEDURE [dbo].[pga_bienservicio_orden_pago_agregar]        
@numCodTransaccion numeric(15,0),        
@vchCreador varchar(50)        
AS        
BEGIN        
 -- SET NOCOUNT ON added to prevent extra result sets from        
 -- interfering with SELECT statements.        
 SET NOCOUNT ON;        
        
 DECLARE @intCodOrdenPagoDigital INT;        
 DECLARE @intCodOrdenPublicacion INT;        
 DECLARE @vchOrigen VARCHAR(100);        
 DECLARE @numCodComprobante NUMERIC(18,0);        
 DECLARE @vchTipDocumento VARCHAR(1);        
 DECLARE @vchNumDocumento VARCHAR(20);        
 DECLARE @vchRazonSocial VARCHAR(300);        
 DECLARE @numCodCliente NUMERIC(18,0);        
        
 DECLARE @vchCodClienteErp varchar(20);        
 DECLARE @vchEmail varchar(120);        
 DECLARE @vchTelefono varchar(20);        
 DECLARE @vchCodMoneda varchar(3);        
 DECLARE @numImporteTotal decimal(18,4);        
 DECLARE @dtmFechaVencimiento datetime;        
 DECLARE @vchHoraVencimiento varchar(10);        
 DECLARE @vchConcepto varchar(1000);        
 DECLARE @vchCodPuntoVenta varchar(4);        
 DECLARE @vchEstado VARCHAR(100);        
 DECLARE @vchGuidRegistro VARCHAR(100);        
 DECLARE @numImporteRedondeo decimal(8,4);        
 DECLARE @vchNumeroPedido VARCHAR(20);            
        
 SET @intCodOrdenPublicacion = 0;        
 SET @vchOrigen = 'SGP_CABECERA_TRANSACCION_BS';        
 SET @dtmFechaVencimiento = CAST(GETDATE() as Date);        
 SET @vchHoraVencimiento = '23:59';        
 SET @vchConcepto = 'Orden de pago por la transacción ' + CONVERT(VARCHAR(10), @numCodTransaccion);        
 SET @vchGuidRegistro = CONVERT(VARCHAR(100), NEWID());        
        
    -- Insert statements for procedure here        
 SELECT         
  @numCodCliente = TRX.numCodCliente,        
  @vchCodClienteErp = CLI.vchCodigoBaan,        
  @vchCodPuntoVenta = TRX.vchCodPuntoVenta,        
  @vchTipDocumento = CLI.vchTipDocumento,        
  @vchNumDocumento = CLI.vchNumDocumento,        
  @vchRazonSocial = RTRIM(LTRIM(RTRIM(LTRIM(ISNULL(CLI.vchApellidos, ''))) + ' ' + RTRIM(LTRIM(ISNULL(CLI.vchNombres, ''))) + ' ' + RTRIM(LTRIM(ISNULL(CLI.vchRazonSocial, ''))))),        
  @vchEmail = CLI.vchEmail,        
  @vchTelefono = CLI.vchTelefax,        
  @vchCodMoneda = TRX.vchCodMoneda,        
  @numImporteTotal = TRX.numMontoTotal        
 FROM SGP_CABECERA_TRANSACCION_BS TRX        
  LEFT JOIN SGP_CLIENTE CLI ON (TRX.numCodCliente = CLI.numCodCliente)        
 WHERE TRX.numCodTransaccion = @numCodTransaccion;        
        
 SET @numImporteRedondeo = @numImporteTotal - ROUND(@numImporteTotal,2);        
        
 SET @vchNumeroPedido = '50' + REPLACE(STR(CONVERT( VARCHAR(20), @numCodTransaccion), 8), SPACE(1), '0')     
      
 INSERT INTO PGA_ORDEN_PAGO_DIGITAL        
           ([intCodOrdenPublicacion]        
           ,[vchOrigen]        
           ,[vchNumTransaccionOrigen]        
           ,[numCodOPublicacion]        
           ,[numCodTransaccion]        
           ,[numCodComprobante]        
           ,[vchTipDocumento]        
           ,[vchNumDocumento]        
           ,[vchRazonSocial]        
           ,[numCodCliente]        
           ,[vchCodClienteErp]        
           ,[vchEmail]        
           ,[vchTelefono]        
           ,[vchCodMoneda]        
           ,[numImporteTotal]        
           ,[dtmFechaVencimiento]        
           ,[vchHoraVencimiento]        
           ,[vchConcepto]        
           ,[vchArchivoAnexo]        
           ,[vchArchivoAnexoOriginal]        
           ,[vchCodPuntoVenta]        
           ,[vchTipoMedioPago]        
           ,[vchPagoLink]        
           ,[vchOrderIdExterno]        
           ,[vchEstado]        
           ,[vchGuidRegistro]        
           ,[chrSituacionRegistro]        
           ,[vchCreador]        
           ,[dtmFechaCreacion]        
           ,[vchModificador]        
           ,[dtmFechaModificacion]        
           ,[numImporteRedondeo]    
     ,vchNumeroPedido)        
     VALUES        
       (@intCodOrdenPublicacion        
           ,@vchOrigen        
           ,@numCodTransaccion -- vchNumTransaccionOrigen        
           ,0  -- <numCodOPublicacion, decimal(18,0),>        
           ,@numCodTransaccion --<numCodTransaccion, decimal(18,0),>        
           ,0 -- <numCodComprobante, decimal(18,0),>        
           ,@vchTipDocumento        
           ,@vchNumDocumento        
           ,@vchRazonSocial        
           ,@numCodCliente        
           ,@vchCodClienteErp        
           ,@vchEmail        
           ,@vchTelefono        
           ,@vchCodMoneda        
           ,@numImporteTotal        
           ,@dtmFechaVencimiento        
           ,@vchHoraVencimiento        
           ,@vchConcepto        
           ,null -- vchArchivoAnexo, varchar(100),>        
           ,null --<vchArchivoAnexoOriginal, varchar(255),>        
           ,@vchCodPuntoVenta        
           ,null --vchTipoMedioPago, varchar(20),>        
           ,null -- <vchPagoLink, varchar(255),>        
           ,null --<vchOrderIdExterno, varchar(100),>        
           ,@vchEstado --, varchar(100),>        
           ,@vchGuidRegistro         
           ,'A'        
           ,@vchCreador        
           ,GETDATE()        
           ,NULL        
           ,NULL        
           ,@numImporteRedondeo        
     ,@vchNumeroPedido    
     )        
        
        
            
END 

GO
CREATE PROCEDURE [dbo].[pga_bienservicio_comprobante_listar_por_numero_transaccion]             
@numCodTransaccion numeric(18,0)                
AS                
BEGIN                
 -- SET NOCOUNT ON added to prevent extra result sets from                
 -- interfering with SELECT statements.                
 SET NOCOUNT ON;                
                
    -- Insert statements for procedure here                
 SELECT                 
     com.numCodComprobante                
    ,com.numCodCliente                
    ,com.vchCodPuntoVenta                
    ,com.numCodAgencia                
    ,com.vchNomComprobante                
    ,com.vchDirComprobante                
    ,com.vchFormaPago                
    ,com.vchTipoComprobante                
    ,com.vchNumSerie                
    ,com.vchNumComprobante                
    ,com.dtmFecEmision                
    ,com.chrFlagFacturado                
    ,com.chrFlagMigrado                
    ,com.numMontoBruto                
    ,com.numMontoDescuento                
    ,com.numMontoRecargo                
    ,com.numSubTotal                
    ,com.numMontoIGV                
    ,com.numMontoTotal                
    ,com.numMontoPagado                
    ,com.vchEstado                
    ,com.vchObservaciones                
    ,com.chrSituacionRegistro                
    ,com.vchCreador                
    ,com.dtmFechaCreacion                
    ,com.vchModificador                
    ,com.dtmFechaModificacion                
    ,com.vchTipoDocumento                
    ,com.vchNumDocumento                
    ,com.vchMotivoAnulacion                
    ,com.vchCodOPublicacion                
    ,com.vchCodTransaccion                
    ,com.vchNumDocPublicacion                
    ,com.vchOrdenVenta                
    ,com.numTipoCambio                     
    ,com.vchFlagMedioPago                    
    ,com.vchFlagImpreso                      
    ,dt1.vchDescripcion vchDescEstado                
   FROM SGP_COMPROBANTE_BS com                
   LEFT JOIN SGP_DETALLE_TABLA dt1 ON (dt1.numCodigo = 25 AND com.vchEstado = dt1.vchValor and dt1.chrSituacionRegistro = 'A')                
   WHERE com.vchCodTransaccion = CONVERT(varchar(20),@numCodTransaccion)               
   AND com.chrSituacionRegistro = 'A'                
   ORDER BY com.vchNumSerie, com.vchNumComprobante;                 
END   

GO
  
CREATE PROCEDURE [dbo].[pga_hemeroteca_detalle_comprobante_listar_por_cod_comprobante]  
(             
 @numCodComprobante numeric              
)         
AS  
BEGIN    
  
 SET NOCOUNT ON         
  
 select numItem, vchCodArticulo, vchNomArticulo, numCantidad, numPrecioUnitario, numSubTotal    
 from SGP_DETALLE_COMPROBANTE_BS   
 where numCodComprobante = @numCodComprobante  and chrSituacionRegistro='A'  
  
END  

GO

CREATE PROCEDURE [dbo].[pga_bienservicio_comprobante_obtener_por_clave]        
@numCodComprobante numeric(15,4)        
AS        
BEGIN        
 -- SET NOCOUNT ON added to prevent extra result sets from        
 -- interfering with SELECT statements.        
 SET NOCOUNT ON;        
        
    -- Insert statements for procedure here        
 SELECT         
     com.numCodComprobante        
    ,com.numCodCliente        
    ,com.vchCodPuntoVenta    
    ,com.numCodAgencia        
    ,com.vchNomComprobante        
    ,com.vchDirComprobante        
    ,com.vchFormaPago        
    ,com.vchTipoComprobante        
    ,com.vchNumSerie        
    ,com.vchNumComprobante        
    ,com.dtmFecEmision        
    ,com.chrFlagFacturado        
    ,com.chrFlagMigrado        
    ,com.numMontoBruto        
    ,com.numMontoDescuento        
    ,com.numMontoRecargo        
    ,com.numSubTotal        
    ,com.numMontoIGV        
    ,com.numMontoTotal        
    ,com.numMontoPagado        
    ,com.vchEstado        
    ,com.vchObservaciones        
    ,com.chrSituacionRegistro        
    ,com.vchCreador        
    ,com.dtmFechaCreacion        
    ,com.vchModificador        
    ,com.dtmFechaModificacion        
    ,com.vchTipoDocumento        
    ,com.vchNumDocumento        
    ,com.vchMotivoAnulacion        
    ,com.vchCodOPublicacion        
    ,com.vchCodTransaccion        
    ,com.vchNumDocPublicacion        
    ,com.vchOrdenVenta        
    ,com.numTipoCambio          
    ,com.vchCodTransaccion        
    ,com.vchFlagMedioPago        
    ,com.vchObservaciones        
    ,com.vchFlagImpreso        
 ,dt1.vchDescripcion vchDescEstado    
   FROM SGP_COMPROBANTE_BS com        
   LEFT JOIN SGP_DETALLE_TABLA dt1 ON (dt1.numCodigo = 25 AND com.vchEstado = dt1.vchValor and dt1.chrSituacionRegistro = 'A')        
   WHERE com.numCodComprobante = @numCodComprobante        
   AND com.chrSituacionRegistro = 'A'        
   ORDER BY com.vchNumSerie, com.vchNumComprobante;        
END 

GO