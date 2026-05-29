using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface ITurnoCajaRepositorio
{
    Task<TurnoCaja?> ObtenerTurnoActivoAsync(CancellationToken ct = default);
    Task<TurnoCaja?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TurnoCaja>> ObtenerHistorialAsync(int pagina, int porPagina, CancellationToken ct = default);
    Task AgregarAsync(TurnoCaja turno, CancellationToken ct = default);
}
