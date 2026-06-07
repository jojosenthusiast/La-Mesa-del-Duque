using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public class DeliveryPedidoDto
{
    public Guid Id { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Items { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? TelefonoCliente { get; set; }
    public Guid? RepartidorId { get; set; }
    public string? RepartidorNombre { get; set; }
    public DateTime? AsignadoEn { get; set; }
    public DateTime? EntregadoEn { get; set; }
    public bool Pagado { get; set; }
    public bool Entregado => EntregadoEn.HasValue || Estado == EstadoPedido.Despachado.ToString();
    public bool PuedeEntregarse => RepartidorId.HasValue && !Entregado && (Pagado || Estado is "Listo" or "Pagado");
}

public class RepartidorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class DeliveryResumenDto
{
    public int TotalDomicilio { get; set; }
    public int SinRepartidor { get; set; }
    public int EnRuta { get; set; }
    public int Entregados { get; set; }
    public decimal VentasDomicilio { get; set; }
}

public interface IDeliveryServicio
{
    Task<List<DeliveryPedidoDto>> ListarPedidosDomicilioAsync(CancellationToken ct = default);
    Task<List<DeliveryPedidoDto>> ListarPedidosAsignadosAsync(Guid repartidorId, CancellationToken ct = default);
    Task<DeliveryResumenDto> ObtenerResumenAsync(CancellationToken ct = default);
    Task<List<RepartidorDto>> ListarRepartidoresAsync(CancellationToken ct = default);
    Task<List<ProductoDto>> ListarProductosAsync(CancellationToken ct = default);
    Task<Guid> CrearPedidoDomicilioAsync(string? direccion, string? telefono, Dictionary<Guid, int> items, CancellationToken ct = default);
    Task AsignarRepartidorAsync(Guid pedidoId, Guid repartidorId, CancellationToken ct = default);
    Task MarcarEntregadoAsync(Guid pedidoId, CancellationToken ct = default);
    Task ActualizarDatosEntregaAsync(Guid pedidoId, string? direccion, string? telefono, CancellationToken ct = default);
}

internal class DeliveryServicio : IDeliveryServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly IPedidosServicio _pedidos;
    private readonly ICatalogoProductosServicio _catalogo;

    public DeliveryServicio(IUnidadDeTrabajo uot, IPedidosServicio pedidos, ICatalogoProductosServicio catalogo)
    {
        _uot = uot;
        _pedidos = pedidos;
        _catalogo = catalogo;
    }

    public async Task<List<DeliveryPedidoDto>> ListarPedidosDomicilioAsync(CancellationToken ct = default)
    {
        var pedidos = await _uot.Pedidos.ObtenerTodosAsync(ct);
        var repartidores = await ListarRepartidoresAsync(ct);
        var mapaNombres = repartidores.ToDictionary(r => r.Id, r => r.Nombre);

        return pedidos
            .Where(p => p.TipoServicio == TipoServicio.Delivery && p.Estado != EstadoPedido.Cancelado && p.Estado != EstadoPedido.AnuladoPago)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => MapDelivery(p, mapaNombres))
            .ToList();
    }

    public async Task<List<DeliveryPedidoDto>> ListarPedidosAsignadosAsync(Guid repartidorId, CancellationToken ct = default)
    {
        if (repartidorId == Guid.Empty)
            return [];

        var pedidos = await ListarPedidosDomicilioAsync(ct);
        return pedidos
            .Where(p => p.RepartidorId == repartidorId && !p.Entregado)
            .OrderBy(p => p.AsignadoEn ?? p.FechaCreacion)
            .ToList();
    }

    public async Task<DeliveryResumenDto> ObtenerResumenAsync(CancellationToken ct = default)
    {
        var pedidos = await ListarPedidosDomicilioAsync(ct);
        return new DeliveryResumenDto
        {
            TotalDomicilio = pedidos.Count,
            SinRepartidor = pedidos.Count(p => !p.RepartidorId.HasValue && !p.Entregado),
            EnRuta = pedidos.Count(p => p.RepartidorId.HasValue && !p.Entregado),
            Entregados = pedidos.Count(p => p.Entregado),
            VentasDomicilio = pedidos.Where(p => p.Pagado || p.Estado == "Despachado").Sum(p => p.Total)
        };
    }

    public async Task<List<RepartidorDto>> ListarRepartidoresAsync(CancellationToken ct = default)
    {
        var usuarios = await _uot.Usuarios.ObtenerTodosAsync(ct);
        return usuarios
            .Where(u => u.Rol is not null && u.Rol.Nombre == "Repartidor" && u.Activo)
            .Select(u => new RepartidorDto { Id = u.Id, Nombre = u.NombreCompleto, Activo = u.Activo })
            .OrderBy(r => r.Nombre)
            .ToList();
    }

    public async Task<List<ProductoDto>> ListarProductosAsync(CancellationToken ct = default)
    {
        var productos = await _catalogo.ListarProductosAsync(ct);
        return productos.Where(p => p.Activo).OrderBy(p => p.Nombre).ToList();
    }

    // Se conserva para compatibilidad, pero el flujo recomendado es crear el pedido desde Caja/POS
    // con TipoServicio = Delivery y datos de entrega.
    public async Task<Guid> CrearPedidoDomicilioAsync(string? direccion, string? telefono, Dictionary<Guid, int> items, CancellationToken ct = default)
    {
        if (items is null || items.Count == 0)
            throw new ReglaDominioException("Agregá al menos un producto al pedido.");

        var productos = (await _catalogo.ListarProductosAsync(ct)).ToDictionary(p => p.Id, p => p);
        var detalles = new List<DetalleCreacionDto>();
        foreach (var (productoId, cantidad) in items)
        {
            if (cantidad <= 0) continue;
            if (!productos.TryGetValue(productoId, out var prod))
                throw new ReglaDominioException("Producto inválido en el pedido.");
            detalles.Add(new DetalleCreacionDto
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                PrecioUnitario = prod.Precio,
                // Compatibilidad con validación backend de ingredientes: este flujo legacy no
                // expone modificadores, así que exige usar el POS para productos con receta.
                ModificacionesJson = null
            });
        }

        if (detalles.Count == 0)
            throw new ReglaDominioException("Agregá al menos un producto con cantidad mayor a cero.");

        var datosEntrega = new DatosEntregaDto
        {
            ClienteNombre = "Cliente delivery",
            Telefono = telefono,
            Direccion = direccion
        };
        var pedido = await _pedidos.CrearPedidoAsync(TipoServicio.Delivery, null, detalles, ct, datosEntrega);
        return pedido.Id;
    }

    public async Task AsignarRepartidorAsync(Guid pedidoId, Guid repartidorId, CancellationToken ct = default)
    {
        var repartidor = (await _uot.Usuarios.ObtenerTodosAsync(ct))
            .FirstOrDefault(u => u.Id == repartidorId && u.Activo && u.Rol is not null && u.Rol.Nombre == "Repartidor");

        if (repartidor is null)
            throw new ReglaDominioException("Seleccioná un repartidor activo válido.");

        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, ct)
            ?? throw new ReglaDominioException("No se encontró el pedido.");

        pedido.AsignarRepartidor(repartidorId);
        await _uot.GuardarCambiosAsync(ct);
    }

    public async Task MarcarEntregadoAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, ct)
            ?? throw new ReglaDominioException("No se encontró el pedido.");

        pedido.MarcarEntregado();
        await _uot.GuardarCambiosAsync(ct);
    }

    public async Task ActualizarDatosEntregaAsync(Guid pedidoId, string? direccion, string? telefono, CancellationToken ct = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, ct)
            ?? throw new ReglaDominioException("No se encontró el pedido.");

        pedido.ActualizarDatosEntrega(direccion, telefono);
        await _uot.GuardarCambiosAsync(ct);
    }

    private static DeliveryPedidoDto MapDelivery(LaMesaDelDuque.Dominio.Entidades.Pedido p, IReadOnlyDictionary<Guid, string> repartidores)
    {
        return new DeliveryPedidoDto
        {
            Id = p.Id,
            FechaCreacion = p.FechaCreacion,
            Estado = p.Estado.ToString(),
            Total = p.Total,
            Items = p.Detalles.Sum(d => d.Cantidad),
            DireccionEntrega = p.ClienteDeliveryDireccion,
            TelefonoCliente = p.ClienteDeliveryTelefono,
            RepartidorId = p.RepartidorId,
            RepartidorNombre = p.RepartidorId.HasValue && repartidores.TryGetValue(p.RepartidorId.Value, out var nombre) ? nombre : null,
            AsignadoEn = p.AsignadoEn,
            EntregadoEn = p.EntregadoEn,
            Pagado = p.EstaPagadoCompletamente || p.Estado is EstadoPedido.Pagado or EstadoPedido.Despachado
        };
    }
}
