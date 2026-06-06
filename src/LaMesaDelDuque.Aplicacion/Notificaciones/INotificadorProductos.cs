namespace LaMesaDelDuque.Aplicacion.Notificaciones;

public interface INotificadorProductos
{
    Task NotificarProductoAgotadoAsync(Guid productoId, string nombreProducto, CancellationToken cancelacion = default);
    Task NotificarProductoReactivadoAsync(Guid productoId, string nombreProducto, CancellationToken cancelacion = default);
}
