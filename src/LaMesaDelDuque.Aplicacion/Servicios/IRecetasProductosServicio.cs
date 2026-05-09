using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IRecetasProductosServicio
{
    Task<RecetaProductoDto> CrearRecetaAsync(Guid productoId, string instrucciones, List<RecetaIngredienteCreacionDto> ingredientes, CancellationToken cancelacion = default);
    Task<RecetaProductoDto?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancelacion = default);
}
