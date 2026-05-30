using LaMesaDelDuque.Dominio.Modelos;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class MetricaServicio : IMetricaServicio
{
    private readonly IMetricaRepositorio _metricaRepositorio;

    public MetricaServicio(IMetricaRepositorio metricaRepositorio)
    {
        _metricaRepositorio = metricaRepositorio;
    }

    public Task<MetricasOperativasDto> ObtenerMetricasOperativasAsync(CancellationToken cancelacion = default)
    {
        var inicioTurno = DateTime.UtcNow.Date;
        return _metricaRepositorio.ObtenerMetricasHoyAsync(inicioTurno, cancelacion);
    }

    public Task<List<VentaPorHoraDto>> ObtenerVentasPorHoraAsync(CancellationToken cancelacion = default)
    {
        var inicioTurno = DateTime.UtcNow.Date;
        return _metricaRepositorio.ObtenerVentasPorHoraAsync(inicioTurno, cancelacion);
    }
}
