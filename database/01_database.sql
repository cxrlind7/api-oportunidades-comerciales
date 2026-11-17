-- Crea la base de datos y la tabla Oportunidades
-- Se puede correr varias veces, si ya existe no hace nada

IF DB_ID('OportunidadesComerciales') IS NULL
    CREATE DATABASE OportunidadesComerciales;
GO

USE OportunidadesComerciales;
GO

IF OBJECT_ID('dbo.Oportunidades') IS NULL
    CREATE TABLE dbo.Oportunidades
    (
        Id               INT IDENTITY(1,1) NOT NULL,
        Nombre           NVARCHAR(100)     NOT NULL,
        Cliente          NVARCHAR(120)     NOT NULL,
        MontoEstimado    DECIMAL(18,2)     NOT NULL,
        Estado           VARCHAR(20)       NOT NULL CONSTRAINT DF_Oportunidades_Estado DEFAULT 'Nueva',
        FechaCreacionUtc DATETIME2         NOT NULL CONSTRAINT DF_Oportunidades_FechaCreacionUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Oportunidades PRIMARY KEY (Id),
        CONSTRAINT CK_Oportunidades_MontoEstimado CHECK (MontoEstimado > 0),
        CONSTRAINT CK_Oportunidades_Estado CHECK (Estado IN ('Nueva', 'EnSeguimiento', 'Ganada', 'Perdida'))
    );
GO

-- índice para el filtro por estado del listado
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Oportunidades_Estado')
    CREATE INDEX IX_Oportunidades_Estado ON dbo.Oportunidades (Estado);
GO
