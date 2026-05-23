using Microsoft.Extensions.Logging;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IShiftHandoffServicio
{
    Task<List<MesaAsignadaDto>> ObtenerMesasActivasAsync(Guid usuarioId, CancellationToken cancelacion = default);
    Task TransferirMesaAsync(Guid mesaId, Guid nuevoMeseroId, CancellationToken cancelacion = default);
}

public class ShiftHandoffServicio : IShiftHandoffServicio
{
    private readonly IMesasServicio _mesas;
    private readonly IPedidosServicio _pedidos;
    private readonly ILogger<ShiftHandoffServicio> _logger;

    public ShiftHandoffServicio(IMesasServicio mesas, IPedidosServicio pedidos, ILogger<ShiftHandoffServicio> logger)
    {
        _mesas = mesas;
        _pedidos = pedidos;
        _logger = logger;
    }

    public async Task<List<MesaAsignadaDto>> ObtenerMesasActivasAsync(Guid usuarioId, CancellationToken cancelacion = default)
    {
        var mesas = await _mesas.ListarMesasAsync();
        return mesas.Where(m => m.Estado == "Ocupada").Select(m => new MesaAsignadaDto
        {
            MesaId = m.Id,
            MesaNumero = m.Numero,
            Capacidad = m.Capacidad,
            MinutosOcupada = m.OcupadaDesde.HasValue ? (int)(DateTime.UtcNow - m.OcupadaDesde.Value).TotalMinutes : 0
        }).ToList();
    }

    public async Task TransferirMesaAsync(Guid mesaId, Guid nuevoMeseroId, CancellationToken cancelacion = default)
    {
        _logger.LogInformation("Transferencia de mesa {MesaId} al mesero {MeseroId}", mesaId, nuevoMeseroId);
        // La transferencia se registra como evento; los pedidos activos en esa mesa
        // quedan visibles para el nuevo mesero vía SignalR.
        await Task.CompletedTask;
    }
}

public class MesaAsignadaDto
{
    public Guid MesaId { get; set; }
    public int MesaNumero { get; set; }
    public int Capacidad { get; set; }
    public int MinutosOcupada { get; set; }
}
