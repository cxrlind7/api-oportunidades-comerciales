using System.Data;
using Microsoft.Data.SqlClient;
using Oportunidades.Application.Interfaces;
using Oportunidades.Domain.Entities;
using Oportunidades.Domain.Enums;
using Oportunidades.Infrastructure.Persistence;

namespace Oportunidades.Infrastructure.Repositories;

// Todo se hace con stored procedures usando ADO.NET
public sealed class OportunidadRepository : IOportunidadRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public OportunidadRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Oportunidad> CrearAsync(string nombre, string cliente, decimal montoEstimado, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await using var command = CrearComando(connection, "dbo.sp_Oportunidades_Crear");
        command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 100) { Value = nombre });
        command.Parameters.Add(new SqlParameter("@Cliente", SqlDbType.NVarChar, 120) { Value = cliente });
        command.Parameters.Add(new SqlParameter("@MontoEstimado", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = montoEstimado });

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("El SP de crear no regresó nada.");

        return Mapear(reader);
    }

    public async Task<Oportunidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await using var command = CrearComando(connection, "dbo.sp_Oportunidades_ObtenerPorId");
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? Mapear(reader) : null;
    }

    public async Task<IReadOnlyList<Oportunidad>> ListarAsync(EstadoOportunidad? estado, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await using var command = CrearComando(connection, "dbo.sp_Oportunidades_Listar");
        command.Parameters.Add(ParametroEstado("@Estado", estado));

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var lista = new List<Oportunidad>();
        while (await reader.ReadAsync(cancellationToken))
            lista.Add(Mapear(reader));

        return lista;
    }

    public async Task<bool> ActualizarEstadoAsync(int id, EstadoOportunidad estadoEsperado, EstadoOportunidad nuevoEstado, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await using var command = CrearComando(connection, "dbo.sp_Oportunidades_ActualizarEstado");
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
        command.Parameters.Add(ParametroEstado("@EstadoEsperado", estadoEsperado));
        command.Parameters.Add(ParametroEstado("@NuevoEstado", nuevoEstado));

        await connection.OpenAsync(cancellationToken);
        var filasAfectadas = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        // 0 filas = alguien más cambió el estado antes
        return filasAfectadas == 1;
    }

    private static SqlCommand CrearComando(SqlConnection connection, string procedimiento) =>
        new(procedimiento, connection) { CommandType = CommandType.StoredProcedure };

    private static SqlParameter ParametroEstado(string nombre, EstadoOportunidad? estado) =>
        new(nombre, SqlDbType.VarChar, 20) { Value = estado?.ToString() ?? (object)DBNull.Value };

    private static Oportunidad Mapear(SqlDataReader reader)
    {
        // la BD guarda la fecha en UTC, la marco así para que el JSON salga con Z
        var fecha = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("FechaCreacionUtc")), DateTimeKind.Utc);

        return new Oportunidad(
            reader.GetInt32(reader.GetOrdinal("Id")),
            reader.GetString(reader.GetOrdinal("Nombre")),
            reader.GetString(reader.GetOrdinal("Cliente")),
            reader.GetDecimal(reader.GetOrdinal("MontoEstimado")),
            Enum.Parse<EstadoOportunidad>(reader.GetString(reader.GetOrdinal("Estado"))),
            fecha);
    }
}
