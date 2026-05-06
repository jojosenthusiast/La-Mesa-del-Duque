using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class PedidosServicio : IPedidosServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public PedidosServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<PedidoDto> CrearPedidoAsync(Guid mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default)
    {
        if (detalles is null || detalles.Count == 0)
            throw new ArgumentException("El pedido debe tener al menos un detalle.", nameof(detalles));

        var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId}.", nameof(mesaId));

        var pedido = new Pedido(mesa);

        foreach (var d in detalles)
        {
            var producto = await _uot.Productos.ObtenerConTrackingAsync(d.ProductoId, cancelacion)
                ?? throw new ArgumentException($"No se encontró el producto con ID {d.ProductoId}.", nameof(detalles));

            var detalle = new DetallePedido(producto, d.Cantidad, d.PrecioUnitario);
            pedido.AgregarDetalle(detalle);
        }

        await _uot.Pedidos.AgregarAsync(pedido, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(pedido);
    }

    public async Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        var producto = await _uot.Productos.ObtenerConTrackingAsync(productoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el producto con ID {productoId}.", nameof(productoId));

        var detalle = new DetallePedido(producto, cantidad, precioUnitario);
        pedido.AgregarDetalle(detalle);

        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(pedido);
    }

    public async Task CerrarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.Cerrar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    public async Task<PedidoDto> EliminarDetalleAsync(Guid pedidoId, Guid detalleId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.EliminarDetalle(detalleId);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(pedido);
    }

    public async Task<PedidoDto> ActualizarCantidadDetalleAsync(Guid pedidoId, Guid detalleId, int nuevaCantidad, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        if (pedido.Estado != EstadoPedido.Abierto)
            throw new ReglaDominioException("Solo se pueden modificar los detalles de pedidos abiertos.");

        var detalle = pedido.Detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new ReglaDominioException("El detalle especificado no pertenece a este pedido.");

        detalle.ActualizarCantidad(nuevaCantidad);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(pedido);
    }

    public async Task CancelarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.Cancelar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    public async Task<PedidoDto?> ObtenerPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesAsync(pedidoId, cancelacion);
        return pedido is null ? null : MapToDto(pedido);
    }

    public async Task<List<PedidoDto>> ListarPedidosActivosAsync(CancellationToken cancelacion = default)
    {
        var pedidos = await _uot.Pedidos.ObtenerTodosAsync(cancelacion);
        return pedidos
            .Where(p => p.Estado == EstadoPedido.Abierto)
            .Select(MapToDto)
            .ToList();
    }

    private static PedidoDto MapToDto(Pedido pedido)
    {
        return new PedidoDto
        {
            Id = pedido.Id,
            MesaId = pedido.Mesa.Id,
            MesaNumero = pedido.Mesa.Numero,
            Estado = pedido.Estado.ToString(),
            Total = pedido.Total,
            Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
            {
                Id = d.Id,
                ProductoId = d.Producto.Id,
                ProductoNombre = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }
}
