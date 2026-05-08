using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class ComboProducto
{
    public Guid ComboId { get; private set; }
    public Combo Combo { get; private set; }
    public Guid ProductoId { get; private set; }
    public Producto Producto { get; private set; }
    public int Cantidad { get; private set; }

    private ComboProducto()
    {
        Combo = null!;
        Producto = null!;
    }

    public ComboProducto(Combo combo, Producto producto, int cantidad = 1)
    {
        if (combo is null)
            throw new ReglaDominioException("El combo es obligatorio.");

        if (producto is null)
            throw new ReglaDominioException("El producto es obligatorio.");

        if (cantidad <= 0)
            throw new ReglaDominioException("La cantidad del producto en el combo debe ser mayor que cero.");

        ComboId = combo.Id;
        Combo = combo;
        ProductoId = producto.Id;
        Producto = producto;
        Cantidad = cantidad;
    }
}
