using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IUsuariosServicio
{
    Task<UsuarioDto> CrearUsuarioAsync(string username, string? email, string password, string nombreCompleto, Guid rolId, CancellationToken cancelacion = default);
    Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken cancelacion = default);
    Task DesactivarUsuarioAsync(Guid usuarioId, CancellationToken cancelacion = default);
    Task<UsuarioDto?> ValidarCredencialesAsync(string username, string password, CancellationToken cancelacion = default);
}
