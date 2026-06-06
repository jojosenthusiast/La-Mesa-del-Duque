namespace LaMesaDelDuque.Aplicacion.Dtos;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string RolNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}
