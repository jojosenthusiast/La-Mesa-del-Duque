using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IIngredienteRepositorio
{
    Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Ingrediente?> ObtenerPorIdConProveedorAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Ingrediente>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task<List<Ingrediente>> ObtenerTodosConProveedorAsync(CancellationToken cancelacion = default);
    Task<List<Ingrediente>> ObtenerPorProveedorIdAsync(Guid proveedorId, CancellationToken cancelacion = default);
    Task AgregarAsync(Ingrediente ingrediente, CancellationToken cancelacion = default);
}
