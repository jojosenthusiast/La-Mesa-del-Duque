using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Rol
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Rol() => Nombre = string.Empty;

    public Rol(string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del rol es obligatorio.");
        if (nombre.Trim().Length > 50)
            throw new ReglaDominioException("El nombre del rol no puede exceder 50 caracteres.");
        if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > 250)
            throw new ReglaDominioException("La descripción del rol no puede exceder 250 caracteres.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        Activo = true;
        CreatedAt = DateTime.UtcNow;
    }
}
