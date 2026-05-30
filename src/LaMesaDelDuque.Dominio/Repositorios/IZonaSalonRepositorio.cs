using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IZonaSalonRepositorio
{
    Task<ZonaSalon?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<ZonaSalon>> ObtenerActivasOrdenadasAsync(CancellationToken cancelacion = default);
    Task<List<ZonaSalon>> ObtenerTodasAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(ZonaSalon zona, CancellationToken cancelacion = default);
    Task<ZonaSalon?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default);
}
