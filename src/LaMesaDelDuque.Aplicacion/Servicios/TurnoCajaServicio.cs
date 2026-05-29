using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ITurnoCajaServicio
{
    Task<TurnoCajaDto> AbrirTurnoAsync(Guid cajeroId, decimal fondoInicial, CancellationToken ct = default);
    Task<TurnoCajaDto> CerrarTurnoAsync(Guid turnoId, decimal efectivoContado, string? observacion, CancellationToken ct = default);
    Task<TurnoCajaDto?> ObtenerTurnoActivoAsync(CancellationToken ct = default);
    Task<List<TurnoCajaDto>> ObtenerHistorialAsync(int pagina = 1, int porPagina = 20, CancellationToken ct = default);
    Task RegistrarMovimientoAsync(Guid turnoId, string tipo, decimal monto, string motivo, Guid usuarioId, CancellationToken ct = default);
    Task<ReporteZDto> GenerarReporteZAsync(Guid turnoId, CancellationToken ct = default);
}

public class TurnoCajaServicio : ITurnoCajaServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public TurnoCajaServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<TurnoCajaDto> AbrirTurnoAsync(Guid cajeroId, decimal fondoInicial, CancellationToken ct = default)
    {
        var turnoActivo = await _uot.TurnosCaja.ObtenerTurnoActivoAsync(ct);
        if (turnoActivo is not null)
            throw new ReglaDominioException("Ya existe un turno de caja activo. Cerralo antes de abrir uno nuevo.");

        var cajero = await _uot.Usuarios.ObtenerPorIdAsync(cajeroId, ct)
            ?? throw new ReglaDominioException("El cajero no fue encontrado.");

        var turno = new TurnoCaja(cajeroId, fondoInicial);
        await _uot.TurnosCaja.AgregarAsync(turno, ct);
        await _uot.GuardarCambiosAsync(ct);

        // Recargar con navegación
        var guardado = await _uot.TurnosCaja.ObtenerPorIdAsync(turno.Id, ct)
            ?? throw new ReglaDominioException("Error al recuperar el turno recién creado.");
        return Map(guardado);
    }

    public async Task<TurnoCajaDto> CerrarTurnoAsync(Guid turnoId, decimal efectivoContado, string? observacion, CancellationToken ct = default)
    {
        var turno = await _uot.TurnosCaja.ObtenerPorIdAsync(turnoId, ct)
            ?? throw new ReglaDominioException("Turno de caja no encontrado.");

        // Calcular efectivo esperado: sum de pagos en efectivo dentro del turno
        var pagos = await _uot.Pagos.ObtenerPorRangoFechaAsync(turno.FechaApertura, DateTime.UtcNow, ct);
        var efectivoEsperado = pagos
            .Where(p => p.Metodo == MetodoPago.Efectivo)
            .Sum(p => p.Monto);

        turno.EstablecerEfectivoEsperado(efectivoEsperado);
        turno.Cerrar(efectivoContado, observacion);
        await _uot.GuardarCambiosAsync(ct);
        return Map(turno);
    }

    public async Task<TurnoCajaDto?> ObtenerTurnoActivoAsync(CancellationToken ct = default)
    {
        var turno = await _uot.TurnosCaja.ObtenerTurnoActivoAsync(ct);
        return turno is null ? null : Map(turno);
    }

    public async Task<List<TurnoCajaDto>> ObtenerHistorialAsync(int pagina = 1, int porPagina = 20, CancellationToken ct = default)
    {
        var turnos = await _uot.TurnosCaja.ObtenerHistorialAsync(pagina, porPagina, ct);
        return turnos.Select(Map).ToList();
    }

    public async Task RegistrarMovimientoAsync(Guid turnoId, string tipo, decimal monto, string motivo, Guid usuarioId, CancellationToken ct = default)
    {
        var turno = await _uot.TurnosCaja.ObtenerPorIdAsync(turnoId, ct)
            ?? throw new ReglaDominioException("Turno de caja no encontrado.");

        var movimiento = new MovimientoCaja(turnoId, tipo, monto, motivo, usuarioId);
        turno.RegistrarMovimiento(movimiento);
        await _uot.TurnosCaja.AgregarMovimientoAsync(movimiento, ct);
        await _uot.GuardarCambiosAsync(ct);
    }

    public async Task<ReporteZDto> GenerarReporteZAsync(Guid turnoId, CancellationToken ct = default)
    {
        var turno = await _uot.TurnosCaja.ObtenerPorIdAsync(turnoId, ct)
            ?? throw new ReglaDominioException("Turno de caja no encontrado.");

        var fechaFin = turno.FechaCierre ?? DateTime.UtcNow;
        var pagos = await _uot.Pagos.ObtenerPorRangoFechaAsync(turno.FechaApertura, fechaFin, ct);

        var totalVentas = pagos.Sum(p => p.Monto);
        var cantidadTickets = pagos.Select(p => p.CuentaId).Distinct().Count();
        var ticketPromedio = cantidadTickets > 0 ? totalVentas / cantidadTickets : 0;

        var desglosePorMetodo = pagos
            .GroupBy(p => p.Metodo)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(p => p.Monto));

        var retiros = turno.Movimientos
            .Where(m => m.Tipo == "retiro_seguridad")
            .Sum(m => m.Monto);

        return new ReporteZDto
        {
            TurnoId = turno.Id,
            CajeroNombre = turno.Cajero?.NombreCompleto ?? turno.CajeroId.ToString(),
            FechaApertura = turno.FechaApertura,
            FechaCierre = turno.FechaCierre,
            FondoInicial = turno.FondoInicial,
            TotalVentas = totalVentas,
            DesglosePorMetodoPago = desglosePorMetodo,
            CantidadTickets = cantidadTickets,
            TicketPromedio = ticketPromedio,
            TotalRetiroSeguridad = retiros,
            EfectivoEsperado = turno.EfectivoEsperado ?? 0,
            EfectivoContado = turno.EfectivoContado ?? 0,
            Diferencia = turno.Diferencia ?? 0,
            ObservacionCierre = turno.ObservacionCierre,
            Movimientos = turno.Movimientos.Select(MapMovimiento).ToList()
        };
    }

    private static TurnoCajaDto Map(TurnoCaja t) => new()
    {
        Id = t.Id,
        CajeroId = t.CajeroId,
        CajeroNombre = t.Cajero?.NombreCompleto ?? t.CajeroId.ToString(),
        FondoInicial = t.FondoInicial,
        FechaApertura = t.FechaApertura,
        FechaCierre = t.FechaCierre,
        Cerrado = t.Cerrado,
        EfectivoEsperado = t.EfectivoEsperado,
        EfectivoContado = t.EfectivoContado,
        Diferencia = t.Diferencia,
        ObservacionCierre = t.ObservacionCierre,
        Movimientos = t.Movimientos.Select(MapMovimiento).ToList()
    };

    private static MovimientoCajaDto MapMovimiento(MovimientoCaja m) => new()
    {
        Id = m.Id,
        Tipo = m.Tipo,
        Monto = m.Monto,
        Motivo = m.Motivo,
        FechaHora = m.FechaHora,
        UsuarioId = m.UsuarioId
    };
}

public class TurnoCajaDto
{
    public Guid Id { get; set; }
    public Guid CajeroId { get; set; }
    public string CajeroNombre { get; set; } = string.Empty;
    public decimal FondoInicial { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public bool Cerrado { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? EfectivoContado { get; set; }
    public decimal? Diferencia { get; set; }
    public string? ObservacionCierre { get; set; }
    public List<MovimientoCajaDto> Movimientos { get; set; } = [];
}

public class MovimientoCajaDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public Guid UsuarioId { get; set; }
}

public class ReporteZDto
{
    public Guid TurnoId { get; set; }
    public string CajeroNombre { get; set; } = string.Empty;
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal TotalVentas { get; set; }
    public Dictionary<string, decimal> DesglosePorMetodoPago { get; set; } = [];
    public int CantidadTickets { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal TotalRetiroSeguridad { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public decimal EfectivoContado { get; set; }
    public decimal Diferencia { get; set; }
    public string? ObservacionCierre { get; set; }
    public List<MovimientoCajaDto> Movimientos { get; set; } = [];
}
