namespace LaMesaDelDuque.Aplicacion.Notificaciones;

public interface INotificadorSalon
{
    Task NotificarMesaMovidaAsync(Guid mesaId, int posX, int posY, CancellationToken cancelacion = default);
    Task NotificarMesaActualizadaAsync(Guid mesaId, string estado, CancellationToken cancelacion = default);
}
