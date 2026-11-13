IF DB_ID(N'InventarioDB1_AD01_Basico') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [InventarioDB1_AD01_Basico]');
END
GO
USE [InventarioDB1_AD01_Basico];
GO

IF OBJECT_ID(N'dbo.Bodega', N'U') IS NULL
CREATE TABLE dbo.Bodega (
    IdBodega INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Ciudad NVARCHAR(100) NOT NULL,
    Direccion NVARCHAR(200) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Existencia', N'U') IS NULL
CREATE TABLE dbo.Existencia (
    IdExistencia INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProducto INT NOT NULL,
    IdBodega INT NOT NULL,
    Stock INT NOT NULL,
    CONSTRAINT FK_Existencia_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO
IF OBJECT_ID(N'dbo.MovimientoInventario', N'U') IS NULL
CREATE TABLE dbo.MovimientoInventario (
    IdMovimiento INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProducto INT NOT NULL,
    IdBodega INT NOT NULL,
    Tipo NVARCHAR(MAX) NOT NULL,
    Cantidad INT NOT NULL,
    Fecha DATETIME2 NOT NULL,
    CONSTRAINT FK_Movimiento_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO
IF OBJECT_ID(N'dbo.ReservaStock', N'U') IS NULL
CREATE TABLE dbo.ReservaStock (
    IdReserva INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdProducto INT NOT NULL,
    IdBodega INT NOT NULL,
    Cantidad INT NOT NULL,
    Estado NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Reserva_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Existencia_IdProducto_IdBodega' AND object_id = OBJECT_ID('dbo.Existencia'))
CREATE UNIQUE INDEX IX_Existencia_IdProducto_IdBodega ON dbo.Existencia(IdProducto,IdBodega);
GO
