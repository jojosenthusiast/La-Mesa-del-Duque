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
    Task<List<PersonalJornadaDto>> PersonalDeLaJornadaAsync(DateOnly fecha, CancellationToken ct = default);
    Task<List<CanalVentaResumenDto>> TotalesPorCanalAsync(DateOnly fecha, CancellationToken ct = default);
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
        // Puede existir un cierre de hoy ya cerrado (la fecha es única). Si es así,
        // se reabre en lugar de intentar insertar otro (que rompería el índice único).
        var existente = await _uot.CierresDia.ObtenerPorFechaAsync(hoy, ct);
        if (existente is not null)
        {
            if (existente.EsCerrado)
            {
                existente.Reabrir();
                await _uot.GuardarCambiosAsync(ct);
            }
            return Map(existente);
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

        // ── Totales reales del sistema ──────────────────────────
        decimal totalVentasEfectivo = 0;
        decimal totalVentasTarjeta = 0;
        int totalPedidos = 0;
        int totalCancelados = 0;

        try
        {
            var pagosHoy = await _uot.Pagos.ObtenerDelDiaAsync(hoy, ct);
            totalVentasEfectivo = pagosHoy
                .Where(p => p.Metodo == MetodoPago.Efectivo)
                .Sum(p => p.Monto);
            totalVentasTarjeta = pagosHoy
                .Where(p => p.Metodo != MetodoPago.Efectivo)
                .Sum(p => p.Monto);
        }
        catch
        {
            // Fallback: si no hay pagos o la tabla falla, usar lo que declara el usuario
            totalVentasEfectivo = req.EfectivoReal;
            totalVentasTarjeta = req.TarjetaReal;
        }

        try
        {
            totalPedidos = await _uot.Pedidos.ContarDelDiaAsync(hoy, ct);
            totalCancelados = await _uot.Pedidos.ContarCanceladosDelDiaAsync(hoy, ct);
        }
        catch
        {
            // Fallback: si no se puede consultar pedidos, dejamos en 0
        }

        decimal totalMerma = 0;
        try
        {
            var mermas = await _merma.ObtenerMermasDelDiaAsync(ct);
            totalMerma = mermas.Sum(m => m.Costo);
        }
        catch
        {
            // Fallback: si falla la consulta de mermas, asumimos 0
        }

        var totalVentas = totalVentasEfectivo + totalVentasTarjeta;

        cierre.Cerrar(totalVentas, totalVentasEfectivo, totalVentasTarjeta,
            totalPedidos, totalCancelados, totalMerma,
            req.EfectivoReal, req.TarjetaReal, req.Observacion);
        await _uot.GuardarCambiosAsync(ct);
        return Map(cierre);
    }

    public async Task<List<CierreDiaDto>> HistorialAsync(CancellationToken ct = default)
    {
        var cierres = await _uot.CierresDia.ObtenerTodosAsync(ct);
        return cierres.Select(Map).ToList();
    }

    public async Task<List<PersonalJornadaDto>> PersonalDeLaJornadaAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var usuarios = await _uot.Usuarios.ObtenerTodosAsync(ct);
        var turnos = await _uot.TurnosCaja.ObtenerHistorialAsync(1, 200, ct);
        var turnosDelDia = turnos
            .Where(t => DateOnly.FromDateTime(t.FechaApertura) == fecha)
            .ToList();

        var lista = new List<PersonalJornadaDto>();
        foreach (var u in usuarios)
        {
            var turno = turnosDelDia.FirstOrDefault(t => t.CajeroId == u.Id);
            var accedioHoy = u.UltimoAcceso.HasValue && DateOnly.FromDateTime(u.UltimoAcceso.Value) == fecha;
            if (turno is null && !accedioHoy)
                continue;

            lista.Add(new PersonalJornadaDto
            {
                NombreCompleto = u.NombreCompleto,
                Rol = u.Rol?.Nombre ?? "—",
                Entrada = turno?.FechaApertura ?? u.UltimoAcceso,
                Salida = turno?.FechaCierre,
                AbrioCaja = turno is not null
            });
        }

        return lista
            .OrderBy(p => p.Rol)
            .ThenBy(p => p.NombreCompleto)
            .ToList();
    }


    public async Task<List<CanalVentaResumenDto>> TotalesPorCanalAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var pedidos = await _uot.Pedidos.ObtenerTodosAsync(ct);
        var baseDia = pedidos
            .Where(p => DateOnly.FromDateTime(p.FechaCreacion) == fecha)
            .Where(p => p.Estado != EstadoPedido.Cancelado && p.Estado != EstadoPedido.AnuladoPago)
            .ToList();

        return Enum.GetValues<TipoServicio>()
            .Select(tipo =>
            {
                var grupo = baseDia.Where(p => p.TipoServicio == tipo).ToList();
                return new CanalVentaResumenDto
                {
                    TipoServicio = tipo.ToString(),
                    Nombre = tipo switch
                    {
                        TipoServicio.ComerAqui => "Comer aquí",
                        TipoServicio.ParaLlevar => "Para llevar",
                        TipoServicio.Domicilio => "Delivery",
                        _ => tipo.ToString()
                    },
                    Pedidos = grupo.Count,
                    Pagados = grupo.Count(p => p.EstaPagadoCompletamente || p.Estado is EstadoPedido.Pagado or EstadoPedido.Despachado),
                    TotalVendido = grupo
                        .Where(p => p.EstaPagadoCompletamente || p.Estado is EstadoPedido.Pagado or EstadoPedido.Despachado)
                        .Sum(p => p.Total)
                };
            })
            .OrderBy(c => c.Nombre)
            .ToList();
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
        CerradoEn = c.CerradoEn,
        Observacion = c.Observacion
    };
}


public class CanalVentaResumenDto
{
    public string TipoServicio { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public int Pagados { get; set; }
    public decimal TotalVendido { get; set; }
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
}

public class CierreCajaRequest
{
    public decimal EfectivoReal { get; set; }
    public decimal TarjetaReal { get; set; }
    public string? Observacion { get; set; }
}

public class PersonalJornadaDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime? Entrada { get; set; }
    public DateTime? Salida { get; set; }
    public bool AbrioCaja { get; set; }
}
