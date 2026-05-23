using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IProveedorRepositorio
{
    Task<List<Proveedor>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(Proveedor proveedor, CancellationToken cancelacion = default);
}
