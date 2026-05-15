using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IPagoRepositorio
{
    Task<Pago?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Pago>> ObtenerPorCuentaAsync(Guid cuentaId, CancellationToken cancelacion = default);
    Task AgregarAsync(Pago pago, CancellationToken cancelacion = default);
}
