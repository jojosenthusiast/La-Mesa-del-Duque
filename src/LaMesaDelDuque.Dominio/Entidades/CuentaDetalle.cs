using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class CuentaDetalle
{
    public Guid Id { get; private set; }
    public Guid CuentaId { get; private set; }
    public Guid DetallePedidoId { get; private set; }
    public int CantidadAsignada { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Subtotal => CantidadAsignada * PrecioUnitario;

    private CuentaDetalle() { } // EF Core

    public CuentaDetalle(Guid cuentaId, Guid detallePedidoId, int cantidadAsignada, decimal precioUnitario)
    {
        if (cuentaId == Guid.Empty)
            throw new ReglaDominioException("El ID de cuenta no puede estar vacío.");

        if (detallePedidoId == Guid.Empty)
            throw new ReglaDominioException("El ID de detalle no puede estar vacío.");

        if (cantidadAsignada <= 0)
            throw new ReglaDominioException("La cantidad asignada debe ser mayor que cero.");

        if (precioUnitario < 0)
            throw new ReglaDominioException("El precio unitario no puede ser negativo.");

        Id = Guid.NewGuid();
        CuentaId = cuentaId;
        DetallePedidoId = detallePedidoId;
        CantidadAsignada = cantidadAsignada;
        PrecioUnitario = precioUnitario;
    }
}
