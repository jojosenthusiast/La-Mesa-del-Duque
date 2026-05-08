using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class OrdenCompraDetalle
{
    public Guid Id { get; private set; }
    public Guid OrdenCompraId { get; private set; }
    public OrdenCompra OrdenCompra { get; private set; }
    public Guid IngredienteId { get; private set; }
    public Ingrediente Ingrediente { get; private set; }
    public decimal CantidadSolicitada { get; private set; }
    public decimal? CantidadRecibida { get; private set; }
    public decimal PrecioUnitario { get; private set; }

    private OrdenCompraDetalle()
    {
        OrdenCompra = null!;
        Ingrediente = null!;
    }

    public OrdenCompraDetalle(
        OrdenCompra ordenCompra,
        Ingrediente ingrediente,
        decimal cantidadSolicitada,
        decimal precioUnitario)
    {
        if (ordenCompra is null)
            throw new ReglaDominioException("La orden de compra es obligatoria.");

        if (ingrediente is null)
            throw new ReglaDominioException("El ingrediente es obligatorio.");

        if (cantidadSolicitada <= 0)
            throw new ReglaDominioException("La cantidad solicitada debe ser mayor que cero.");

        if (precioUnitario < 0)
            throw new ReglaDominioException("El precio unitario no puede ser negativo.");

        Id = Guid.NewGuid();
        OrdenCompra = ordenCompra;
        OrdenCompraId = ordenCompra.Id;
        Ingrediente = ingrediente;
        IngredienteId = ingrediente.Id;
        CantidadSolicitada = cantidadSolicitada;
        PrecioUnitario = precioUnitario;
    }

    public void RegistrarRecepcion(decimal cantidadRecibida)
    {
        if (cantidadRecibida < 0)
            throw new ReglaDominioException("La cantidad recibida no puede ser negativa.");

        CantidadRecibida = cantidadRecibida;
    }
}
