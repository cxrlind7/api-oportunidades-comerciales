-- Stored procedures de Oportunidades
-- Correr después de 01_database.sql

USE OportunidadesComerciales;
GO

-- Estado y FechaCreacionUtc los pone la tabla por default
CREATE OR ALTER PROCEDURE dbo.sp_Oportunidades_Crear
    @Nombre        NVARCHAR(100),
    @Cliente       NVARCHAR(120),
    @MontoEstimado DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Oportunidades (Nombre, Cliente, MontoEstimado)
    OUTPUT INSERTED.Id, INSERTED.Nombre, INSERTED.Cliente, INSERTED.MontoEstimado, INSERTED.Estado, INSERTED.FechaCreacionUtc
    VALUES (@Nombre, @Cliente, @MontoEstimado);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Oportunidades_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Nombre, Cliente, MontoEstimado, Estado, FechaCreacionUtc
    FROM dbo.Oportunidades
    WHERE Id = @Id;
END
GO

-- Si @Estado es NULL regresa todas
CREATE OR ALTER PROCEDURE dbo.sp_Oportunidades_Listar
    @Estado VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Nombre, Cliente, MontoEstimado, Estado, FechaCreacionUtc
    FROM dbo.Oportunidades
    WHERE @Estado IS NULL OR Estado = @Estado
    ORDER BY Id;
END
GO

-- Solo actualiza si el estado sigue siendo @EstadoEsperado (concurrencia optimista)
-- Si regresa 0 es que otra petición lo cambió antes y la API responde 409
CREATE OR ALTER PROCEDURE dbo.sp_Oportunidades_ActualizarEstado
    @Id             INT,
    @EstadoEsperado VARCHAR(20),
    @NuevoEstado    VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Oportunidades
    SET Estado = @NuevoEstado
    WHERE Id = @Id AND Estado = @EstadoEsperado;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO
