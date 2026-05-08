using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IMesaRepositorio
{
    Task<Mesa?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Mesa?> ObtenerPorNumeroAsync(int numero, CancellationToken cancelacion = default);
    Task<List<Mesa>> ObtenerTodasAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(Mesa mesa, CancellationToken cancelacion = default);
    Task<Mesa?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default);
}
