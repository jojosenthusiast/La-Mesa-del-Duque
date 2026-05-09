using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Usuario?> ObtenerPorUsernameAsync(string username, CancellationToken cancelacion = default);
    Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(Usuario usuario, CancellationToken cancelacion = default);
}
