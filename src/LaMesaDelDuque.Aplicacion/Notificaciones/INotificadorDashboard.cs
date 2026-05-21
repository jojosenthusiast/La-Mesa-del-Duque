namespace LaMesaDelDuque.Aplicacion.Notificaciones;

public interface INotificadorDashboard
{
    Task NotificarMetricasInvalidadasAsync(CancellationToken cancelacion = default);
}
