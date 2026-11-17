using Oportunidades.Application.DTOs;
using Oportunidades.Application.Interfaces;
using Oportunidades.Application.Validation;
using Oportunidades.Domain.Exceptions;

namespace Oportunidades.Application.Services;

public sealed class OportunidadService : IOportunidadService
{
    private const string Recurso = "la oportunidad";

    private readonly IOportunidadRepository _repository;

    public OportunidadService(IOportunidadRepository repository)
    {
        _repository = repository;
    }

    public async Task<OportunidadResponse> CrearAsync(CrearOportunidadRequest request, CancellationToken cancellationToken)
    {
        var (nombre, cliente, monto) = OportunidadValidator.ValidarCreacion(request);

        var creada = await _repository.CrearAsync(nombre, cliente, monto, cancellationToken);
        return OportunidadResponse.FromEntity(creada);
    }

    public async Task<OportunidadResponse> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        OportunidadValidator.ValidarId(id);

        var oportunidad = await _repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Recurso, id);

        return OportunidadResponse.FromEntity(oportunidad);
    }

    public async Task<IReadOnlyList<OportunidadResponse>> ListarAsync(string? estado, CancellationToken cancellationToken)
    {
        var filtro = OportunidadValidator.ValidarFiltroEstado(estado);

        var oportunidades = await _repository.ListarAsync(filtro, cancellationToken);
        return oportunidades.Select(OportunidadResponse.FromEntity).ToList();
    }

    public async Task<OportunidadResponse> ActualizarEstadoAsync(int id, ActualizarEstadoRequest request, CancellationToken cancellationToken)
    {
        var nuevoEstado = OportunidadValidator.ValidarCambioEstado(id, request);

        var oportunidad = await _repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Recurso, id);

        // guardo el estado que leí para mandarlo al SP
        var estadoPersistido = oportunidad.Estado;

        // si es el mismo estado no hago nada en la BD
        if (!oportunidad.CambiarEstado(nuevoEstado))
            return OportunidadResponse.FromEntity(oportunidad);

        // si alguien lo cambió mientras tanto, el SP no actualiza nada y regreso 409
        var actualizado = await _repository.ActualizarEstadoAsync(id, estadoPersistido, nuevoEstado, cancellationToken);
        if (!actualizado)
            throw new ConcurrencyConflictException("La oportunidad", id);

        return OportunidadResponse.FromEntity(oportunidad);
    }
}
