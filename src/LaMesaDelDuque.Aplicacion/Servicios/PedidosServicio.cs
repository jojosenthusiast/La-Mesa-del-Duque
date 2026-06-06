using System.Security.Claims;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using Microsoft.AspNetCore.Http;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class PedidosServicio : IPedidosServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly INotificadorPedidos _notificadorPedidos;
    private readonly INotificadorDashboard? _notificadorDashboard;
    private readonly ICocinaServicio? _cocinaServicio;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public PedidosServicio(IUnidadDeTrabajo uot, INotificadorPedidos notificadorPedidos, ICocinaServicio? cocinaServicio = null, IHttpContextAccessor? httpContextAccessor = null, INotificadorDashboard? notificadorDashboard = null)
    {
        _uot = uot;
        _notificadorPedidos = notificadorPedidos;
        _cocinaServicio = cocinaServicio;
        _httpContextAccessor = httpContextAccessor;
        _notificadorDashboard = notificadorDashboard;
    }

    public async Task<PedidoDto> CrearPedidoAsync(TipoServicio tipoServicio, Guid? mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

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
            {
                var pedidoActivoMesa = await _uot.Pedidos.ObtenerActivoPorMesaParaActualizarAsync(mesa.Id, cancelacion);
                var mesaOcupadaSinPedido = mesa.Estado == EstadoMesa.Ocupada && pedidoActivoMesa is null;

                if (!mesaOcupadaSinPedido)
                    throw new ReglaDominioException("Solo se puede asignar una mesa disponible o una mesa ocupada sin pedido activo.");
            }
        }

        var pedido = new Pedido(tipoServicio, mesa);

        var meseroActualId = ObtenerMeseroActualId();
        if (mesa is not null && meseroActualId != Guid.Empty)
            pedido.AsignarMesero(meseroActualId);

        foreach (var d in detalles)
        {
            var producto = await _uot.Productos.ObtenerConTrackingAsync(d.ProductoId, cancelacion)
                ?? throw new ArgumentException($"No se encontró el producto con ID {d.ProductoId}.", nameof(detalles));

            var detalle = new DetallePedido(producto, d.Cantidad, d.PrecioUnitario, d.Notas, d.ModificacionesJson);
            await AplicarMejorPromocionAsync(detalle, d.ProductoId, d.PrecioUnitario, cancelacion);
            pedido.AgregarDetalle(detalle);
        }

        await ReservarStockAsync(pedido.Detalles, cancelacion);

        if (mesa is not null)
            mesa.CambiarEstado(EstadoMesa.Ocupada);

        await _uot.Pedidos.AgregarAsync(pedido, cancelacion);
        pedido.MarcarEnPreparacion();
        await GuardarCambiosDePedidoConStockAsync(cancelacion);
        await _notificadorPedidos.NotificarPedidoCreadoAsync(pedido.Id, pedido.Estado, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);

        if (_cocinaServicio is not null)
            await _cocinaServicio.GenerarOrdenesAsync(pedido.Id, null, cancelacion);

        return MapToDto(pedido);
    }

    public async Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, string? notas = null, string? modificacionesJson = null, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        if (pedido.Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido pagado.");

        if (pedido.Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido cancelado.");

        var producto = await _uot.Productos.ObtenerConTrackingAsync(productoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el producto con ID {productoId}.", nameof(productoId));

        var detalle = new DetallePedido(producto, cantidad, precioUnitario, notas, modificacionesJson);
        await AplicarMejorPromocionAsync(detalle, productoId, precioUnitario, cancelacion);
        await ReservarStockAsync([detalle], cancelacion);

        pedido.AgregarDetalle(detalle);
        await _uot.Pedidos.AgregarDetalleAsync(detalle, cancelacion);
        await GuardarCambiosDePedidoConStockAsync(cancelacion);

        if (_cocinaServicio is not null)
            await _cocinaServicio.GenerarOrdenesAsync(pedidoId, new[] { detalle.Id }, cancelacion);

        return MapToDto(pedido);
    }

    public async Task AgregarItemsAsync(Guid pedidoId, List<DetalleCreacionDto> items, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        if (pedido.Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido pagado.");

        if (pedido.Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido cancelado.");

        var nuevosDetalles = new List<DetallePedido>();
        foreach (var item in items)
        {
            var producto = await _uot.Productos.ObtenerConTrackingAsync(item.ProductoId, cancelacion)
                ?? throw new ArgumentException($"No se encontró el producto con ID {item.ProductoId}.", nameof(items));
            var detalle = new DetallePedido(producto, item.Cantidad, item.PrecioUnitario, item.Notas, item.ModificacionesJson);
            await AplicarMejorPromocionAsync(detalle, item.ProductoId, item.PrecioUnitario, cancelacion);
            nuevosDetalles.Add(detalle);
        }

        await ReservarStockAsync(nuevosDetalles, cancelacion);

        foreach (var detalle in nuevosDetalles)
        {
            pedido.AgregarDetalle(detalle);
            await _uot.Pedidos.AgregarDetalleAsync(detalle, cancelacion);
        }

        await GuardarCambiosDePedidoConStockAsync(cancelacion);

        if (_cocinaServicio is not null && nuevosDetalles.Count > 0)
            await _cocinaServicio.GenerarOrdenesAsync(pedidoId, nuevosDetalles.Select(d => d.Id), cancelacion);
    }

    public async Task MarcarEnPreparacionAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarEnPreparacion();
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);
    }

    public async Task MarcarListoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarListo();
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
    }

    public async Task PagarPedidoAsync(Guid pedidoId, MetodoPago metodoPago = MetodoPago.Efectivo, string? referenciaPos = null, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);
        await AsegurarTurnoCajaActivoAsync(cancelacion);

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        // Crear cuenta única si no existe ninguna (flujo de pago simple)
        var cuentasExistentes = await _uot.Cuentas.ObtenerPorPedidoAsync(pedidoId, cancelacion);
        if (!cuentasExistentes.Any())
        {
            var cuentaUnica = new Cuenta(pedidoId, 1);
            cuentaUnica.EstablecerTotalBase(pedido.Total);
            await _uot.Cuentas.AgregarAsync(cuentaUnica, cancelacion);
            await _uot.GuardarCambiosAsync(cancelacion);

            var usuarioId = ObtenerUsuarioIdActual();
            cuentaUnica.Pagar(metodoPago, 0, usuarioId);
            var pago = new Pago(cuentaUnica.Id, pedido.Total, metodoPago, 0, usuarioId, referenciaPos);
            await _uot.Pagos.AgregarAsync(pago, cancelacion);
        }

        pedido.MarcarComoPagado();
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);
    }

    private async Task ReservarStockAsync(IEnumerable<DetallePedido> detalles, CancellationToken ct)
    {
        var consumos = await CalcularConsumosTotalesAsync(detalles, ct);
        await ValidarStockSuficienteAsync(consumos, ct);
        await DescontarStockAsync(consumos, ct);
        await SincronizarProductosPorIngredientesAsync(consumos.Keys, ct);
    }

    private async Task<Dictionary<Guid, ConsumoIngrediente>> CalcularConsumosTotalesAsync(IEnumerable<DetallePedido> detalles, CancellationToken ct)
    {
        var totales = new Dictionary<Guid, ConsumoIngrediente>();

        foreach (var detalle in detalles)
        {
            var receta = await _uot.RecetasProductos.ObtenerPorProductoIdAsync(detalle.Producto.Id, ct);
            if (receta is null) continue;

            var consumos = CalcularConsumosDetalle(detalle, receta);
            foreach (var (ingredienteId, consumo) in consumos)
            {
                if (!totales.TryGetValue(ingredienteId, out var total))
                {
                    total = new ConsumoIngrediente(ingredienteId);
                    totales[ingredienteId] = total;
                }

                total.Cantidad += consumo;
                total.Productos.Add(detalle.Producto.Nombre);
            }
        }

        return totales;
    }

    private async Task ValidarStockSuficienteAsync(Dictionary<Guid, ConsumoIngrediente> consumos, CancellationToken ct)
    {
        var faltantes = new List<string>();

        foreach (var consumo in consumos.Values)
        {
            var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(consumo.IngredienteId, ct);
            if (ingrediente is null) continue;

            if (ingrediente.StockActual < consumo.Cantidad)
            {
                var productos = string.Join(", ", consumo.Productos.OrderBy(p => p));
                faltantes.Add($"{productos}: {ingrediente.Nombre} (necesario: {consumo.Cantidad} {ingrediente.UnidadMedida}, disponible: {ingrediente.StockActual})");
            }
        }

        if (faltantes.Count > 0)
            throw new ReglaDominioException($"Stock insuficiente para completar el pedido: {string.Join("; ", faltantes)}");
    }

    private async Task DescontarStockAsync(Dictionary<Guid, ConsumoIngrediente> consumos, CancellationToken ct)
    {
        foreach (var consumo in consumos.Values)
        {
            var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(consumo.IngredienteId, ct);
            if (ingrediente is null) continue;
            ingrediente.DescontarStock(consumo.Cantidad);
        }
    }

    private async Task DevolverStockAsync(IEnumerable<DetallePedido> detalles, CancellationToken ct)
    {
        var consumos = await CalcularConsumosTotalesAsync(detalles, ct);
        await DevolverConsumosAsync(consumos.ToDictionary(c => c.Key, c => c.Value.Cantidad), ct);
        await SincronizarProductosPorIngredientesAsync(consumos.Keys, ct);
    }

    private async Task DevolverConsumosAsync(Dictionary<Guid, decimal> consumos, CancellationToken ct)
    {
        foreach (var (ingredienteId, consumo) in consumos)
        {
            var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(ingredienteId, ct);
            if (ingrediente is null) continue;
            ingrediente.DevolverStock(consumo);
        }
    }

    private async Task<Dictionary<Guid, decimal>> CalcularConsumosDetalleAsync(DetallePedido detalle, int cantidad, CancellationToken ct)
    {
        var receta = await _uot.RecetasProductos.ObtenerPorProductoIdAsync(detalle.Producto.Id, ct);
        return receta is null ? [] : CalcularConsumosDetalle(detalle, receta, cantidad);
    }

    private async Task SincronizarProductosPorIngredientesAsync(IEnumerable<Guid> ingredienteIds, CancellationToken ct)
    {
        var recetasProcesadas = new HashSet<Guid>();

        foreach (var ingredienteId in ingredienteIds.Distinct())
        {
            var recetas = await _uot.RecetasProductos.ObtenerPorIngredienteAsync(ingredienteId, ct);
            foreach (var receta in recetas.Where(r => recetasProcesadas.Add(r.Id)))
            {
                var disponible = receta.Ingredientes.All(ri =>
                    ri.Ingrediente.Activo && ri.Ingrediente.StockActual >= ri.CantidadRequerida);

                if (!disponible && receta.Producto.Activo)
                    receta.Producto.Desactivar();
            }
        }
    }

    private async Task GuardarCambiosDePedidoConStockAsync(CancellationToken ct)
    {
        try
        {
            await _uot.GuardarCambiosAsync(ct);
        }
        catch (ConcurrenciaException ex)
        {
            throw new ReglaDominioException("El stock cambió mientras se confirmaba el pedido. Revise disponibilidad e intente nuevamente.", ex);
        }
    }

    private async Task AplicarMejorPromocionAsync(DetallePedido detalle, Guid productoId, decimal precioUnitario, CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var promos = await _uot.Promociones.ObtenerActivasPorProductoAsync(productoId, hoy, ct);
        if (promos.Count == 0) return;

        var mejor = promos.OrderByDescending(pp =>
            pp.Promocion.TipoDescuento == "porcentaje"
                ? precioUnitario * pp.Promocion.ValorDescuento / 100
                : pp.Promocion.ValorDescuento
        ).First();

        var descuento = mejor.Promocion.TipoDescuento == "porcentaje"
            ? Math.Round(precioUnitario * mejor.Promocion.ValorDescuento / 100, 2)
            : mejor.Promocion.ValorDescuento;

        detalle.AplicarPromocion(Math.Min(descuento, precioUnitario), mejor.Promocion.Nombre);
    }

    private static Dictionary<Guid, decimal> CalcularConsumosDetalle(DetallePedido detalle, RecetaProducto receta, int? cantidadOverride = null)
    {
        var mods = detalle.ObtenerModificaciones();

        var quitados = mods
            .Where(m => m.Accion == "quitar")
            .Select(m => m.IngredienteId)
            .ToHashSet();

        var intercambios = mods
            .Where(m => m.Accion == "intercambiar" && m.IngredienteReemplazoId.HasValue)
            .ToDictionary(m => m.IngredienteId, m => m.IngredienteReemplazoId!.Value);

        var extras = mods
            .Where(m => m.Accion == "extra")
            .Select(m => m.IngredienteId)
            .ToHashSet();

        var consumos = new Dictionary<Guid, decimal>();

        foreach (var ri in receta.Ingredientes)
        {
            if (quitados.Contains(ri.IngredienteId)) continue;

            var targetId = intercambios.TryGetValue(ri.IngredienteId, out var reemplazo)
                ? reemplazo
                : ri.IngredienteId;

            var cantidadDetalle = cantidadOverride ?? detalle.Cantidad;
            var cantidad = ri.CantidadRequerida * cantidadDetalle;
            if (extras.Contains(ri.IngredienteId))
                cantidad += ri.CantidadRequerida * cantidadDetalle;

            consumos[targetId] = consumos.TryGetValue(targetId, out var prev) ? prev + cantidad : cantidad;
        }

        foreach (var m in mods.Where(m => m.Accion == "extra"))
        {
            if (receta.Ingredientes.Any(ri => ri.IngredienteId == m.IngredienteId)) continue;
            var cantidad = cantidadOverride ?? detalle.Cantidad;
            consumos[m.IngredienteId] = consumos.TryGetValue(m.IngredienteId, out var prev) ? prev + cantidad : cantidad;
        }

        return consumos;
    }

    public async Task<PedidoDto> EliminarDetalleAsync(Guid pedidoId, Guid detalleId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        var detalle = pedido.Detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new ReglaDominioException("El detalle especificado no pertenece a este pedido.");

        pedido.EliminarDetalle(detalleId);
        await DevolverStockAsync([detalle], cancelacion);
        await GuardarCambiosDePedidoConStockAsync(cancelacion);

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

        if (nuevaCantidad == detalle.Cantidad)
            return MapToDto(pedido);

        if (nuevaCantidad <= 0)
            throw new ReglaDominioException("La cantidad del detalle debe ser mayor que cero.");

        var consumoActual = await CalcularConsumosDetalleAsync(detalle, detalle.Cantidad, cancelacion);
        var consumoNuevo = await CalcularConsumosDetalleAsync(detalle, nuevaCantidad, cancelacion);
        var incremento = RestarConsumos(consumoNuevo, consumoActual);
        var devolucion = RestarConsumos(consumoActual, consumoNuevo);

        if (incremento.Count > 0)
        {
            var consumos = incremento.ToDictionary(
                c => c.Key,
                c => new ConsumoIngrediente(c.Key)
                {
                    Cantidad = c.Value,
                    Productos = { detalle.Producto.Nombre }
                });
            await ValidarStockSuficienteAsync(consumos, cancelacion);
            await DescontarStockAsync(consumos, cancelacion);
        }

        if (devolucion.Count > 0)
        {
            await DevolverConsumosAsync(devolucion, cancelacion);
            await SincronizarProductosPorIngredientesAsync(devolucion.Keys, cancelacion);
        }

        if (incremento.Count > 0)
            await SincronizarProductosPorIngredientesAsync(incremento.Keys, cancelacion);

        detalle.ActualizarCantidad(nuevaCantidad);
        await GuardarCambiosDePedidoConStockAsync(cancelacion);

        return MapToDto(pedido);
    }

    public async Task CancelarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        var stockDescontado = pedido.Estado is EstadoPedido.EnPreparacion or EstadoPedido.Listo or EstadoPedido.EnCobro;
        pedido.Cancelar();
        await LiberarMesaSiCorrespondeAsync(pedido, cancelacion);
        if (stockDescontado)
            await DevolverStockAsync(pedido.Detalles, cancelacion);
        await GuardarCambiosDePedidoConStockAsync(cancelacion);
        await _notificadorPedidos.NotificarPedidoCanceladoAsync(pedido.Id, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);
    }

    public async Task AnularPagoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.AnularPago();
        await LiberarMesaSiCorrespondeAsync(pedido, cancelacion);
        await DevolverStockAsync(pedido.Detalles, cancelacion);
        await GuardarCambiosDePedidoConStockAsync(cancelacion);
        await _notificadorPedidos.NotificarPedidoCanceladoAsync(pedido.Id, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);
    }

    public async Task EliminarPedidoPendienteAsync(Guid pedidoId, Guid usuarioId, string? ipAddress = null, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        if (pedido.Estado == EstadoPedido.Pagado || pedido.Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("Solo se puede eliminar un pedido que no haya sido pagado ni cancelado.");

        var usuario = await _uot.Usuarios.ObtenerPorIdAsync(usuarioId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el usuario con ID {usuarioId}.", nameof(usuarioId));

        var stockDescontado = pedido.Estado is EstadoPedido.EnPreparacion or EstadoPedido.Listo or EstadoPedido.EnCobro;
        if (stockDescontado)
            await DevolverStockAsync(pedido.Detalles, cancelacion);

        if (pedido.Mesa is not null)
            pedido.Mesa.CambiarEstado(EstadoMesa.Disponible);

        var auditoria = new Auditoria("pedido", pedido.Id, "DELETE", usuario, pedido.Estado.ToString(), null, ipAddress);

        await _uot.Auditorias.AgregarAsync(auditoria, cancelacion);
        _uot.Pedidos.Eliminar(pedido);
        await GuardarCambiosDePedidoConStockAsync(cancelacion);
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
            .Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion || p.Estado == EstadoPedido.EnCobro || p.Estado == EstadoPedido.Listo)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<List<PedidoDto>> ListarListosParaDespachoAsync(CancellationToken cancelacion = default)
    {
        var pedidos = await _uot.Pedidos.ObtenerTodosAsync(cancelacion);
        var listos = pedidos
            .Where(p => p.Estado == EstadoPedido.Listo || p.Estado == EstadoPedido.Pagado)
            .ToList();

        var dtos = new List<PedidoDto>(listos.Count);
        foreach (var pedido in listos)
        {
            var dto = MapToDto(pedido);
            dto.FechaListoDespacho = await ObtenerFechaListoDespachoAsync(pedido.Id, cancelacion);
            dtos.Add(dto);
        }

        return dtos;
    }

    private async Task<DateTime?> ObtenerFechaListoDespachoAsync(Guid pedidoId, CancellationToken cancelacion)
    {
        var ordenes = await _uot.OrdenesCocina.ListarPorPedidoAsync(pedidoId, cancelacion);
        var horasListas = ordenes
            .Where(o => o.HoraListo.HasValue)
            .Select(o => o.HoraListo!.Value)
            .ToList();

        return horasListas.Count == 0 ? null : horasListas.Max();
    }

    public async Task MarcarEnCobroAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarEnCobro();
        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);
    }

    public async Task<List<CuentaDto>> CrearCuentasAsync(Guid pedidoId, int cantidad, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

        var pedido = await _uot.Pedidos.ObtenerConCuentasParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarEnCobro();
        var cuentas = pedido.CrearCuentas(cantidad);

        foreach (var cuenta in cuentas)
        {
            await _uot.Cuentas.AgregarAsync(cuenta, cancelacion);
        }

        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);

        return cuentas.Select(MapToCuentaDto).ToList();
    }

    public async Task<List<CuentaDto>> CrearCuentasConItemsAsync(Guid pedidoId, Dictionary<int, List<(Guid detalleId, int cantidad)>> asignaciones, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);

        if (asignaciones is null || asignaciones.Count < 2)
            throw new ArgumentException("Se requieren al menos 2 cuentas para dividir por items.", nameof(asignaciones));

        var pedido = await _uot.Pedidos.ObtenerConCuentasParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        pedido.MarcarEnCobro();

        var detalles = pedido.Detalles.ToDictionary(d => d.Id);
        var asignacionesEntidades = new Dictionary<int, List<(DetallePedido detalle, int cantidad)>>();

        foreach (var kvp in asignaciones)
        {
            var lista = new List<(DetallePedido detalle, int cantidad)>();
            foreach (var (detalleId, cantidad) in kvp.Value)
            {
                if (!detalles.TryGetValue(detalleId, out var detalle))
                    throw new ArgumentException($"El detalle {detalleId} no pertenece al pedido.");
                lista.Add((detalle, cantidad));
            }
            asignacionesEntidades[kvp.Key] = lista;
        }

        var cuentas = pedido.CrearCuentasConItems(asignacionesEntidades);

        foreach (var cuenta in cuentas)
        {
            await _uot.Cuentas.AgregarAsync(cuenta, cancelacion);
        }

        await _uot.GuardarCambiosAsync(cancelacion);
        await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
        await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);

        return cuentas.Select(MapToCuentaDto).ToList();
    }

    public async Task<CuentaDto> PagarCuentaAsync(Guid cuentaId, MetodoPago metodoPago, decimal propinaMonto = 0, string? referenciaPos = null, CancellationToken cancelacion = default)
    {
        await AsegurarDiaOperativoAbiertoAsync(cancelacion);
        await AsegurarTurnoCajaActivoAsync(cancelacion);

        for (int intento = 0; intento < 3; intento++)
        {
            try
            {
                var cuenta = await _uot.Cuentas.ObtenerParaActualizarAsync(cuentaId, cancelacion)
                    ?? throw new ArgumentException($"No se encontró la cuenta con ID {cuentaId}.", nameof(cuentaId));

                var usuarioId = ObtenerUsuarioIdActual();
                if (usuarioId == Guid.Empty)
                    throw new InvalidOperationException("No se pudo identificar el usuario actual para registrar el pago.");

                cuenta.Pagar(metodoPago, propinaMonto, usuarioId);

                var pago = new Pago(cuentaId, cuenta.Total, metodoPago, propinaMonto, usuarioId, referenciaPos);
                await _uot.Pagos.AgregarAsync(pago, cancelacion);

                var pedido = await _uot.Pedidos.ObtenerConCuentasParaActualizarAsync(cuenta.PedidoId, cancelacion);
                if (pedido is not null && pedido.EstaPagadoCompletamente)
                {
                    pedido.MarcarComoPagado();
                    await _notificadorPedidos.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, cancelacion);
                }

                await _uot.GuardarCambiosAsync(cancelacion);
                await NotificarMetricasInvalidadasSiExisteAsync(cancelacion);
                return MapToCuentaDto(cuenta);
            }
            catch (ConcurrenciaException) when (intento < 2)
            {
                await Task.Delay(50, cancelacion);
            }
        }

        throw new InvalidOperationException("No se pudo completar el pago por conflictos de concurrencia.");
    }

    public async Task<List<CuentaDto>> ObtenerCuentasAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var cuentas = await _uot.Cuentas.ObtenerPorPedidoAsync(pedidoId, cancelacion);
        return cuentas.Select(MapToCuentaDto).ToList();
    }

    private async Task AsegurarDiaOperativoAbiertoAsync(CancellationToken cancelacion)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var cierre = await _uot.CierresDia.ObtenerAbiertoAsync(hoy, cancelacion);
        if (cierre is null)
            throw new ReglaDominioException("No hay día operativo abierto. Abra el día en Cierre de Día antes de registrar pedidos o pagos.");
    }

    private async Task AsegurarTurnoCajaActivoAsync(CancellationToken cancelacion)
    {
        var turno = await _uot.TurnosCaja.ObtenerTurnoActivoAsync(cancelacion);
        if (turno is null)
            throw new ReglaDominioException("No hay turno de caja activo. Abra un turno de caja activo antes de registrar pagos.");
    }

    private Guid ObtenerUsuarioIdActual()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Guid.Empty;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var usuarioId))
            return usuarioId;

        return Guid.Empty;
    }

    private Guid ObtenerMeseroActualId()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Mesero"))
            return Guid.Empty;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var usuarioId))
            return usuarioId;

        return Guid.Empty;
    }

    private async Task NotificarMetricasInvalidadasSiExisteAsync(CancellationToken cancelacion)
    {
        if (_notificadorDashboard is not null)
            await _notificadorDashboard.NotificarMetricasInvalidadasAsync(cancelacion);
    }

    private async Task LiberarMesaSiCorrespondeAsync(Pedido pedido, CancellationToken cancelacion)
    {
        if (pedido.Mesa is null) return;

        var pedidosMesa = await _uot.Pedidos.ObtenerPorMesaAsync(pedido.Mesa.Id, cancelacion);
        var tieneActivos = pedidosMesa.Any(p =>
            p.Id != pedido.Id &&
            MantieneMesaOcupada(p.Estado));

        if (!tieneActivos)
        {
            var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(pedido.Mesa.Id, cancelacion);
            mesa?.CambiarEstado(EstadoMesa.Disponible);
        }
    }

    private static bool MantieneMesaOcupada(EstadoPedido estado) =>
        estado is EstadoPedido.Pendiente
            or EstadoPedido.EnPreparacion
            or EstadoPedido.EnCobro
            or EstadoPedido.Pagado
            or EstadoPedido.Listo;

    private static Dictionary<Guid, decimal> RestarConsumos(Dictionary<Guid, decimal> origen, Dictionary<Guid, decimal> aRestar)
    {
        var resultado = new Dictionary<Guid, decimal>();
        foreach (var (ingredienteId, cantidad) in origen)
        {
            var delta = cantidad - aRestar.GetValueOrDefault(ingredienteId);
            if (delta > 0) resultado[ingredienteId] = delta;
        }

        return resultado;
    }

    private sealed class ConsumoIngrediente
    {
        public ConsumoIngrediente(Guid ingredienteId)
        {
            IngredienteId = ingredienteId;
        }

        public Guid IngredienteId { get; }
        public decimal Cantidad { get; set; }
        public HashSet<string> Productos { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private static PedidoDto MapToDto(Pedido pedido)
    {
        return new PedidoDto
        {
            Id = pedido.Id,
            TipoServicio = pedido.TipoServicio.ToString(),
            MesaId = pedido.Mesa?.Id,
            MesaNumero = pedido.Mesa?.Numero,
            MeseroAsignadoId = pedido.MeseroAsignadoId,
            Estado = pedido.Estado.ToString(),
            Total = pedido.Total,
            FechaCreacion = pedido.CreatedAt,
            Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
            {
                Id = d.Id,
                ProductoId = d.Producto.Id,
                ProductoNombre = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                PrecioOriginal = d.PrecioOriginal,
                DescuentoAplicado = d.DescuentoAplicado,
                PromocionNombre = d.PromocionNombre,
                Subtotal = d.Subtotal,
                Notas = d.Notas,
                ModificacionesJson = d.ModificacionesJson
            }).ToList()
        };
    }

    private static CuentaDto MapToCuentaDto(Cuenta cuenta)
    {
        return new CuentaDto
        {
            Id = cuenta.Id,
            PedidoId = cuenta.PedidoId,
            Numero = cuenta.Numero,
            Total = cuenta.Total,
            PropinaMonto = cuenta.PropinaMonto,
            MetodoPago = cuenta.MetodoPago?.ToString(),
            Estado = cuenta.Estado.ToString(),
            FechaPago = cuenta.FechaPago,
            Detalles = cuenta.DetallesAsignados.Select(d => new CuentaDetalleDto
            {
                Id = d.Id,
                DetallePedidoId = d.DetallePedidoId,
                CantidadAsignada = d.CantidadAsignada,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }
}
