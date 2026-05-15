using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class PedidosServicio : IPedidosServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly INotificadorPedidos _notificadorPedidos;
    private readonly ICocinaServicio? _cocinaServicio;

    public PedidosServicio(IUnidadDeTrabajo uot, INotificadorPedidos notificadorPedidos, ICocinaServicio? cocinaServicio = null)
    {
        _uot = uot;
        _notificadorPedidos = notificadorPedidos;
        _cocinaServicio = cocinaServicio;
    }

    public async Task<PedidoDto> CrearPedidoAsync(TipoServicio tipoServicio, Guid? mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default)
    {
        if (detalles is null || detalles.Count == 0)
            throw new ArgumentException("El pedido debe tener al menos un detalle.", nameof(detalles));

        Mesa? mesa = null;

        if (tipoServicio == TipoServicio.ParaLlevar && mesaId.HasValue)
            throw new ReglaDominioException("Un pedido para llevar no puede tener mesa asignada.");

        if (mesaId.HasValue)
        {
            mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId.Value, cancelacion)
                ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId.Value}.", nameof(mesaId));

            if (mesa.Estado != EstadoMesa.Disponible)
                throw new ReglaDominioException("Solo se puede asignar una mesa disponible.");
        }

        var pedido = new Pedido(tipoServicio, mesa);

        if (mesa is not null)
            mesa.CambiarEstado(EstadoMesa.Ocupada);

        foreach (var d in detalles)
        {
            var producto = await _uot.Productos.ObtenerConTrackingAsync(d.ProductoId, cancelacion)
                ?? throw new ArgumentException($"No se encontró el producto con ID {d.ProductoId}.", nameof(detalles));

            var detalle = new DetallePedido(producto, d.Cantidad, d.PrecioUnitario, d.Notas);
            pedido.AgregarDetalle(detalle);
        }

        await _uot.Pedidos.AgregarAsync(pedido, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarPedidoCreadoAsync(pedido.Id, pedido.Estado, cancelacion);

        if (_cocinaServicio is not null)
            await _cocinaServicio.GenerarOrdenesAsync(pedido.Id, cancelacion);

        return MapToDto(pedido);
    }

    public async Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, string? notas = null, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        var producto = await _uot.Productos.ObtenerConTrackingAsync(productoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el producto con ID {productoId}.", nameof(productoId));

        var detalle = new DetallePedido(producto, cantidad, precioUnitario, notas);
        pedido.AgregarDetalle(detalle);

        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(pedido);
    }

    public async Task MarcarEnPreparacionAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarEnPreparacion();
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
    }

    public async Task PagarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarComoPagado();
        await LiberarMesaSiCorrespondeAsync(pedido, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
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

        if (pedido.Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("No se pueden modificar los detalles de un pedido pagado.");

        if (pedido.Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se pueden modificar los detalles de un pedido cancelado.");

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
        await LiberarMesaSiCorrespondeAsync(pedido, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarPedidoCanceladoAsync(pedido.Id, cancelacion);
    }

    public async Task EliminarPedidoPendienteAsync(Guid pedidoId, Guid usuarioId, string? ipAddress = null, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        if (pedido.Estado != EstadoPedido.Pendiente)
            throw new ReglaDominioException("Solo se puede eliminar un pedido pendiente que no ha sido pagado.");

        var usuario = await _uot.Usuarios.ObtenerPorIdAsync(usuarioId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el usuario con ID {usuarioId}.", nameof(usuarioId));

        if (pedido.Mesa is not null)
            pedido.Mesa.CambiarEstado(EstadoMesa.Disponible);

        var auditoria = new Auditoria("pedido", pedido.Id, "DELETE", usuario, pedido.Estado.ToString(), null, ipAddress);

        await _uot.Auditorias.AgregarAsync(auditoria, cancelacion);
        _uot.Pedidos.Eliminar(pedido);
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
            .Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion)
            .Select(MapToDto)
            .ToList();
    }

    private async Task LiberarMesaSiCorrespondeAsync(Pedido pedido, CancellationToken cancelacion)
    {
        if (pedido.Mesa is null) return;

        var pedidosMesa = await _uot.Pedidos.ObtenerPorMesaAsync(pedido.Mesa.Id, cancelacion);
        var tieneActivos = pedidosMesa.Any(p =>
            p.Id != pedido.Id &&
            (p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion));

        if (!tieneActivos)
        {
            var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(pedido.Mesa.Id, cancelacion);
            mesa?.CambiarEstado(EstadoMesa.Disponible);
        }
    }

    private static PedidoDto MapToDto(Pedido pedido)
    {
        return new PedidoDto
        {
            Id = pedido.Id,
            TipoServicio = pedido.TipoServicio.ToString(),
            MesaId = pedido.Mesa?.Id,
            MesaNumero = pedido.Mesa?.Numero,
            Estado = pedido.Estado.ToString(),
            Total = pedido.Total,
            Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
            {
                Id = d.Id,
                ProductoId = d.Producto.Id,
                ProductoNombre = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                Notas = d.Notas
            }).ToList()
        };
    }
}
