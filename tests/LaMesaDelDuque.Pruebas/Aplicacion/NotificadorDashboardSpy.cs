using LaMesaDelDuque.Aplicacion.Notificaciones;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

internal sealed class NotificadorDashboardSpy : INotificadorDashboard
{
    public int MetricasInvalidadasCount { get; private set; }

    public Task NotificarMetricasInvalidadasAsync(CancellationToken cancelacion = default)
    {
        MetricasInvalidadasCount++;
        return Task.CompletedTask;
    }

    public void Reset() => MetricasInvalidadasCount = 0;
}
