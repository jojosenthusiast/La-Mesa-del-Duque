using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IDevolucionRepositorio
{
    Task<List<DevolucionPago>> ObtenerPorPagoAsync(Guid pagoOriginalId, CancellationToken cancelacion = default);
    Task<List<DevolucionPago>> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancelacion = default);
    Task<List<DevolucionPago>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, CancellationToken cancelacion = default);
    Task AgregarAsync(DevolucionPago devolucion, CancellationToken cancelacion = default);
}
