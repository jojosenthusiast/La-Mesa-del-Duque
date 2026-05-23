using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ICierreServicio
{
    Task<CierreDiaDto?> ObtenerCierreHoyAsync(CancellationToken ct = default);
    Task<CierreDiaDto> AbrirCierreAsync(Guid usuarioId, CancellationToken ct = default);
    Task<CierreDiaDto> CerrarDiaAsync(CierreCajaRequest req, Guid usuarioId, CancellationToken ct = default);
    Task<List<CierreDiaDto>> HistorialAsync(CancellationToken ct = default);
}

public class CierreServicio : ICierreServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly IMermaServicio _merma;

    public CierreServicio(IUnidadDeTrabajo uot, IMermaServicio merma)
    {
        _uot = uot;
        _merma = merma;
    }

    public async Task<CierreDiaDto?> ObtenerCierreHoyAsync(CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var cierre = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, ct);
        return cierre is null ? null : Map(cierre);
    }

    public async Task<CierreDiaDto> AbrirCierreAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var existente = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, ct);
        if (existente is not null) return Map(existente);

        var usuario = await _uot.Usuarios.ObtenerPorIdAsync(usuarioId, ct)
            ?? throw new ReglaDominioException("Usuario no encontrado para abrir el cierre.");

        var cierre = new CierreDia(hoy, 0, 0, 0, 0, 0, 0, usuario);
        await _uot.CierresDia.AgregarAsync(cierre, ct);
        await _uot.GuardarCambiosAsync(ct);
        return Map(cierre);
    }

    public async Task<CierreDiaDto> CerrarDiaAsync(CierreCajaRequest req, Guid usuarioId, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var cierre = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, ct)
            ?? throw new ReglaDominioException("No hay cierre de día abierto.");

        // Calcular totales desde pagos reales del día
        var pagosHoy = await _uot.Pagos.ObtenerDelDiaAsync(hoy, ct);
        var totalVentas = pagosHoy.Sum(p => p.Monto);
        var totalEfectivo = pagosHoy.Where(p => p.Metodo == MetodoPago.Efectivo).Sum(p => p.Monto);
        var totalTarjeta = pagosHoy.Where(p => p.Metodo != MetodoPago.Efectivo).Sum(p => p.Monto);

        var mermas = await _merma.ObtenerMermasDelDiaAsync(ct);
        var totalMerma = mermas.Sum(m => m.Costo);

        // Contar pedidos pagados y cancelados desde la tabla Pedido por EstadosLog
        // Aproximación pragmática: contar desde cuentas con pagos del día
        var cuentaIds = pagosHoy.Select(p => p.CuentaId).Distinct().ToList();
        var totalPedidos = cuentaIds.Count;
        var totalCancelados = await _uot.Pedidos.ContarCanceladosDelDiaAsync(hoy, ct);

        cierre.Cerrar(totalVentas, totalEfectivo, totalTarjeta, totalPedidos, totalCancelados, totalMerma, req.EfectivoReal, req.TarjetaReal);
        await _uot.GuardarCambiosAsync(ct);
        return Map(cierre);
    }

    public async Task<List<CierreDiaDto>> HistorialAsync(CancellationToken ct = default)
    {
        var cierres = await _uot.CierresDia.ObtenerTodosAsync(ct);
        return cierres.Select(Map).ToList();
    }

    private static CierreDiaDto Map(CierreDia c) => new()
    {
        Id = c.Id,
        Fecha = c.Fecha,
        TotalVentas = c.TotalVentas,
        TotalEfectivo = c.TotalVentasEfectivo,
        TotalTarjeta = c.TotalVentasTarjeta,
        TotalPedidos = c.TotalPedidos,
        Cancelados = c.TotalPedidosCancelados,
        TotalMerma = c.TotalMermaValorizada,
        EfectivoReal = c.EfectivoReal,
        TarjetaReal = c.TarjetaReal,
        DiferenciaEfectivo = c.DiferenciaEfectivo,
        DiferenciaTarjeta = c.DiferenciaTarjeta,
        EsCerrado = c.EsCerrado,
        CerradoEn = c.CerradoEn
    };
}

public class CierreDiaDto
{
    public Guid Id { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalTarjeta { get; set; }
    public int TotalPedidos { get; set; }
    public int Cancelados { get; set; }
    public decimal TotalMerma { get; set; }
    public decimal EfectivoReal { get; set; }
    public decimal TarjetaReal { get; set; }
    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaTarjeta { get; set; }
    public bool EsCerrado { get; set; }
    public DateTime? CerradoEn { get; set; }
}

public class CierreCajaRequest
{
    public decimal EfectivoReal { get; set; }
    public decimal TarjetaReal { get; set; }
}
