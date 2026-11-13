IF DB_ID(N'PedidosDB1_AD01_Basico') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [PedidosDB1_AD01_Basico]');
END
GO
USE [PedidosDB1_AD01_Basico];
GO

IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
CREATE TABLE dbo.Cliente (
    IdCliente INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombres NVARCHAR(150) NOT NULL,
    Cedula NVARCHAR(10) NOT NULL,
    Correo NVARCHAR(150) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Pedido', N'U') IS NULL
CREATE TABLE dbo.Pedido (
    IdPedido INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdCliente INT NOT NULL,
    Fecha DATETIME2 NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente(IdCliente)
);
GO
IF OBJECT_ID(N'dbo.DetallePedido', N'U') IS NULL
CREATE TABLE dbo.DetallePedido (
    IdDetalle INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdPedido INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_Detalle_Pedido FOREIGN KEY (IdPedido) REFERENCES dbo.Pedido(IdPedido)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cliente_Cedula' AND object_id = OBJECT_ID('dbo.Cliente'))
CREATE UNIQUE INDEX IX_Cliente_Cedula ON dbo.Cliente(Cedula);
GO
