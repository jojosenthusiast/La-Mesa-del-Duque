using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class CocinaServicio : ICocinaServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly INotificadorPedidos _notificador;

    public CocinaServicio(IUnidadDeTrabajo uot, INotificadorPedidos notificador)
    {
        _uot = uot;
        _notificador = notificador;
    }

    public async Task GenerarOrdenesAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesAsync(pedidoId, ct)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        foreach (var detalle in pedido.Detalles)
        {
            var estacion = detalle.Producto.Categoria?.EstacionCocina ?? EstacionCocina.Expo;

            var orden = new OrdenCocina(
                pedidoId,
                detalle.Id,
                detalle.Producto.Nombre,
                detalle.Cantidad,
                estacion,
                pedido.Mesa?.Numero,
                pedido.TipoServicio.ToString());

            await _uot.OrdenesCocina.AgregarAsync(orden, ct);
        }

        await _uot.GuardarCambiosAsync(ct);

        // Notificar a cada estación
        var ordenesCreadas = await _uot.OrdenesCocina.ListarPendientesAsync(cancelacion: ct);
        foreach (var orden in ordenesCreadas.Where(o => o.PedidoId == pedidoId))
        {
            await _notificador.NotificarOrdenCocinaAsync(orden.Estacion.ToString(), MapToDto(orden), ct);
        }
    }

    public async Task<List<OrdenCocinaDto>> ListarPendientesAsync(EstacionCocina? estacion = null, CancellationToken ct = default)
    {
        var ordenes = await _uot.OrdenesCocina.ListarPendientesAsync(estacion, ct);
        return ordenes.Select(MapToDto).ToList();
    }

    public async Task<OrdenCocinaDto> MarcarListoAsync(Guid ordenId, CancellationToken ct = default)
    {
        var orden = await _uot.OrdenesCocina.ObtenerPorIdAsync(ordenId, ct)
            ?? throw new ArgumentException($"No se encontró la orden de cocina con ID {ordenId}.", nameof(ordenId));

        orden.MarcarComoListo();
        await _uot.GuardarCambiosAsync(ct);
        await _notificador.NotificarItemListoAsync(orden.Estacion.ToString(), ordenId, ct);

        return MapToDto(orden);
    }

    public async Task<OrdenCocinaDto> RecuperarAsync(Guid ordenId, CancellationToken ct = default)
    {
        var orden = await _uot.OrdenesCocina.ObtenerPorIdAsync(ordenId, ct)
            ?? throw new ArgumentException($"No se encontró la orden de cocina con ID {ordenId}.", nameof(ordenId));

        orden.Recuperar();
        await _uot.GuardarCambiosAsync(ct);
        await _notificador.NotificarItemRecuperadoAsync(orden.Estacion.ToString(), MapToDto(orden), ct);

        return MapToDto(orden);
    }

    private static OrdenCocinaDto MapToDto(OrdenCocina orden)
    {
        return new OrdenCocinaDto
        {
            Id = orden.Id,
            PedidoId = orden.PedidoId,
            ProductoNombre = orden.ProductoNombre,
            Cantidad = orden.Cantidad,
            Estacion = orden.Estacion.ToString(),
            Estado = orden.Estado.ToString(),
            HoraRecibido = orden.HoraRecibido,
            MesaNumero = orden.MesaNumero,
            TipoServicio = orden.TipoServicio
        };
    }
}
