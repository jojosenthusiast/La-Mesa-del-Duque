using LaMesaDelDuque.Dominio.Modelos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IMetricaServicio
{
    Task<MetricasOperativasDto> ObtenerMetricasOperativasAsync(CancellationToken cancelacion = default);
    Task<List<VentaPorHoraDto>> ObtenerVentasPorHoraAsync(CancellationToken cancelacion = default);
}
