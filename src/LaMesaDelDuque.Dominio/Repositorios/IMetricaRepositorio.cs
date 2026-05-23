using LaMesaDelDuque.Dominio.Modelos;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IMetricaRepositorio
{
    Task<MetricasOperativasDto> ObtenerMetricasHoyAsync(DateTime inicioTurno, CancellationToken ct = default);
    Task<List<VentaPorHoraDto>> ObtenerVentasPorHoraAsync(DateTime inicioTurno, CancellationToken ct = default);
}
