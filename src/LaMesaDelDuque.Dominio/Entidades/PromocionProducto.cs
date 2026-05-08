using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class PromocionProducto
{
    public Guid PromocionId { get; private set; }
    public Promocion Promocion { get; private set; }
    public Guid ProductoId { get; private set; }
    public Producto Producto { get; private set; }

    private PromocionProducto()
    {
        Promocion = null!;
        Producto = null!;
    }

    public PromocionProducto(Promocion promocion, Producto producto)
    {
        if (promocion is null)
            throw new ReglaDominioException("La promoción es obligatoria.");

        if (producto is null)
            throw new ReglaDominioException("El producto es obligatorio.");

        PromocionId = promocion.Id;
        Promocion = promocion;
        ProductoId = producto.Id;
        Producto = producto;
    }
}
