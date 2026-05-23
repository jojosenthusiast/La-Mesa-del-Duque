namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IMargenServicio
{
    Task<List<MargenProductoDto>> CalcularMargenesAsync(CancellationToken ct = default);
}

public class MargenServicio : IMargenServicio
{
    private readonly ICatalogoProductosServicio _catalogo;
    private readonly IRecetasProductosServicio _recetas;

    public MargenServicio(ICatalogoProductosServicio catalogo, IRecetasProductosServicio recetas)
    {
        _catalogo = catalogo;
        _recetas = recetas;
    }

    public async Task<List<MargenProductoDto>> CalcularMargenesAsync(CancellationToken ct = default)
    {
        var productos = await _catalogo.ListarProductosAsync();
        var resultado = new List<MargenProductoDto>();

        foreach (var p in productos.Where(p => p.Activo))
        {
            var receta = await _recetas.ObtenerPorProductoIdAsync(p.Id);
            var costoTotal = receta?.Ingredientes.Sum(i => i.CantidadRequerida) ?? 0;
            var margen = p.Precio > 0 ? ((p.Precio - costoTotal) / p.Precio) * 100 : 0;

            resultado.Add(new MargenProductoDto
            {
                ProductoId = p.Id,
                Nombre = p.Nombre,
                PrecioVenta = p.Precio,
                CostoEstimado = costoTotal,
                MargenPorcentaje = Math.Round((double)margen, 1),
                Rentabilidad = margen >= 60 ? "Alta" : margen >= 30 ? "Media" : "Baja"
            });
        }

        return resultado.OrderByDescending(r => r.MargenPorcentaje).ToList();
    }
}

public class MargenProductoDto
{
    public Guid ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public decimal PrecioVenta { get; set; }
    public decimal CostoEstimado { get; set; }
    public double MargenPorcentaje { get; set; }
    public string Rentabilidad { get; set; } = "";
}
