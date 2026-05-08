using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class CategoriaProducto
{
    private const int LongitudMaximaNombre = 100;
    private const int LongitudMaximaDescripcion = 250;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public int OrdenDisplay { get; private set; }
    public bool Activo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CategoriaProducto()
    {
        Nombre = string.Empty;
    }

    public CategoriaProducto(string nombre, string? descripcion = null, int ordenDisplay = 0)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre de la categoría es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre de la categoría no puede exceder {LongitudMaximaNombre} caracteres.");

        if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > LongitudMaximaDescripcion)
            throw new ReglaDominioException($"La descripción de la categoría no puede exceder {LongitudMaximaDescripcion} caracteres.");

        if (ordenDisplay < 0)
            throw new ReglaDominioException("El orden de visualización no puede ser negativo.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        OrdenDisplay = ordenDisplay;
        Activo = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void ActualizarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre de la categoría es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre de la categoría no puede exceder {LongitudMaximaNombre} caracteres.");

        Nombre = nombre.Trim();
    }

    public void ActualizarPresentacion(string? descripcion, int ordenDisplay)
    {
        if (ordenDisplay < 0)
            throw new ReglaDominioException("El orden de visualización no puede ser negativo.");

        if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > LongitudMaximaDescripcion)
            throw new ReglaDominioException($"La descripción de la categoría no puede exceder {LongitudMaximaDescripcion} caracteres.");

        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        OrdenDisplay = ordenDisplay;
    }
}
