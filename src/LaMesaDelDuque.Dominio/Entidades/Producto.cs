using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public CategoriaProducto Categoria { get; private set; }
    public bool Activo { get; private set; }
    public string? Descripcion { get; private set; }

    private Producto()
    {
        Nombre = string.Empty;
        Categoria = null!;
    }

    public Producto(string nombre, decimal precio, CategoriaProducto categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del producto es obligatorio.");

        if (precio < 0)
            throw new ReglaDominioException("El precio no puede ser negativo.");

        if (categoria is null)
            throw new ReglaDominioException("El producto debe pertenecer a una categoría.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Precio = precio;
        Categoria = categoria;
        Activo = true;
    }

    public void ActualizarDatos(string nombre, decimal precio, CategoriaProducto categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del producto es obligatorio.");

        if (precio <= 0)
            throw new ReglaDominioException("El precio debe ser mayor que cero.");

        if (categoria is null)
            throw new ReglaDominioException("El producto debe pertenecer a una categoría.");

        Nombre = nombre.Trim();
        Precio = precio;
        Categoria = categoria;
    }

    public void ActualizarDescripcion(string? descripcion)
    {
        Descripcion = descripcion;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }
}
