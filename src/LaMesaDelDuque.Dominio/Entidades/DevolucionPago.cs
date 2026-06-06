using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class DevolucionPago
{
    public Guid Id { get; private set; }
    public Guid PagoOriginalId { get; private set; }
    public Pago PagoOriginal { get; private set; } = null!;
    public decimal MontoDevuelto { get; private set; }
    public string MetodoDevolucion { get; private set; }
    public string MotivoDevolucion { get; private set; }
    public Guid UsuarioSolicitaId { get; private set; }
    public Guid UsuarioAutorizaId { get; private set; }
    public DateTime FechaHora { get; private set; }
    public bool StockReintegrado { get; private set; }

    private DevolucionPago() { MetodoDevolucion = string.Empty; MotivoDevolucion = string.Empty; }

    public DevolucionPago(
        Guid pagoOriginalId,
        decimal montoDevuelto,
        string metodoDevolucion,
        string motivoDevolucion,
        Guid usuarioSolicitaId,
        Guid usuarioAutorizaId)
    {
        if (montoDevuelto <= 0) throw new ReglaDominioException("El monto a devolver debe ser positivo.");
        if (string.IsNullOrWhiteSpace(motivoDevolucion)) throw new ReglaDominioException("El motivo de la devolución es obligatorio.");
        if (string.IsNullOrWhiteSpace(metodoDevolucion)) throw new ReglaDominioException("El método de devolución es obligatorio.");
        Id = Guid.NewGuid();
        PagoOriginalId = pagoOriginalId;
        MontoDevuelto = montoDevuelto;
        MetodoDevolucion = metodoDevolucion.Trim().ToLower();
        MotivoDevolucion = motivoDevolucion.Trim();
        UsuarioSolicitaId = usuarioSolicitaId;
        UsuarioAutorizaId = usuarioAutorizaId;
        FechaHora = DateTime.UtcNow;
        StockReintegrado = false;
    }

    public void MarcarStockReintegrado() => StockReintegrado = true;
}
