using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class CategoriaProducto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }

    private CategoriaProducto()
    {
        Nombre = string.Empty;
    }

    public CategoriaProducto(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre de la categoría es obligatorio.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Activo = true;
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

        Nombre = nombre.Trim();
    }
}
