using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Permiso
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Modulo { get; private set; }
    public string? Descripcion { get; private set; }

    private Permiso()
    {
        Nombre = string.Empty;
        Modulo = string.Empty;
    }

    public Permiso(string nombre, string modulo, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del permiso es obligatorio.");
        if (nombre.Trim().Length > 100)
            throw new ReglaDominioException("El nombre del permiso no puede exceder 100 caracteres.");
        if (string.IsNullOrWhiteSpace(modulo))
            throw new ReglaDominioException("El módulo del permiso es obligatorio.");
        if (modulo.Trim().Length > 50)
            throw new ReglaDominioException("El módulo del permiso no puede exceder 50 caracteres.");
        if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > 250)
            throw new ReglaDominioException("La descripción del permiso no puede exceder 250 caracteres.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Modulo = modulo.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
    }
}
