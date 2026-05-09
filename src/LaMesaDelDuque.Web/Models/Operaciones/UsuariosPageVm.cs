using System.ComponentModel.DataAnnotations;
using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Web.Models.Operaciones;

public class UsuariosPageVm
{
    public List<UsuarioDto> Usuarios { get; set; } = [];
    public UsuarioFormVm Form { get; set; } = new();
}

public class UsuarioFormVm
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public Guid RolId { get; set; }
}
