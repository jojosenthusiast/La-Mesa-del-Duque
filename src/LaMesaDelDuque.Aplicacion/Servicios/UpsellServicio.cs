using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IUpsellServicio
{
    Task<List<UpsellSugerenciaDto>> ObtenerSugerenciasAsync(Guid pedidoId, CancellationToken cancelacion = default);
}

public class UpsellServicio : IUpsellServicio
{
    private readonly IPedidosServicio _pedidos;
    private readonly ICatalogoProductosServicio _catalogo;

    public UpsellServicio(IPedidosServicio pedidos, ICatalogoProductosServicio catalogo)
    {
        _pedidos = pedidos;
        _catalogo = catalogo;
    }

    public async Task<List<UpsellSugerenciaDto>> ObtenerSugerenciasAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedidos = await _pedidos.ListarPedidosActivosAsync();
        var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);
        if (pedido is null) return [];

        var productos = await _catalogo.ListarProductosAsync();
        var activos = productos.Where(p => p.Activo).ToList();
        var sugerencias = new List<UpsellSugerenciaDto>();

        // Check if any dessert/bebida already in order
        var nombresEnPedido = pedido.Detalles.Select(d => d.ProductoNombre).ToHashSet();
        var categoriasEnPedido = activos.Where(p => nombresEnPedido.Contains(p.Nombre)).Select(p => p.CategoriaNombre).Distinct().ToList();
        var tienePostre = categoriasEnPedido.Any(c => c is "Postres" or "Postre");
        var tieneBebida = categoriasEnPedido.Any(c => c is "Bebidas" or "Bebida");

        if (!tienePostre)
        {
            var postres = activos.Where(p => p.CategoriaNombre is "Postres" or "Postre").Take(3).ToList();
            foreach (var p in postres)
                sugerencias.Add(new UpsellSugerenciaDto { ProductoId = p.Id, Nombre = p.Nombre, Precio = p.Precio, Razon = "Postre" });
        }

        if (!tieneBebida)
        {
            var bebidas = activos.Where(p => p.CategoriaNombre is "Bebidas" or "Bebida").Take(3).ToList();
            foreach (var p in bebidas)
                sugerencias.Add(new UpsellSugerenciaDto { ProductoId = p.Id, Nombre = p.Nombre, Precio = p.Precio, Razon = "Bebida" });
        }

        // If both already present, suggest a random high-margin item
        if (sugerencias.Count == 0)
        {
            var extra = activos.Where(p => p.Precio > 8).OrderBy(_ => Guid.NewGuid()).Take(2).ToList();
            foreach (var p in extra)
                sugerencias.Add(new UpsellSugerenciaDto { ProductoId = p.Id, Nombre = p.Nombre, Precio = p.Precio, Razon = "Recomendado" });
        }

        return sugerencias;
    }
}

public class UpsellSugerenciaDto
{
    public Guid ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Razon { get; set; } = string.Empty;
}
