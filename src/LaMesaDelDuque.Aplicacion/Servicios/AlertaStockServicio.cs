using LaMesaDelDuque.Dominio.Repositorios;
using Microsoft.Extensions.Logging;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IAlertaStockServicio
{
    Task<List<AlertaStockDto>> ObtenerAlertasAsync(CancellationToken cancelacion = default);
}

public class AlertaStockServicio : IAlertaStockServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly ILogger<AlertaStockServicio> _logger;

    public AlertaStockServicio(IUnidadDeTrabajo uot, ILogger<AlertaStockServicio> logger)
    {
        _uot = uot;
        _logger = logger;
    }

    public async Task<List<AlertaStockDto>> ObtenerAlertasAsync(CancellationToken cancelacion = default)
    {
        var alertas = new List<AlertaStockDto>();
        try
        {
            var ingredientes = await _uot.Ingredientes.ObtenerTodosAsync(cancelacion);
            foreach (var ing in ingredientes.Where(i => i.StockActual <= i.StockMinimo))
            {
                alertas.Add(new AlertaStockDto
                {
                    IngredienteId = ing.Id,
                    Nombre = ing.Nombre,
                    StockActual = ing.StockActual,
                    StockMinimo = ing.StockMinimo,
                    Proveedor = ing.ProveedorDefault?.Nombre ?? "Sin proveedor",
                    Urgencia = ing.StockActual == 0 ? "Critico" : ing.StockActual <= ing.StockMinimo / 2 ? "Alto" : "Medio"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener alertas de stock");
        }
        return alertas;
    }
}

public class AlertaStockDto
{
    public Guid IngredienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string Urgencia { get; set; } = string.Empty;
}
