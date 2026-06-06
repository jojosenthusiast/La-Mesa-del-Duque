namespace LaMesaDelDuque.Aplicacion.Servicios;

/// <summary>
/// Verifica mesas con ocupación prolongada (>90 min) y genera alertas.
/// Usa OcupadaDesde de Mesa (Slice 1).
/// </summary>
public interface ITableTimerServicio
{
    Task<List<TableTimerAlertaDto>> ObtenerMesasConAlertaAsync(CancellationToken cancelacion = default);
}

public class TableTimerServicio : ITableTimerServicio
{
    private readonly IMesasServicio _mesas;

    public TableTimerServicio(IMesasServicio mesas) => _mesas = mesas;

    public async Task<List<TableTimerAlertaDto>> ObtenerMesasConAlertaAsync(CancellationToken cancelacion = default)
    {
        var mesas = await _mesas.ListarMesasAsync();
        var ahora = DateTime.UtcNow;
        return mesas
            .Where(m => m.Estado == "Ocupada" && m.OcupadaDesde.HasValue)
            .Select(m => new { m, minutos = (ahora - m.OcupadaDesde!.Value).TotalMinutes })
            .Where(x => x.minutos > 90)
            .Select(x => new TableTimerAlertaDto
            {
                MesaId = x.m.Id,
                MesaNumero = x.m.Numero,
                MinutosOcupada = (int)x.minutos,
                Urgencia = x.minutos > 150 ? "Critico" : "Alerta"
            })
            .ToList();
    }
}

public class TableTimerAlertaDto
{
    public Guid MesaId { get; set; }
    public int MesaNumero { get; set; }
    public int MinutosOcupada { get; set; }
    public string Urgencia { get; set; } = string.Empty;
}
