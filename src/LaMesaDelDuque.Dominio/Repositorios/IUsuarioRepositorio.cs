using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
}
