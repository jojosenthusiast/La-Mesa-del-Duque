using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IDevolucionServicio
{
    Task<DevolucionPagoDto> ProcesarDevolucionAsync(
        Guid pagoOriginalId,
        decimal montoDevuelto,
        string metodoDevolucion,
        string motivo,
        Guid usuarioAutorizaId,
        CancellationToken ct = default);

    Task<List<DevolucionPagoDto>> ObtenerDelDiaAsync(DateOnly? fecha = null, CancellationToken ct = default);
    Task<List<DevolucionPagoDto>> ObtenerPorPagoAsync(Guid pagoOriginalId, CancellationToken ct = default);
}

public class DevolucionServicio : IDevolucionServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly IPedidosServicio _pedidosServicio;

    public DevolucionServicio(IUnidadDeTrabajo uot, IPedidosServicio pedidosServicio)
    {
        _uot = uot;
        _pedidosServicio = pedidosServicio;
    }

    public async Task<DevolucionPagoDto> ProcesarDevolucionAsync(
        Guid pagoOriginalId,
        decimal montoDevuelto,
        string metodoDevolucion,
        string motivo,
        Guid usuarioAutorizaId,
        CancellationToken ct = default)
    {
        var pago = await _uot.Pagos.ObtenerPorIdAsync(pagoOriginalId, ct)
            ?? throw new ReglaDominioException("El pago original no fue encontrado.");

        // Cargar cuenta con pedido
        var cuenta = await _uot.Cuentas.ObtenerPorIdAsync(pago.CuentaId, ct)
            ?? throw new ReglaDominioException("No se encontró la cuenta asociada al pago.");

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(cuenta.PedidoId, ct)
            ?? throw new ReglaDominioException("No se encontró el pedido asociado al pago.");

        if (pedido.Estado != EstadoPedido.Pagado && pedido.Estado != EstadoPedido.AnuladoPago)
            throw new ReglaDominioException("Solo se puede procesar la devolución de un pedido pagado.");

        if (montoDevuelto > pago.Monto)
            throw new ReglaDominioException($"El monto a devolver ({montoDevuelto:C}) no puede superar el monto del pago original ({pago.Monto:C}).");

        var devolucion = new DevolucionPago(
            pagoOriginalId,
            montoDevuelto,
            metodoDevolucion,
            motivo,
            usuarioAutorizaId,
            usuarioAutorizaId);

        // Devolución total: revertir estado del pedido + stock
        if (montoDevuelto == pago.Monto && pedido.Estado == EstadoPedido.Pagado)
        {
            await _pedidosServicio.AnularPagoAsync(pedido.Id, ct);
            devolucion.MarcarStockReintegrado();
        }

        await _uot.Devoluciones.AgregarAsync(devolucion, ct);

        // Registrar auditoría
        var usuarioAutoriza = await _uot.Usuarios.ObtenerPorIdAsync(usuarioAutorizaId, ct);
        if (usuarioAutoriza is not null)
        {
            var auditoria = new Auditoria(
                "DevolucionesPago",
                devolucion.Id,
                "INSERT",
                usuarioAutoriza,
                null,
                $"Devolución de {montoDevuelto:C} sobre pago {pagoOriginalId}. Motivo: {motivo}");
            await _uot.Auditorias.AgregarAsync(auditoria, ct);
        }

        await _uot.GuardarCambiosAsync(ct);

        return Map(devolucion);
    }

    public async Task<List<DevolucionPagoDto>> ObtenerDelDiaAsync(DateOnly? fecha = null, CancellationToken ct = default)
    {
        var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var lista = await _uot.Devoluciones.ObtenerPorFechaAsync(fechaConsulta, ct);
        return lista.Select(Map).ToList();
    }

    public async Task<List<DevolucionPagoDto>> ObtenerPorPagoAsync(Guid pagoOriginalId, CancellationToken ct = default)
    {
        var lista = await _uot.Devoluciones.ObtenerPorPagoAsync(pagoOriginalId, ct);
        return lista.Select(Map).ToList();
    }

    private static DevolucionPagoDto Map(DevolucionPago d) => new()
    {
        Id = d.Id,
        PagoOriginalId = d.PagoOriginalId,
        MontoDevuelto = d.MontoDevuelto,
        MetodoDevolucion = d.MetodoDevolucion,
        MotivoDevolucion = d.MotivoDevolucion,
        UsuarioSolicitaId = d.UsuarioSolicitaId,
        UsuarioAutorizaId = d.UsuarioAutorizaId,
        FechaHora = d.FechaHora,
        StockReintegrado = d.StockReintegrado,
    };
}
