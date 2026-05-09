using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class UsuariosServicio : IUsuariosServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public UsuariosServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<UsuarioDto> CrearUsuarioAsync(string username, string? email, string password, string nombreCompleto, Guid rolId, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.", nameof(password));

        var existente = await _uot.Usuarios.ObtenerPorUsernameAsync(username, cancelacion);
        if (existente is not null)
            throw new InvalidOperationException($"Ya existe un usuario con el nombre '{username}'.");

        var rol = await _uot.Roles.ObtenerPorIdAsync(rolId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el rol con ID {rolId}.", nameof(rolId));

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var usuario = new Usuario(username, email, passwordHash, nombreCompleto, rol);

        await _uot.Usuarios.AgregarAsync(usuario, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(usuario);
    }

    public async Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken cancelacion = default)
    {
        var usuarios = await _uot.Usuarios.ObtenerTodosAsync(cancelacion);
        return usuarios.Select(MapToDto).ToList();
    }

    public async Task DesactivarUsuarioAsync(Guid usuarioId, CancellationToken cancelacion = default)
    {
        var usuario = await _uot.Usuarios.ObtenerPorIdAsync(usuarioId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el usuario con ID {usuarioId}.", nameof(usuarioId));

        usuario.Desactivar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    public async Task<UsuarioDto?> ValidarCredencialesAsync(string username, string password, CancellationToken cancelacion = default)
    {
        var usuario = await _uot.Usuarios.ObtenerPorUsernameAsync(username, cancelacion);
        if (usuario is null) return null;
        if (!usuario.Activo) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash)) return null;

        usuario.ActualizarUltimoAcceso();
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(usuario);
    }

    private static UsuarioDto MapToDto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Username = usuario.Username,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            RolNombre = usuario.Rol.Nombre,
            Activo = usuario.Activo,
            UltimoAcceso = usuario.UltimoAcceso
        };
    }
}
