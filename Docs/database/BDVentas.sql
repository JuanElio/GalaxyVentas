USE [BDVenta]
GO
/****** Objeto: Table [dbo].[PGA_LOG] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[SGP_RUTA](
	[vchCodRuta] [varchar](6) NOT NULL,
	[vchDescripcion] [varchar](30) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchCodPuntoVenta] [varchar](4) NULL,
PRIMARY KEY CLUSTERED 
(
	[vchCodRuta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_TABLA] Fecha de script: 27/10/2026 09:51:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_TABLA](
	[numCodigo] [int] NOT NULL,
	[vchNombre] [varchar](64) NOT NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NULL,
	[dtmFechaCreacion] [datetime] NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[numCodigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SGP_RUTA] ADD  CONSTRAINT [DF__SGP_RUTA__chrSit__3AA1AEB8]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_RUTA] ADD  CONSTRAINT [DF__SGP_RUTA__dtmFec__3B95D2F1]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_TABLA] ADD  CONSTRAINT [DF__SGP_TABLA__chrSi__075714DC]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO

GO
CREATE TABLE [dbo].[PGA_LOG](
	[IdLog] [int] IDENTITY(1,1) NOT NULL,
	[Proceso] [varchar](300) NULL,
	[Descripcion] [nvarchar](max) NULL,
	[Nivel] [varchar](100) NULL,
	[NombreMaquina] [varchar](100) NULL,
	[Logger] [varchar](300) NULL,
	[CallSite] [nvarchar](max) NULL,
	[Exception] [nvarchar](max) NULL,
	[FlagNotificacion] [char](1) NULL,
	[EmailNotificacion] [varchar](300) NULL,
	[Aplicacion] [varchar](100) NULL,
	[Tabla] [varchar](300) NULL,
	[IdTabla] [int] NULL,
	[Creador] [varchar](100) NULL,
	[FechaCreacion] [datetime] NULL,
 CONSTRAINT [PK_PGA_LOG] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PGA_ORDEN_PAGO_DIGITAL] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PGA_ORDEN_PAGO_DIGITAL](
	[intCodOrdenPagoDigital] [int] IDENTITY(1,1) NOT NULL,
	[intCodOrdenPublicacion] [int] NULL,
	[vchOrigen] [varchar](50) NULL,
	[vchNumTransaccionOrigen] [varchar](50) NULL,
	[numCodOPublicacion] [decimal](18, 0) NULL,
	[numCodTransaccion] [decimal](18, 0) NULL,
	[numCodComprobante] [decimal](18, 0) NULL,
	[vchTipDocumento] [varchar](1) NULL,
	[vchNumDocumento] [varchar](20) NULL,
	[vchRazonSocial] [varchar](300) NULL,
	[numCodCliente] [int] NULL,
	[vchCodClienteErp] [varchar](20) NULL,
	[vchEmail] [varchar](120) NULL,
	[vchTelefono] [varchar](20) NULL,
	[vchCodMoneda] [varchar](3) NULL,
	[numImporteTotal] [decimal](18, 4) NULL,
	[dtmFechaVencimiento] [datetime] NULL,
	[vchHoraVencimiento] [varchar](10) NULL,
	[vchConcepto] [varchar](1000) NULL,
	[vchArchivoAnexo] [varchar](100) NULL,
	[vchArchivoAnexoOriginal] [varchar](255) NULL,
	[vchCodPuntoVenta] [varchar](4) NULL,
	[vchTipoMedioPago] [varchar](20) NULL,
	[vchPagoLink] [varchar](255) NULL,
	[vchOrderIdExterno] [varchar](100) NULL,
	[vchEstado] [varchar](100) NULL,
	[vchGuidRegistro] [varchar](100) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](100) NULL,
	[dtmFechaCreacion] [datetime] NULL,
	[vchModificador] [varchar](100) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[numImporteRedondeo] [decimal](8, 4) NULL,
	[vchNumeroPedido] [varchar](20) NULL,
 CONSTRAINT [PK_PGA_ORDEN_PAGO_DIGITAL] PRIMARY KEY CLUSTERED 
(
	[intCodOrdenPagoDigital] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PGA_PARAMETRO] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PGA_PARAMETRO](
	[Codigo] [varchar](255) NOT NULL,
	[Valor] [varchar](255) NULL,
	[FechaCreacion] [datetime] NULL,
	[Creador] [varchar](20) NULL,
	[FechaModificacion] [datetime] NULL,
	[Modificador] [varchar](50) NULL,
	[SituacionRegistro] [char](1) NULL,
	[GuidRegistro] [varchar](100) NULL,
	[Descripcion] [varchar](255) NULL,
 CONSTRAINT [PK_PGA_PARAMETRO] PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PGA_TRANSACCION_KARDEX] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PGA_TRANSACCION_KARDEX](
	[CodTransaccionKardex] [int] IDENTITY(1,1) NOT NULL,
	[CodArticulo] [varchar](20) NULL,
	[CodTarifaCanal] [varchar](3) NULL,
	[CodPuntoVenta] [varchar](4) NULL,
	[TipoTransaccion] [varchar](50) NULL,
	[FechaTransaccion] [datetime] NULL,
	[Cantidad] [numeric](18, 4) NULL,
	[CodTransaccion] [numeric](18, 0) NULL,
	[Descripcion] [varchar](100) NULL,
	[Estado] [varchar](10) NULL,
	[Proceso] [varchar](50) NULL,
	[GuidRegistro] [varchar](100) NULL,
	[SituacionRegistro] [varchar](1) NULL,
	[Creador] [varchar](50) NULL,
	[FechaCreacion] [datetime] NULL,
	[Modificador] [varchar](50) NULL,
	[FechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_PGA_TRANSACCION_KARDEX] PRIMARY KEY CLUSTERED 
(
	[CodTransaccionKardex] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_ALMACEN] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_ALMACEN](
	[vchCodAlmacen] [varchar](16) NOT NULL,
	[vchDescripcion] [varchar](64) NOT NULL,
	[chrSituacionRegistro] [char](1) NOT NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_ALMACEN] PRIMARY KEY NONCLUSTERED 
(
	[vchCodAlmacen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_ARTICULO] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_ARTICULO](
	[vchCodArticulo] [varchar](16) NOT NULL,
	[vchCodGEstadistico] [varchar](6) NULL,
	[vchCodIva] [varchar](9) NULL,
	[vchDescripcion] [varchar](64) NULL,
	[numPrecioUnidad] [numeric](15, 4) NULL,
	[vchUnidadMedida] [varchar](3) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchTipoArticulo] [char](1) NULL,
	[vchCodigoDetraccion] [char](3) NULL,
	[vchGuidRegistro] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[vchCodArticulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_BANCO] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_BANCO](
	[vchCodBanco] [varchar](12) NOT NULL,
	[vchDescripcion] [varchar](64) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchGirador] [varchar](30) NULL,
	[flagBoletaDeposito] [varchar](1) NULL,
	[flagCheque] [varchar](1) NULL,
	[vchNumeroCtacte] [varchar](150) NULL,
	[chrFlagBoletaDeposito] [char](1) NULL,
PRIMARY KEY CLUSTERED 
(
	[vchCodBanco] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_CABECERA_TRANSACCION_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_CABECERA_TRANSACCION_BS](
	[numCodTransaccion] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[vchTipoOrden] [varchar](3) NULL,
	[vchCodPuntoVenta] [varchar](4) NULL,
	[numCodCliente] [int] NOT NULL,
	[intEstado] [int] NULL,
	[vchFormaPago] [varchar](3) NULL,
	[vchTipComprobante] [varchar](2) NULL,
	[numMontoBruto] [numeric](15, 4) NULL,
	[numMontoDescuento] [numeric](15, 4) NULL,
	[numMontoRecargo] [numeric](15, 4) NULL,
	[numSubTotal] [numeric](15, 4) NULL,
	[numMontoIGV] [numeric](15, 4) NULL,
	[numMontoTotal] [numeric](15, 4) NULL,
	[vchOrdenVenta] [varchar](32) NULL,
	[numTipoCambio] [numeric](9, 4) NULL,
	[numCodComprobante] [numeric](9, 4) NULL,
	[vchCodAlmacen] [varchar](8) NOT NULL,
	[vchCodTarifaCanal] [varchar](20) NULL,
	[vchCodMoneda] [varchar](6) NULL,
	[dtmFechaVenta] [datetime] NULL,
	[vchDocReferencia] [varchar](128) NULL,
	[vchCodigoBaan] [varchar](6) NULL,
	[vchCodClienteBaan] [varchar](10) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[numCodTransaccionOperador] [numeric](18, 0) NULL,
	[chrFlgTransferido] [char](1) NULL,
	[numOrdenVenta] [numeric](18, 0) NULL,
	[vchTipLiquidacion] [char](2) NULL,
	[vchTipBase] [varchar](24) NULL,
	[numCodAgencia] [int] NULL,
	[chrFlagClienteAgencia] [char](1) NULL,
	[vchGuidRegistro] [varchar](100) NULL,
 CONSTRAINT [PK_SGP_CABECERA_TRAN_BS] PRIMARY KEY CLUSTERED 
(
	[numCodTransaccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_CATEGORIA_VENTA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_CATEGORIA_VENTA](
	[intCodCategoria] [int] NOT NULL,
	[vchDescripcion] [varchar](50) NULL,
	[vchestado] [varchar](1) NULL,
 CONSTRAINT [PK_SGP_CATEGORIA_VENTA] PRIMARY KEY CLUSTERED 
(
	[intCodCategoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_CLIENTE] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_CLIENTE](
	[numCodCliente] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[vchCodRuta] [varchar](6) NULL,
	[vchNombres] [varchar](128) NULL,
	[vchApellidos] [varchar](128) NULL,
	[vchRazonSocial] [varchar](128) NULL,
	[vchTipPersona] [varchar](2) NULL,
	[vchDirecFacturacion] [varchar](128) NULL,
	[vchFacUbigeo] [varchar](6) NULL,
	[vchDirecEntrega] [varchar](60) NULL,
	[vchEntUbigeo] [varchar](6) NULL,
	[vchTipoCliente] [varchar](3) NULL,
	[vchCodigoBaan] [varchar](6) NULL,
	[vchTelefono] [varchar](15) NULL,
	[vchTelefax] [varchar](15) NULL,
	[vchEmail] [varchar](128) NULL,
	[vchTipDocumento] [varchar](2) NULL,
	[vchNumDocumento] [varchar](16) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchZonaVenta] [varchar](6) NULL,
	[vchEstado] [varchar](1) NULL,
	[numCodigoUsuario] [int] NULL,
	[numCodClienteOperador] [numeric](18, 0) NULL,
	[numCodClienteWeb] [numeric](18, 0) NULL,
	[chrFlgConvenio] [char](1) NULL,
	[chrFlgClienteFrecuente] [char](1) NULL,
	[numCodClienteCentral] [numeric](18, 0) NULL,
	[chrClienteEDP] [char](1) NULL,
	[vchApruebaMorosidad] [char](1) NULL,
 CONSTRAINT [PK__SGP_CLIENTE__442B18F2] PRIMARY KEY CLUSTERED 
(
	[numCodCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_COMPROBANTE_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_COMPROBANTE_BS](
	[numCodComprobante] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[numCodCliente] [numeric](18, 0) NOT NULL,
	[vchCodPuntoVenta] [varchar](4) NULL,
	[vchZonaVenta] [varchar](50) NULL,
	[numCodAgencia] [int] NULL,
	[vchNomComprobante] [varchar](64) NULL,
	[vchDirComprobante] [varchar](128) NULL,
	[vchFormaPago] [varchar](3) NULL,
	[vchTipoComprobante] [varchar](2) NULL,
	[vchNumSerie] [varchar](4) NULL,
	[vchNumComprobante] [varchar](10) NULL,
	[dtmFecEmision] [datetime] NULL,
	[chrFlagFacturado] [char](1) NULL,
	[chrFlagMigrado] [char](1) NULL,
	[numMontoBruto] [numeric](15, 4) NULL,
	[numMontoDescuento] [numeric](15, 4) NULL,
	[numMontoRecargo] [numeric](15, 4) NULL,
	[numSubTotal] [numeric](15, 4) NULL,
	[numMontoIGV] [numeric](15, 4) NULL,
	[numMontoTotal] [numeric](15, 4) NULL,
	[numMontoPagado] [numeric](15, 4) NULL,
	[vchEstado] [varchar](2) NULL,
	[vchObservaciones] [varchar](128) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchTipoDocumento] [varchar](2) NULL,
	[vchNumDocumento] [varchar](16) NULL,
	[vchMotivoAnulacion] [varchar](2) NULL,
	[vchCodOPublicacion] [varchar](128) NULL,
	[vchCodTransaccion] [varchar](128) NULL,
	[vchNumDocPublicacion] [varchar](64) NULL,
	[vchOrdenVenta] [varchar](32) NULL,
	[numTipoCambio] [numeric](9, 4) NULL,
	[chrFlagClienteAgencia] [char](1) NULL,
	[vchFlagMedioPago] [varchar](1) NULL,
	[vchFlagImpreso] [varchar](1) NULL,
 CONSTRAINT [PK_SGP_COMPROBANTE_BS] PRIMARY KEY CLUSTERED 
(
	[numCodComprobante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_DETALLE_COMPROBANTE_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_DETALLE_COMPROBANTE_BS](
	[numCodComprobante] [numeric](18, 0) NOT NULL,
	[numItem] [int] NOT NULL,
	[vchNomArticulo] [varchar](128) NULL,
	[numVeces] [int] NULL,
	[numCantidad] [numeric](15, 4) NULL,
	[numPrecioUnitario] [numeric](15, 4) NULL,
	[vchFecPublicacion] [varchar](128) NULL,
	[numMontoBruto] [numeric](15, 4) NULL,
	[numMontoDescuento] [numeric](15, 4) NULL,
	[numMontoRecargo] [numeric](15, 4) NULL,
	[numSubTotal] [numeric](15, 4) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchCodArticulo] [varchar](16) NULL,
	[vchUnidadMedida] [varchar](3) NULL,
	[vchTituloAviso] [varchar](128) NULL,
	[numPagina] [numeric](15, 4) NULL,
	[numPalabra] [int] NULL,
	[numAncho] [int] NULL,
	[numAlto] [int] NULL,
	[numIGV] [numeric](15, 4) NULL,
	[numEjemplar] [int] NULL,
	[numDia] [int] NULL,
 CONSTRAINT [PK_SGP_DETALLE_COMPROBANTE_BS] PRIMARY KEY CLUSTERED 
(
	[numCodComprobante] ASC,
	[numItem] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_DETALLE_TABLA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_DETALLE_TABLA](
	[numCodigo] [int] NOT NULL,
	[numItem] [int] NOT NULL,
	[vchValor] [varchar](64) NULL,
	[vchDescripcion] [varchar](128) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NULL,
	[dtmFechaCreacion] [datetime] NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchDesSecundaria] [varchar](128) NULL,
	[intJerarquia] [int] NULL,
	[vchCodSunat] [varchar](10) NULL,
 CONSTRAINT [PK__SGP_DETALLE_TABL__0B27A5C0] PRIMARY KEY CLUSTERED 
(
	[numCodigo] ASC,
	[numItem] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_DETALLE_TRANSACCION_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_DETALLE_TRANSACCION_BS](
	[numCodTransaccion] [numeric](18, 0) NOT NULL,
	[numLinTransaccion] [int] NOT NULL,
	[vchCodArticulo] [varchar](16) NULL,
	[numMontoBruto] [numeric](15, 4) NULL,
	[numSubTotal] [numeric](15, 4) NULL,
	[numCantidad] [numeric](15, 4) NULL,
	[numTarifa] [numeric](15, 4) NULL,
	[numIGV] [numeric](15, 4) NULL,
	[vchUnidadMedida] [varchar](3) NULL,
	[chrTipDscto01] [char](1) NULL,
	[tipoDscto01] [varchar](3) NULL,
	[numMontoDscto01] [numeric](15, 4) NULL,
	[numPorcDscto01] [numeric](15, 4) NULL,
	[chrTipDscto02] [char](1) NULL,
	[tipoDscto02] [varchar](3) NULL,
	[numMontoDscto02] [numeric](15, 4) NULL,
	[numPorcDscto02] [numeric](15, 4) NULL,
	[chrTipDscto03] [char](1) NULL,
	[tipoDscto03] [varchar](3) NULL,
	[numMontoDscto03] [numeric](15, 4) NULL,
	[numPorcDscto03] [numeric](15, 4) NULL,
	[dtmFechaEntrega] [datetime] NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_DETALLE_TRANSACCION_BS] PRIMARY KEY CLUSTERED 
(
	[numCodTransaccion] ASC,
	[numLinTransaccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_IMPRESORA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_IMPRESORA](
	[intCodImpresora] [int] NOT NULL,
	[vchTipComprobante] [varchar](1) NULL,
	[vchNomImpresora] [varchar](64) NULL,
	[vchCorrelativo] [int] NULL,
	[vchNumSerie] [varchar](4) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchIP] [varchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[intCodImpresora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_IVA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_IVA](
	[vchCodIva] [varchar](9) NOT NULL,
	[vchDescripcion] [varchar](30) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchTipoIVA] [varchar](2) NULL,
PRIMARY KEY CLUSTERED 
(
	[vchCodIva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_MEDIO_PAGO_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_MEDIO_PAGO_BS](
	[numCodMediPago] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[numCodTransaccion] [numeric](18, 0) NULL,
	[vchCodBanco] [varchar](12) NULL,
	[vchMedioPago] [varchar](2) NULL,
	[vchCodMoneda] [varchar](3) NULL,
	[vchCodCuentaCorriente] [varchar](3) NULL,
	[numTipoCambio] [numeric](9, 4) NULL,
	[vchNumDocumento] [varchar](40) NULL,
	[vchEstado] [varchar](2) NULL,
	[numMonto] [numeric](15, 4) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchCodSucursal] [varchar](32) NULL,
	[dtmFecDeposito] [datetime] NULL,
	[vchCodCiudad] [varchar](32) NULL,
	[vchReferencia] [varchar](80) NULL,
	[vchTipoTarjeta] [varchar](2) NULL,
	[numBDMonto] [numeric](15, 4) NULL,
	[chrflagProcesado] [char](1) NULL,
	[chrFlagPendienteAjuste] [varchar](1) NULL,
	[vchTipoCodigo] [varchar](3) NULL,
	[vchNumdocCodigo] [varchar](30) NULL,
	[numLinea] [int] NULL,
	[numMontoOrigen] [numeric](15, 4) NULL,
	[correlativoAnt] [numeric](18, 0) NULL,
	[numMontoRedondeo] [numeric](15, 4) NULL,
	[vchTipoMedioPago] [varchar](2) NULL,
	[vchOrigenTarjeta] [varchar](2) NULL,
	[vchOrigen] [varchar](20) NULL,
 CONSTRAINT [PK_SGP_MEDIO_PAGO_BS] PRIMARY KEY CLUSTERED 
(
	[numCodMediPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_MEDIO_PAGO_COMPROBANTE_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_MEDIO_PAGO_COMPROBANTE_BS](
	[numCodComprobante] [numeric](18, 0) NOT NULL,
	[numCodMediPago] [numeric](18, 0) NOT NULL,
	[numMontoPago] [numeric](15, 4) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_MEDIO_PAGO_COMPROBANTE_BS] PRIMARY KEY CLUSTERED 
(
	[numCodComprobante] ASC,
	[numCodMediPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_MOVIMIENTO_TRANSACCION_BS] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_MOVIMIENTO_TRANSACCION_BS](
	[numCodComprobante] [numeric](18, 0) NOT NULL,
	[numItem] [int] NOT NULL,
	[numCodTransaccion] [numeric](18, 0) NOT NULL,
	[numLinTransaccion] [int] NOT NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_MOVIMIENTO_TRANSACCION_BS] PRIMARY KEY CLUSTERED 
(
	[numCodComprobante] ASC,
	[numItem] ASC,
	[numCodTransaccion] ASC,
	[numLinTransaccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_ORDEN_TARIFA_CANAL] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_ORDEN_TARIFA_CANAL](
	[vchCodTarifaCanal] [varchar](3) NOT NULL,
	[vchCodTipoOrden] [varchar](6) NOT NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_ORDEN_TARIFA_CANAL] PRIMARY KEY NONCLUSTERED 
(
	[vchCodTarifaCanal] ASC,
	[vchCodTipoOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_PUNTO_VENTA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_PUNTO_VENTA](
	[vchCodPuntoVenta] [varchar](4) NOT NULL,
	[vchDescripcion] [varchar](45) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[intImpConBoleta] [int] NULL,
	[intImpCreBoleta] [int] NULL,
	[intImpConFactura] [int] NULL,
	[intImpCreFactura] [int] NULL,
	[intImpProforma] [int] NULL,
	[intImpReporte] [int] NULL,
	[intImpNotaCredito] [int] NULL,
	[intImpNotaDebito] [int] NULL,
	[vchCodUbigeo] [varchar](6) NULL,
	[vchZonaVenta] [varchar](6) NULL,
	[intLocalidad] [int] NULL,
	[chrFlagSoloGenVenta] [varchar](1) NULL,
	[chrFlgOperador] [char](1) NULL,
	[vchFacturaSerieImp] [varchar](10) NULL,
	[vchBoletaSerieImp] [varchar](10) NULL,
	[vchFlagDir] [varchar](1) NULL,
	[vchDireccion] [varchar](100) NULL,
	[vchDistrito] [varchar](80) NULL,
	[vchProvincia] [varchar](80) NULL,
	[vchDepartamento] [varchar](80) NULL,
	[vchFlagAvSunat] [varchar](1) NULL,
	[vchTelOpera] [varchar](30) NULL,
	[vchTipoPJ] [varchar](50) NULL,
	[chrBoletinOficial] [char](1) NULL,
	[intOrden] [int] NULL,
	[chrFlagVigente] [char](1) NULL,
 CONSTRAINT [PK__SGP_PUNTO_VENTA__3D491139] PRIMARY KEY CLUSTERED 
(
	[vchCodPuntoVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_PUNTO_VENTA_ALMACEN] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_PUNTO_VENTA_ALMACEN](
	[vchCodAlmacen] [varchar](16) NOT NULL,
	[vchCodPuntoVenta] [varchar](4) NOT NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_PUNTO_VENTA_ALMACEN] PRIMARY KEY NONCLUSTERED 
(
	[vchCodAlmacen] ASC,
	[vchCodPuntoVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_TARIFA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_TARIFA](
	[vchCodArticulo] [nvarchar](16) NOT NULL,
	[vchCodTarifaCanal] [nvarchar](3) NOT NULL,
	[numCantidad] [numeric](15, 4) NOT NULL,
	[numPrecioUnidad] [numeric](15, 4) NULL,
	[vchDescripcion] [nvarchar](64) NULL,
	[dtmFecInicio] [datetime] NOT NULL,
	[dtmFecCaducidad] [datetime] NOT NULL,
	[chrSituacionRegistro] [varchar](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchGuidRegistro] [varchar](100) NULL,
 CONSTRAINT [PK_SGP_TARIFA] PRIMARY KEY CLUSTERED 
(
	[vchCodArticulo] ASC,
	[vchCodTarifaCanal] ASC,
	[numCantidad] ASC,
	[dtmFecInicio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_TARIFA_CANAL] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_TARIFA_CANAL](
	[vchCodTarifaCanal] [varchar](3) NOT NULL,
	[vchFormaPago] [varchar](3) NULL,
	[intLocalidad] [int] NULL,
	[vchDescripcion] [varchar](30) NULL,
	[vchMoneda] [varchar](3) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
 CONSTRAINT [PK__SGP_TARIFA_CANAL__6BCEF5F8] PRIMARY KEY CLUSTERED 
(
	[vchCodTarifaCanal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_TIPO_LIQUIDACION] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_TIPO_LIQUIDACION](
	[intCodTipoLiquidacion] [int] NULL,
	[vchDescripcion] [varchar](32) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NULL,
	[dtmFechaCreacion] [datetime] NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_TIPO_ORDEN] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_TIPO_ORDEN](
	[vchCodTipoOrden] [varchar](6) NOT NULL,
	[vchDescripcion] [varchar](64) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[intCodTipoLiquidacion] [int] NULL,
	[intOrden] [int] NULL,
	[chrFlagBienServicio] [char](1) NULL,
 CONSTRAINT [PK_SGP_TIPO_ORDEN] PRIMARY KEY NONCLUSTERED 
(
	[vchCodTipoOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_TIPO_ORDEN_ARTICULO] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_TIPO_ORDEN_ARTICULO](
	[vchCodTipoOrden] [varchar](6) NOT NULL,
	[vchCodArticulo] [varchar](16) NOT NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NULL,
	[dtmFechaCreacion] [datetime] NULL,
 CONSTRAINT [PK_SGP_TIPO_ORDEN_ARTICULO] PRIMARY KEY CLUSTERED 
(
	[vchCodTipoOrden] ASC,
	[vchCodArticulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_UBIGEO] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_UBIGEO](
	[vchCodUbigeo] [varchar](6) NOT NULL,
	[vchDescripcion] [varchar](64) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[numPrioridad] [int] NULL,
	[numAlto] [numeric](15, 4) NULL,
	[numAncho] [numeric](15, 4) NULL,
	[vchDescripcion2] [varchar](300) NULL,
	[vchTipo] [varchar](1) NULL,
	[vchCodPais] [varchar](2) NULL,
 CONSTRAINT [PK__SGP_UBIGEO__405A880E] PRIMARY KEY CLUSTERED 
(
	[vchCodUbigeo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_USUARIO] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_USUARIO](
	[numCodigoUsuario] [int] IDENTITY(1,1) NOT NULL,
	[vchCodPuntoVenta] [varchar](4) NULL,
	[chrFlagAutorizaDscto] [char](1) NULL,
	[vchEstacionTrabajo] [varchar](2) NULL,
	[vchLogin] [varchar](16) NOT NULL,
	[vchPassword] [varchar](16) NOT NULL,
	[vchNombre] [varchar](64) NULL,
	[dtmFechaInicioAct] [datetime] NOT NULL,
	[dtmFechaFinAct] [datetime] NOT NULL,
	[vchEmail] [varchar](100) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[chrFlagSolRegVenta] [char](1) NULL,
	[chrFlagEdicionPrecio] [char](1) NULL,
	[chrFlagCambioFinal] [char](1) NULL,
	[chrFlagDiagramacion] [char](1) NULL,
	[vchRutaImpSumilla] [varchar](512) NULL,
	[dtmFechaExpiraClave] [datetime] NULL,
	[vchMacAddress] [varchar](80) NULL,
	[numcodEmpleado] [numeric](8, 0) NULL,
	[vchNumeroDocumento] [nvarchar](8) NULL,
	[vchTelefono] [varchar](20) NULL,
	[vchCodResetearPassword] [varchar](64) NULL,
	[chrFlagAdministradorSistema] [char](1) NULL,
	[vchTipoAutentificacion] [varchar](5) NULL,
 CONSTRAINT [PK__SGP_USUARIO__4F12BBB9] PRIMARY KEY CLUSTERED 
(
	[numCodigoUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SGP_ZONA_VENTA] Fecha de script: 27/10/2026 09:32:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SGP_ZONA_VENTA](
	[vchZonaVenta] [varchar](6) NOT NULL,
	[vchDescripcion] [varchar](30) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
	[vchCodigoBaan] [varchar](6) NULL,
 CONSTRAINT [PK__SGP_ZONA_VENTA__6A3BB341] PRIMARY KEY CLUSTERED 
(
	[vchZonaVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SGP_ALMACEN] ADD  CONSTRAINT [DF_SGP_ALMACEN_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_ARTICULO] ADD  CONSTRAINT [DF__SGP_ARTIC__chrSi__17236851]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_ARTICULO] ADD  CONSTRAINT [DF__SGP_ARTIC__dtmFe__18178C8A]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_BANCO] ADD  CONSTRAINT [DF__SGP_BANCO__chrSi__00FF1D08]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_BANCO] ADD  CONSTRAINT [DF__SGP_BANCO__dtmFe__01F34141]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_CABECERA_TRANSACCION_BS] ADD  CONSTRAINT [DF__SGP_CABEC__vchFo__0A266454]  DEFAULT ('C00') FOR [vchFormaPago]
GO
ALTER TABLE [dbo].[SGP_CABECERA_TRANSACCION_BS] ADD  CONSTRAINT [DF__SGP_CABEC__chrSi__0B1A888D]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_CABECERA_TRANSACCION_BS] ADD  CONSTRAINT [DF__SGP_CABEC__dtmFe__0C0EACC6]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_CABECERA_TRANSACCION_BS] ADD  CONSTRAINT [DF_SGP_CABECERA_TRANSACCION_BS_chrFlgTransferido]  DEFAULT ('N') FOR [chrFlgTransferido]
GO
ALTER TABLE [dbo].[SGP_CLIENTE] ADD  CONSTRAINT [DF__SGP_CLIEN__chrSi__4242D080]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_CLIENTE] ADD  CONSTRAINT [DF__SGP_CLIEN__dtmFe__4336F4B9]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_CLIENTE] ADD  CONSTRAINT [DF_SGP_CLIENTE_vchEstado]  DEFAULT ('1') FOR [vchEstado]
GO
ALTER TABLE [dbo].[SGP_CLIENTE] ADD  CONSTRAINT [DF_SGP_CLIENTE_chrFlgConvenio]  DEFAULT ('N') FOR [chrFlgConvenio]
GO
ALTER TABLE [dbo].[SGP_CLIENTE] ADD  CONSTRAINT [DF_SGP_CLIENTE_chrClienteEDP]  DEFAULT ('0') FOR [chrClienteEDP]
GO
ALTER TABLE [dbo].[SGP_CLIENTE] ADD  DEFAULT ('0') FOR [vchApruebaMorosidad]
GO
ALTER TABLE [dbo].[SGP_COMPROBANTE_BS] ADD  CONSTRAINT [DF_SGP_COMPROBANTE_BS_chrSituacionRegistro]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_COMPROBANTE_BS] ADD  CONSTRAINT [DF_SGP_COMPROBANTE_BS_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_COMPROBANTE_BS] ADD  DEFAULT ('N') FOR [vchFlagMedioPago]
GO
ALTER TABLE [dbo].[SGP_COMPROBANTE_BS] ADD  DEFAULT ('N') FOR [vchFlagImpreso]
GO
ALTER TABLE [dbo].[SGP_DETALLE_COMPROBANTE_BS] ADD  CONSTRAINT [DF_SGP_DETALLE_COMPROBANTE_BS_chrSituacionRegistro]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_DETALLE_COMPROBANTE_BS] ADD  CONSTRAINT [DF_SGP_DETALLE_COMPROBANTE_BS_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA] ADD  CONSTRAINT [DF__SGP_DETAL__chrSi__0A338187]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA] ADD  CONSTRAINT [DF_SGP_DETALLE_TABLA_intJerarquia]  DEFAULT ((1)) FOR [intJerarquia]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TRANSACCION_BS] ADD  CONSTRAINT [DF__SGP_DETAL__chrSi__0EEB1971]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TRANSACCION_BS] ADD  CONSTRAINT [DF__SGP_DETAL__dtmFe__0FDF3DAA]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_IMPRESORA] ADD  CONSTRAINT [DF__SGP_IMPRE__chrSi__6C23FBB3]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_IMPRESORA] ADD  CONSTRAINT [DF__SGP_IMPRE__dtmFe__6D181FEC]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_IVA] ADD  CONSTRAINT [DF__SGP_IVA__chrSitu__0F824689]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_IVA] ADD  CONSTRAINT [DF__SGP_IVA__dtmFech__10766AC2]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_MEDIO_PAGO_BS] ADD  CONSTRAINT [DF_SGP_MEDIO_PAGO_BS_chrSituacionRegistro]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_MEDIO_PAGO_BS] ADD  CONSTRAINT [DF_SGP_MEDIO_PAGO_BS_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_MEDIO_PAGO_BS] ADD  DEFAULT ('N') FOR [chrflagProcesado]
GO
ALTER TABLE [dbo].[SGP_MEDIO_PAGO_BS] ADD  DEFAULT ('N') FOR [chrFlagPendienteAjuste]
GO
ALTER TABLE [dbo].[SGP_MEDIO_PAGO_COMPROBANTE_BS] ADD  CONSTRAINT [DF_SGP_MEDIO_PAGO_COMPROBANTE_BS_chrSituacionRegistro]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_MEDIO_PAGO_COMPROBANTE_BS] ADD  CONSTRAINT [DF_SGP_MEDIO_PAGO_COMPROBANTE_BS_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_MOVIMIENTO_TRANSACCION_BS] ADD  CONSTRAINT [DF_SGP_MOVIMIENTO_TRANSACCION_BS_chrSituacionRegistro]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_MOVIMIENTO_TRANSACCION_BS] ADD  CONSTRAINT [DF_SGP_MOVIMIENTO_TRANSACCION_BS_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA] ADD  CONSTRAINT [DF__SGP_PUNTO__chrSi__3B60C8C7]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA] ADD  CONSTRAINT [DF__SGP_PUNTO__dtmFe__3C54ED00]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA] ADD  CONSTRAINT [DF_SGP_PUNTO_VENTA_intLocalidad]  DEFAULT ((1)) FOR [intLocalidad]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA] ADD  CONSTRAINT [DF_SGP_PUNTO_VENTA_chrFlagSoloGenVenta]  DEFAULT ('N') FOR [chrFlagSoloGenVenta]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA] ADD  CONSTRAINT [DF_SGP_PUNTO_VENTA_chrFlgOperador]  DEFAULT ('N') FOR [chrFlgOperador]
GO
ALTER TABLE [dbo].[SGP_TARIFA] ADD  CONSTRAINT [DF_SGP_TARIFA_chrSituacionRegistro]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_TARIFA_CANAL] ADD  CONSTRAINT [DF__SGP_TARIF__chrSi__69E6AD86]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_TARIFA_CANAL] ADD  CONSTRAINT [DF__SGP_TARIF__dtmFe__6ADAD1BF]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_TIPO_ORDEN] ADD  CONSTRAINT [DF_SGP_TIPO_ORDEN_dtmFechaCreacion]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_TIPO_ORDEN_ARTICULO] ADD  CONSTRAINT [DF__SGP_TIPO___chrSi__02EF56E0]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_TIPO_ORDEN_ARTICULO] ADD  CONSTRAINT [DF__SGP_TIPO___dtmFe__03E37B19]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_UBIGEO] ADD  CONSTRAINT [DF__SGP_UBIGE__chrSi__3E723F9C]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_UBIGEO] ADD  CONSTRAINT [DF__SGP_UBIGE__dtmFe__3F6663D5]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_USUARIO] ADD  CONSTRAINT [DF__SGP_USUAR__chrSi__4E1E9780]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_USUARIO] ADD  CONSTRAINT [DF_SGP_USUARIO_chrFlagSolRegVenta]  DEFAULT ('N') FOR [chrFlagSolRegVenta]
GO
ALTER TABLE [dbo].[SGP_USUARIO] ADD  CONSTRAINT [DF_SGP_USUARIO_chrFlagEdicionPrecio]  DEFAULT ('N') FOR [chrFlagEdicionPrecio]
GO
ALTER TABLE [dbo].[SGP_ZONA_VENTA] ADD  CONSTRAINT [DF__SGP_ZONA___chrSi__68536ACF]  DEFAULT ('A') FOR [chrSituacionRegistro]
GO
ALTER TABLE [dbo].[SGP_ZONA_VENTA] ADD  CONSTRAINT [DF__SGP_ZONA___dtmFe__69478F08]  DEFAULT (getdate()) FOR [dtmFechaCreacion]
GO
ALTER TABLE [dbo].[SGP_CLIENTE]  WITH NOCHECK ADD  CONSTRAINT [FK__SGP_CLIEN__vchCo__451F3D2B] FOREIGN KEY([vchCodRuta])
REFERENCES [dbo].[SGP_RUTA] ([vchCodRuta])
GO
ALTER TABLE [dbo].[SGP_CLIENTE] CHECK CONSTRAINT [FK__SGP_CLIEN__vchCo__451F3D2B]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA]  WITH NOCHECK ADD  CONSTRAINT [FK__SGP_DETAL__numCo__0EF836A4] FOREIGN KEY([numCodigo])
REFERENCES [dbo].[SGP_TABLA] ([numCodigo])
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA] CHECK CONSTRAINT [FK__SGP_DETAL__numCo__0EF836A4]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA]  WITH NOCHECK ADD  CONSTRAINT [FK__SGP_DETAL__numCo__11D4A34F] FOREIGN KEY([numCodigo])
REFERENCES [dbo].[SGP_TABLA] ([numCodigo])
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA] CHECK CONSTRAINT [FK__SGP_DETAL__numCo__11D4A34F]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA]  WITH NOCHECK ADD  CONSTRAINT [FK__SGP_DETAL__numCo__14B10FFA] FOREIGN KEY([numCodigo])
REFERENCES [dbo].[SGP_TABLA] ([numCodigo])
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA] CHECK CONSTRAINT [FK__SGP_DETAL__numCo__14B10FFA]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA]  WITH NOCHECK ADD  CONSTRAINT [FK__SGP_DETAL__numCo__1881A0DE] FOREIGN KEY([numCodigo])
REFERENCES [dbo].[SGP_TABLA] ([numCodigo])
GO
ALTER TABLE [dbo].[SGP_DETALLE_TABLA] CHECK CONSTRAINT [FK__SGP_DETAL__numCo__1881A0DE]
GO
ALTER TABLE [dbo].[SGP_DETALLE_TRANSACCION_BS]  WITH CHECK ADD  CONSTRAINT [FK_SGP_DETALLE_TRANSACCION_BS] FOREIGN KEY([numCodTransaccion])
REFERENCES [dbo].[SGP_CABECERA_TRANSACCION_BS] ([numCodTransaccion])
GO
ALTER TABLE [dbo].[SGP_DETALLE_TRANSACCION_BS] CHECK CONSTRAINT [FK_SGP_DETALLE_TRANSACCION_BS]
GO
ALTER TABLE [dbo].[SGP_ORDEN_TARIFA_CANAL]  WITH CHECK ADD  CONSTRAINT [FK_SGP_ORDEN_TARIFA_CANAL] FOREIGN KEY([vchCodTarifaCanal])
REFERENCES [dbo].[SGP_TARIFA_CANAL] ([vchCodTarifaCanal])
GO
ALTER TABLE [dbo].[SGP_ORDEN_TARIFA_CANAL] CHECK CONSTRAINT [FK_SGP_ORDEN_TARIFA_CANAL]
GO
ALTER TABLE [dbo].[SGP_ORDEN_TARIFA_CANAL]  WITH CHECK ADD  CONSTRAINT [FK_SGP_ORDEN_TARIFA_CANAL1] FOREIGN KEY([vchCodTipoOrden])
REFERENCES [dbo].[SGP_TIPO_ORDEN] ([vchCodTipoOrden])
GO
ALTER TABLE [dbo].[SGP_ORDEN_TARIFA_CANAL] CHECK CONSTRAINT [FK_SGP_ORDEN_TARIFA_CANAL1]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA_ALMACEN]  WITH NOCHECK ADD  CONSTRAINT [FK_SGP_PUNTO_VENTA_ALMACEN] FOREIGN KEY([vchCodAlmacen])
REFERENCES [dbo].[SGP_ALMACEN] ([vchCodAlmacen])
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA_ALMACEN] CHECK CONSTRAINT [FK_SGP_PUNTO_VENTA_ALMACEN]
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA_ALMACEN]  WITH CHECK ADD  CONSTRAINT [FK_SGP_PUNTO_VENTA_ALMACEN1] FOREIGN KEY([vchCodPuntoVenta])
REFERENCES [dbo].[SGP_PUNTO_VENTA] ([vchCodPuntoVenta])
GO
ALTER TABLE [dbo].[SGP_PUNTO_VENTA_ALMACEN] CHECK CONSTRAINT [FK_SGP_PUNTO_VENTA_ALMACEN1]
GO
ALTER TABLE [dbo].[SGP_TIPO_ORDEN_ARTICULO]  WITH CHECK ADD  CONSTRAINT [FK_SGP_TIPO_ORDEN_ARTICULO] FOREIGN KEY([vchCodArticulo])
REFERENCES [dbo].[SGP_ARTICULO] ([vchCodArticulo])
GO
ALTER TABLE [dbo].[SGP_TIPO_ORDEN_ARTICULO] CHECK CONSTRAINT [FK_SGP_TIPO_ORDEN_ARTICULO]
GO
CREATE TABLE [dbo].[SGP_DETALLE_IVA](
	[vchCodIva] [varchar](9) NOT NULL,
	[dtmFechaVigencia] [datetime] NOT NULL,
	[numValorIGV] [numeric](15, 4) NULL,
	[chrSituacionRegistro] [char](1) NULL,
	[vchCreador] [varchar](16) NOT NULL,
	[dtmFechaCreacion] [datetime] NOT NULL,
	[vchModificador] [varchar](16) NULL,
	[dtmFechaModificacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[vchCodIva] ASC,
	[dtmFechaVigencia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO