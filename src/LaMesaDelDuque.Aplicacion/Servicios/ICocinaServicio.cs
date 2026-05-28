using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ICocinaServicio
{
    Task GenerarOrdenesAsync(Guid pedidoId, IEnumerable<Guid>? soloDetalles = null, CancellationToken ct = default);
    Task<List<OrdenCocinaDto>> ListarPendientesAsync(EstacionCocina? estacion = null, CancellationToken ct = default);
    Task<OrdenCocinaDto> MarcarListoAsync(Guid ordenId, CancellationToken ct = default);
    Task<OrdenCocinaDto> RecuperarAsync(Guid ordenId, CancellationToken ct = default);
}
