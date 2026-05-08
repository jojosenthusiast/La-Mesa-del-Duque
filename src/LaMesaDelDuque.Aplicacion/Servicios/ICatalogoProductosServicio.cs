using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ICatalogoProductosServicio
{
    Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default);
    Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default);
    Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default);
    Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default);
    Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default);
    Task<List<ProductoDto>> ListarProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default);
    Task<ProductoDto> CrearProductoAsync(string nombre, decimal precio, Guid categoriaId, CancellationToken cancelacion = default);
    Task<ProductoDto> ActualizarProductoAsync(Guid productoId, string nombre, decimal precio, Guid categoriaId, string? descripcion, CancellationToken cancelacion = default);
    Task DesactivarProductoAsync(Guid productoId, CancellationToken cancelacion = default);
}
