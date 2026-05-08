using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class ProductoIngrediente
{
    public Guid ProductoId { get; private set; }
    public Producto Producto { get; private set; }
    public Guid IngredienteId { get; private set; }
    public Ingrediente Ingrediente { get; private set; }
    public decimal CantidadRequerida { get; private set; }

    private ProductoIngrediente()
    {
        Producto = null!;
        Ingrediente = null!;
    }

    public ProductoIngrediente(Producto producto, Ingrediente ingrediente, decimal cantidadRequerida)
    {
        if (producto is null)
            throw new ReglaDominioException("El producto es obligatorio en la receta.");

        if (ingrediente is null)
            throw new ReglaDominioException("El ingrediente es obligatorio en la receta.");

        if (cantidadRequerida <= 0)
            throw new ReglaDominioException("La cantidad requerida debe ser mayor que cero.");

        Producto = producto;
        ProductoId = producto.Id;
        Ingrediente = ingrediente;
        IngredienteId = ingrediente.Id;
        CantidadRequerida = cantidadRequerida;
    }
}
