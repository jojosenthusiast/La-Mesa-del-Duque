using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IProveedorRepositorio
{
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Proveedor>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(Proveedor proveedor, CancellationToken cancelacion = default);
}
