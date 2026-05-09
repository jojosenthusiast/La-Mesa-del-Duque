using System.Text.RegularExpressions;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string? Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string NombreCompleto { get; private set; }
    public Guid RolId { get; private set; }
    public Rol Rol { get; private set; }
    public bool Activo { get; private set; }
    public DateTime? UltimoAcceso { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Usuario()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
        NombreCompleto = string.Empty;
        Rol = null!;
    }

    public Usuario(string username, string? email, string passwordHash, string nombreCompleto, Rol rol)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ReglaDominioException("El username es obligatorio.");
        if (username.Trim().Length > 50)
            throw new ReglaDominioException("El username no puede exceder 50 caracteres.");
        if (!string.IsNullOrWhiteSpace(email) && email.Trim().Length > 150)
            throw new ReglaDominioException("El email no puede exceder 150 caracteres.");
        if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ReglaDominioException("El email tiene un formato inválido.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ReglaDominioException("El password hash es obligatorio.");
        if (passwordHash.Trim().Length > 255)
            throw new ReglaDominioException("El password hash no puede exceder 255 caracteres.");
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ReglaDominioException("El nombre completo es obligatorio.");
        if (nombreCompleto.Trim().Length > 200)
            throw new ReglaDominioException("El nombre completo no puede exceder 200 caracteres.");
        if (rol is null)
            throw new ReglaDominioException("El rol del usuario es obligatorio.");

        Id = Guid.NewGuid();
        Username = username.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        PasswordHash = passwordHash.Trim();
        NombreCompleto = nombreCompleto.Trim();
        Rol = rol;
        RolId = rol.Id;
        Activo = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activar()
    {
        Activo = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CambiarRol(Rol nuevoRol)
    {
        if (nuevoRol is null)
            throw new ReglaDominioException("El rol del usuario es obligatorio.");

        Rol = nuevoRol;
        RolId = nuevoRol.Id;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarUltimoAcceso()
    {
        UltimoAcceso = DateTime.UtcNow;
    }

    public void CambiarPasswordHash(string nuevoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            throw new ReglaDominioException("El password hash es obligatorio.");

        PasswordHash = nuevoPasswordHash.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
