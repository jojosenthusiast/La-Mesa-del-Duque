using System.Text.Json;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class DetallePedido
{
    public Guid Id { get; private set; }
    public Producto Producto { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal PrecioOriginal { get; private set; }
    public decimal DescuentoAplicado { get; private set; }
    public string? PromocionNombre { get; private set; }
    public string? Notas { get; private set; }
    public string? ModificacionesJson { get; private set; }
    public decimal Subtotal => Cantidad * (PrecioUnitario - DescuentoAplicado);

    private DetallePedido()
    {
        Producto = null!;
    }

    public DetallePedido(Producto producto, int cantidad, decimal precioUnitario, string? notas = null, string? modificacionesJson = null)
    {
        if (producto is null)
            throw new ReglaDominioException("El detalle debe tener un producto asociado.");

        if (cantidad <= 0)
            throw new ReglaDominioException("La cantidad debe ser mayor que cero.");

        if (precioUnitario < 0)
            throw new ReglaDominioException("El precio unitario no puede ser negativo.");

        Id = Guid.NewGuid();
        Producto = producto;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        PrecioOriginal = precioUnitario;
        DescuentoAplicado = 0;
        Notas = notas;
        ModificacionesJson = modificacionesJson;
    }

    public void AplicarPromocion(decimal descuentoMonto, string nombrePromo)
    {
        if (descuentoMonto < 0)
            throw new ReglaDominioException("El monto de descuento no puede ser negativo.");

        DescuentoAplicado = descuentoMonto;
        PromocionNombre = nombrePromo;
    }

    public void ActualizarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            throw new ReglaDominioException("La cantidad debe ser mayor que cero.");

        Cantidad = nuevaCantidad;
    }

    public void GuardarModificaciones(List<ModificacionIngrediente> modificaciones)
    {
        if (modificaciones is null || modificaciones.Count == 0)
        {
            ModificacionesJson = null;
            return;
        }

        ModificacionesJson = JsonSerializer.Serialize(modificaciones, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public List<ModificacionIngrediente> ObtenerModificaciones()
    {
        if (string.IsNullOrWhiteSpace(ModificacionesJson))
            return [];

        return JsonSerializer.Deserialize<List<ModificacionIngrediente>>(ModificacionesJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? [];
    }
}
