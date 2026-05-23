using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ICierreServicio
{
    Task<CierreDiaDto?> ObtenerCierreHoyAsync(CancellationToken ct = default);
    Task<CierreDiaDto> AbrirCierreAsync(CancellationToken ct = default);
    Task<CierreDiaDto> CerrarDiaAsync(CierreCajaRequest req, CancellationToken ct = default);
    Task<List<CierreDiaDto>> HistorialAsync(CancellationToken ct = default);
}

public class CierreServicio : ICierreServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly IPedidosServicio _pedidos;
    private readonly IMermaServicio _merma;
    public CierreServicio(IUnidadDeTrabajo uot, IPedidosServicio pedidos, IMermaServicio merma)
    { _uot = uot; _pedidos = pedidos; _merma = merma; }

    public async Task<CierreDiaDto?> ObtenerCierreHoyAsync(CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var cierre = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, ct);
        return cierre is null ? null : Map(cierre);
    }

    public async Task<CierreDiaDto> AbrirCierreAsync(CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var existente = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, ct);
        if (existente is not null) return Map(existente);

        var cierre = new CierreDia(hoy, 0, 0, 0, 0, 0, 0, null!);
        await _uot.CierresDia.AgregarAsync(cierre, ct);
        await _uot.GuardarCambiosAsync(ct);
        return Map(cierre);
    }

    public async Task<CierreDiaDto> CerrarDiaAsync(CierreCajaRequest req, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var cierre = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, ct)
            ?? throw new ReglaDominioException("No hay cierre de día abierto.");

        // Calcular totales reales
        var mermas = await _merma.ObtenerMermasDelDiaAsync(ct);
        var totalMerma = mermas.Sum(m => m.Costo);
        var pedidos = await _pedidos.ListarPedidosActivosAsync();

        var cerrado = new CierreDia(hoy,
            pedidos.Where(p => p.Estado is "Pagado" or "EnCobro").Sum(p => p.Total),
            req.EfectivoReal, req.TarjetaReal, pedidos.Count(),
            pedidos.Count(p => p.Estado == "Cancelado"),
            totalMerma, null!);

        // Replace the open one
        await _uot.CierresDia.AgregarAsync(cerrado, ct);
        await _uot.GuardarCambiosAsync(ct);
        return Map(cerrado);
    }

    public async Task<List<CierreDiaDto>> HistorialAsync(CancellationToken ct = default)
    {
        var cierres = await _uot.CierresDia.ObtenerTodosAsync(ct);
        return cierres.Select(Map).ToList();
    }

    private static CierreDiaDto Map(CierreDia c) => new()
    {
        Id = c.Id, Fecha = c.Fecha, TotalVentas = c.TotalVentas,
        TotalEfectivo = c.TotalVentasEfectivo, TotalTarjeta = c.TotalVentasTarjeta,
        TotalPedidos = c.TotalPedidos, Cancelados = c.TotalPedidosCancelados,
        TotalMerma = c.TotalMermaValorizada
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
}

public class CierreCajaRequest
{
    public decimal EfectivoReal { get; set; }
    public decimal TarjetaReal { get; set; }
}
