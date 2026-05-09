using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class RecetasProductosServicio : IRecetasProductosServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public RecetasProductosServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<RecetaProductoDto> CrearRecetaAsync(Guid productoId, string instrucciones, List<RecetaIngredienteCreacionDto> ingredientes, CancellationToken cancelacion = default)
    {
        var producto = await _uot.Productos.ObtenerConTrackingAsync(productoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el producto con ID {productoId}.", nameof(productoId));

        var ingredientesReceta = new List<RecetaIngrediente>();
        foreach (var ingredienteDto in ingredientes)
        {
            var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(ingredienteDto.IngredienteId, cancelacion)
                ?? throw new ArgumentException($"No se encontró el ingrediente con ID {ingredienteDto.IngredienteId}.", nameof(ingredientes));

            ingredientesReceta.Add(new RecetaIngrediente(ingrediente, ingredienteDto.CantidadRequerida));
        }

        var receta = new RecetaProducto(producto, instrucciones, ingredientesReceta);
        await _uot.RecetasProductos.AgregarAsync(receta, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(receta);
    }

    public async Task<RecetaProductoDto?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancelacion = default)
    {
        var receta = await _uot.RecetasProductos.ObtenerPorProductoIdAsync(productoId, cancelacion);
        return receta is null ? null : MapToDto(receta);
    }

    private static RecetaProductoDto MapToDto(RecetaProducto receta)
    {
        return new RecetaProductoDto
        {
            Id = receta.Id,
            ProductoId = receta.ProductoId,
            ProductoNombre = receta.Producto.Nombre,
            Instrucciones = receta.Instrucciones,
            Ingredientes = receta.Ingredientes.Select(i => new RecetaIngredienteDto
            {
                IngredienteId = i.IngredienteId,
                IngredienteNombre = i.Ingrediente.Nombre,
                CantidadRequerida = i.CantidadRequerida
            }).ToList()
        };
    }
}
