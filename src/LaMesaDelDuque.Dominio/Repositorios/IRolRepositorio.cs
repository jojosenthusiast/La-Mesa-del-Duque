using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IRolRepositorio
{
    Task<Rol?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Rol?> ObtenerPorNombreAsync(string nombre, CancellationToken cancelacion = default);
    Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(Rol rol, CancellationToken cancelacion = default);
}
