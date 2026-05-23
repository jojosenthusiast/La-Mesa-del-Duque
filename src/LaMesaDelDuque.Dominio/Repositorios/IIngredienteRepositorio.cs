using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IIngredienteRepositorio
{
    Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Ingrediente>> ObtenerTodosAsync(CancellationToken cancelacion = default);
}
