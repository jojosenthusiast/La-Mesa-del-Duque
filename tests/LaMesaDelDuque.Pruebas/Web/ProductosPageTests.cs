using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Pages.Operaciones.Productos;

namespace LaMesaDelDuque.Pruebas.Web;

public class ProductosPageTests
{
    [Fact]
    public async Task OnGetAsync_applies_filters_and_loads_categories()
    {
        var categoriaBebidas = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas", Activo = true };
        var categoriaCocina = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Cocina", Activo = true };

        var servicio = new FakeCatalogoProductosServicio
        {
            Categorias =
            [
                categoriaBebidas,
                categoriaCocina
            ],
            Productos =
            [
                new ProductoDto { Id = Guid.NewGuid(), Nombre = "Café", CategoriaId = categoriaBebidas.Id, CategoriaNombre = "Bebidas", Precio = 2300m, Activo = true },
                new ProductoDto { Id = Guid.NewGuid(), Nombre = "Tostada", CategoriaId = categoriaCocina.Id, CategoriaNombre = "Cocina", Precio = 5200m, Activo = false }
            ]
        };

        var page = new IndexModel(servicio)
        {
            Buscar = "caf",
            CategoriaId = categoriaBebidas.Id,
            Estado = "activos"
        };

        await page.OnGetAsync();

        Assert.Single(page.Vm.Productos);
        Assert.Equal("Café", page.Vm.Productos[0].Nombre);
        Assert.Equal(2, page.Vm.Categorias.Count);
    }

    [Fact]
    public async Task OnPostGuardarAsync_maps_domain_exception_to_model_state()
    {
        var servicio = new FakeCatalogoProductosServicio
        {
            ThrowOnGuardar = new ReglaDominioException("Precio inválido para la categoría seleccionada.")
        };

        var page = new IndexModel(servicio)
        {
            Vm =
            {
                Form = new()
                {
                    Nombre = "Producto test",
                    Precio = 1500m,
                    CategoriaId = Guid.NewGuid()
                }
            }
        };

        var result = await page.OnPostGuardarAsync();

        Assert.IsType<Microsoft.AspNetCore.Mvc.RazorPages.PageResult>(result);
        Assert.False(page.ModelState.IsValid);
    }
}

internal sealed class FakeCatalogoProductosServicio : ICatalogoProductosServicio
{
    public List<ProductoDto> Productos { get; set; } = [];
    public List<CategoriaProductoDto> Categorias { get; set; } = [];
    public Exception? ThrowOnGuardar { get; set; }

    public Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default) => Task.FromResult(Categorias);
    public Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default) => Task.FromResult(Productos);
    public Task<List<ProductoDto>> ListarProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => Task.FromResult(Productos.Where(p => p.CategoriaId == categoriaId).ToList());

    public Task<ProductoDto> CrearProductoAsync(string nombre, decimal precio, Guid categoriaId, string? descripcion = null, string? imagenUrl = null, int tiempoPreparacionMin = 5, CancellationToken cancelacion = default)
    {
        if (ThrowOnGuardar is not null)
        {
            throw ThrowOnGuardar;
        }

        return Task.FromResult(new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Precio = precio,
            CategoriaId = categoriaId,
            CategoriaNombre = Categorias.FirstOrDefault(c => c.Id == categoriaId)?.Nombre ?? string.Empty,
            Descripcion = descripcion,
            ImagenUrl = imagenUrl,
            TiempoPreparacionMin = tiempoPreparacionMin,
            Activo = true
        });
    }

    public Task<ProductoDto> ActualizarProductoAsync(Guid productoId, string nombre, decimal precio, Guid categoriaId, string? descripcion, string? imagenUrl = null, int? tiempoPreparacionMin = null, CancellationToken cancelacion = default)
    {
        if (ThrowOnGuardar is not null)
        {
            throw ThrowOnGuardar;
        }

        return Task.FromResult(new ProductoDto
        {
            Id = productoId,
            Nombre = nombre,
            Precio = precio,
            CategoriaId = categoriaId,
            CategoriaNombre = Categorias.FirstOrDefault(c => c.Id == categoriaId)?.Nombre ?? string.Empty,
            Descripcion = descripcion,
            ImagenUrl = imagenUrl,
            TiempoPreparacionMin = tiempoPreparacionMin ?? 5,
            Activo = true
        });
    }

    public Task DesactivarProductoAsync(Guid productoId, CancellationToken cancelacion = default)
        => Task.CompletedTask;
}
