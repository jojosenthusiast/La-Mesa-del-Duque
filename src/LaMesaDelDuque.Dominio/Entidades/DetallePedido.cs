using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class DetallePedido
{
    public Guid Id { get; private set; }
    public Producto Producto { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;

    private DetallePedido()
    {
        Producto = null!;
    }

    public DetallePedido(Producto producto, int cantidad, decimal precioUnitario)
    {
        if (producto is null)
            throw new ReglaDominioException("El detalle debe tener un producto asociado.");

        if (cantidad <= 0)
            throw new ReglaDominioException("La cantidad debe ser mayor que cero.");

        if (precioUnitario < 0)
            throw new ReglaDominioException("El precio unitario no puede ser negativo.");

        Producto = producto;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }

    public void ActualizarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            throw new ReglaDominioException("La cantidad debe ser mayor que cero.");

        Cantidad = nuevaCantidad;
    }
}
