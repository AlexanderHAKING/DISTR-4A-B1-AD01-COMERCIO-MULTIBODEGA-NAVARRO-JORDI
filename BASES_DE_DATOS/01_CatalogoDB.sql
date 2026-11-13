IF DB_ID(N'CatalogoDB_AD01_Basico') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [CatalogoDB_AD01_Basico]');
END
GO
USE [CatalogoDB_AD01_Basico];
GO

IF OBJECT_ID(N'dbo.Categoria', N'U') IS NULL
CREATE TABLE dbo.Categoria (
    IdCategoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Marca', N'U') IS NULL
CREATE TABLE dbo.Marca (
    IdMarca INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    PaisOrigen NVARCHAR(100) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
CREATE TABLE dbo.Producto (
    IdProducto INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IdCategoria INT NOT NULL,
    IdMarca INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    SKU NVARCHAR(60) NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Estado BIT NOT NULL,
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (IdCategoria) REFERENCES dbo.Categoria(IdCategoria),
    CONSTRAINT FK_Producto_Marca FOREIGN KEY (IdMarca) REFERENCES dbo.Marca(IdMarca)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Producto_SKU' AND object_id = OBJECT_ID('dbo.Producto'))
CREATE UNIQUE INDEX IX_Producto_SKU ON dbo.Producto(SKU);
GO
