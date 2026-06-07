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
        return cierre is null ? null : await MapConTotalesEnVivoSiAbiertoAsync(cierre, ct);
    }

    public async Task<CierreDiaDto> AbrirCierreAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var existente = await _uot.CierresDia.ObtenerPorFechaAsync(hoy, ct);
        if (existente is not null)
        {
            if (existente.EsCerrado)
                throw new ReglaDominioException("El día operativo de hoy ya fue cerrado y no puede reabrirse.");

            return await MapConTotalesEnVivoSiAbiertoAsync(existente, ct);
        }

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

        var turnoActivo = await _uot.TurnosCaja.ObtenerTurnoActivoAsync(ct);
        if (turnoActivo is not null)
            throw new ReglaDominioException("Hay un turno de caja activo. Cierre el turno de caja antes de cerrar el día operativo.");

        var totales = await CalcularTotalesSistemaAsync(hoy, ct);

        cierre.Cerrar(totales.TotalVentas, totales.TotalEfectivo, totales.TotalTarjeta,
            totales.TotalPedidos, totales.TotalCancelados, totales.TotalMerma,
            req.EfectivoReal, req.TarjetaReal, req.Observacion);
        await _uot.GuardarCambiosAsync(ct);
        return Map(cierre);
    }

    public async Task<List<CierreDiaDto>> HistorialAsync(CancellationToken ct = default)
    {
        var cierres = await _uot.CierresDia.ObtenerTodosAsync(ct);
        var resultado = new List<CierreDiaDto>(cierres.Count);

        foreach (var cierre in cierres)
        {
            resultado.Add(await MapConTotalesEnVivoSiAbiertoAsync(cierre, ct));
        }

        return resultado;
    }

    private async Task<CierreDiaDto> MapConTotalesEnVivoSiAbiertoAsync(CierreDia cierre, CancellationToken ct)
    {
        if (cierre.EsCerrado)
            return Map(cierre);

        var totales = await CalcularTotalesSistemaAsync(cierre.Fecha, ct);
        return Map(cierre, totales);
    }

    private async Task<TotalesSistemaCierre> CalcularTotalesSistemaAsync(DateOnly fecha, CancellationToken ct)
    {
        var pagos = await _uot.Pagos.ObtenerDelDiaAsync(fecha, ct);
        var totalEfectivo = pagos
            .Where(p => p.Metodo == MetodoPago.Efectivo)
            .Sum(p => p.Monto);
        var totalNoEfectivo = pagos
            .Where(p => p.Metodo != MetodoPago.Efectivo)
            .Sum(p => p.Monto);
        var totalVentas = totalEfectivo + totalNoEfectivo;

        var totalPedidos = await _uot.Pedidos.ContarPagadosDelDiaAsync(fecha, ct);
        var totalCancelados = await _uot.Pedidos.ContarCanceladosDelDiaAsync(fecha, ct);
        var totalMerma = 0m;

        if (fecha == DateOnly.FromDateTime(DateTime.UtcNow))
        {
            var mermas = await _merma.ObtenerMermasDelDiaAsync(ct);
            totalMerma = mermas.Sum(m => m.Costo);
        }

        return new TotalesSistemaCierre(totalVentas, totalEfectivo, totalNoEfectivo, totalPedidos, totalCancelados, totalMerma);
    }

    private static CierreDiaDto Map(CierreDia c, TotalesSistemaCierre? totales = null) => new()
    {
        Id = c.Id,
        Fecha = c.Fecha,
        TotalVentas = totales?.TotalVentas ?? c.TotalVentas,
        TotalEfectivo = totales?.TotalEfectivo ?? c.TotalVentasEfectivo,
        TotalTarjeta = totales?.TotalTarjeta ?? c.TotalVentasTarjeta,
        TotalPedidos = totales?.TotalPedidos ?? c.TotalPedidos,
        Cancelados = totales?.TotalCancelados ?? c.TotalPedidosCancelados,
        TotalMerma = totales?.TotalMerma ?? c.TotalMermaValorizada,
        EfectivoReal = c.EfectivoReal,
        TarjetaReal = c.TarjetaReal,
        DiferenciaEfectivo = c.DiferenciaEfectivo,
        DiferenciaTarjeta = c.DiferenciaTarjeta,
        EsCerrado = c.EsCerrado,
        CerradoEn = c.CerradoEn,
        Observacion = c.Observacion,
        UsuarioNombre = c.Usuario?.NombreCompleto ?? (c.UsuarioId == Guid.Empty ? "-" : c.UsuarioId.ToString())
    };

    private sealed record TotalesSistemaCierre(
        decimal TotalVentas,
        decimal TotalEfectivo,
        decimal TotalTarjeta,
        int TotalPedidos,
        int TotalCancelados,
        decimal TotalMerma);
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
    public string? Observacion { get; set; }
    public string UsuarioNombre { get; set; } = "-";
}

public class CierreCajaRequest
{
    public decimal EfectivoReal { get; set; }
    public decimal TarjetaReal { get; set; }
    public string? Observacion { get; set; }
}
