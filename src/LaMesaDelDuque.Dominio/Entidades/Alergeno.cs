namespace LaMesaDelDuque.Dominio.Entidades;

/// <summary>
/// Alérgeno registrable en el sistema.
/// Puede asociarse a ingredientes (alergia directa) o a productos (cross-contaminación).
/// Ej: Mariscos, Lácteos, Maní, Gluten, Soja, Sulfitos, Aceite de pescado.
/// </summary>
public class Alergeno
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Icono { get; private set; }
    public bool Activo { get; private set; }

    private Alergeno()
    {
        Nombre = string.Empty;
    }

    public Alergeno(string nombre, string? icono = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del alérgeno es obligatorio.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Icono = icono;
        Activo = true;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
