using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public CategoriaProducto Categoria { get; private set; }
    public bool Activo { get; private set; }

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

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }
}
