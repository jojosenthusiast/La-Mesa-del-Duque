using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using LaMesaDelDuque.Web.Pages.Operaciones.Productos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace LaMesaDelDuque.Pruebas.Web;

public class ProductosPageTests
{
    private static FakeWebHostEnvironment CreateFakeEnv()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"lmd-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(Path.Combine(tempPath, "uploads", "productos"));
        return new FakeWebHostEnvironment(tempPath);
    }

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

        var page = new IndexModel(servicio, CreateFakeEnv())
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

        var page = new IndexModel(servicio, CreateFakeEnv())
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

    [Fact]
    public async Task OnPostGuardarAsync_rejects_invalid_file_extension()
    {
        var servicio = new FakeCatalogoProductosServicio();
        var env = CreateFakeEnv();
        var page = new IndexModel(servicio, env)
        {
            Vm = new ProductosPageVm
            {
                Form = new ProductoFormVm
                {
                    Nombre = "Producto con foto",
                    Precio = 1500m,
                    CategoriaId = Guid.NewGuid(),
                    ImagenFile = FakeFormFile.FromString("archivo.pdf", "contenido", "application/pdf")
                }
            }
        };

        var result = await page.OnPostGuardarAsync();

        Assert.IsType<Microsoft.AspNetCore.Mvc.RazorPages.PageResult>(result);
        Assert.False(page.ModelState.IsValid);
        Assert.Contains(page.ModelState, kvp => kvp.Key == "Vm.Form.ImagenFile" && kvp.Value?.Errors.Any(e => e.ErrorMessage.Contains("JPG")) == true);
    }

    [Fact]
    public async Task OnPostGuardarAsync_rejects_oversized_file()
    {
        var servicio = new FakeCatalogoProductosServicio();
        var env = CreateFakeEnv();
        var largeContent = new byte[6 * 1024 * 1024]; // 6MB
        Array.Fill(largeContent, (byte)0xFF);
        var page = new IndexModel(servicio, env)
        {
            Vm = new ProductosPageVm
            {
                Form = new ProductoFormVm
                {
                    Nombre = "Producto con foto",
                    Precio = 1500m,
                    CategoriaId = Guid.NewGuid(),
                    ImagenFile = FakeFormFile.FromBytes("foto.jpg", largeContent, "image/jpeg")
                }
            }
        };

        var result = await page.OnPostGuardarAsync();

        Assert.IsType<Microsoft.AspNetCore.Mvc.RazorPages.PageResult>(result);
        Assert.False(page.ModelState.IsValid);
        Assert.Contains(page.ModelState, kvp => kvp.Key == "Vm.Form.ImagenFile" && kvp.Value?.Errors.Any(e => e.ErrorMessage.Contains("5MB")) == true);
    }

    [Fact]
    public async Task OnPostGuardarAsync_saves_file_and_sets_imagen_url_for_new_product()
    {
        var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas", Activo = true };
        var servicio = new FakeCatalogoProductosServicio
        {
            Categorias = [categoria]
        };
        var env = CreateFakeEnv();
        var page = new IndexModel(servicio, env)
        {
            Vm = new ProductosPageVm
            {
                Form = new ProductoFormVm
                {
                    Nombre = "Café con foto",
                    Precio = 2500m,
                    CategoriaId = categoria.Id,
                    ImagenFile = FakeFormFile.FromString("foto.jpg", "imagen-de-prueba", "image/jpeg")
                }
            }
        };

        var result = await page.OnPostGuardarAsync();

        Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToPageResult>(result);
        // Verify the created product got an ImagenUrl set via update
        Assert.NotNull(servicio.LastImagenUrl);
        Assert.Contains("/uploads/productos/", servicio.LastImagenUrl);
    }

    [Fact]
    public async Task OnPostEliminarFotoAsync_removes_file_and_clears_url()
    {
        var productoId = Guid.NewGuid();
        var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas", Activo = true };
        var servicio = new FakeCatalogoProductosServicio
        {
            Categorias = [categoria],
            Productos =
            [
                new ProductoDto { Id = productoId, Nombre = "Café", CategoriaId = categoria.Id, CategoriaNombre = "Bebidas", Precio = 2500m, Activo = true, ImagenUrl = "/uploads/productos/test.jpg", TiempoPreparacionMin = 5 }
            ]
        };
        var env = CreateFakeEnv();
        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "productos");
        Directory.CreateDirectory(uploadsDir);
        await File.WriteAllTextAsync(Path.Combine(uploadsDir, $"{productoId}.jpg"), "fake-image");

        var page = new IndexModel(servicio, env);

        var result = await page.OnPostEliminarFotoAsync(productoId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToPageResult>(result);
        Assert.Null(servicio.LastImagenUrl);
        Assert.False(File.Exists(Path.Combine(uploadsDir, $"{productoId}.jpg")));
    }
}

internal sealed class FakeCatalogoProductosServicio : ICatalogoProductosServicio
{
    public List<ProductoDto> Productos { get; set; } = [];
    public List<CategoriaProductoDto> Categorias { get; set; } = [];
    public Exception? ThrowOnGuardar { get; set; }
    public string? LastImagenUrl { get; set; }

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

        LastImagenUrl = imagenUrl;

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

internal sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public FakeWebHostEnvironment(string webRootPath)
    {
        WebRootPath = webRootPath;
        ContentRootPath = webRootPath;
        WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRootPath);
        ContentRootFileProvider = WebRootFileProvider;
    }

    public string ApplicationName { get; set; } = "Test";
    public IFileProvider ContentRootFileProvider { get; set; }
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; } = "Development";
    public IFileProvider WebRootFileProvider { get; set; }
    public string WebRootPath { get; set; }
}

internal sealed class FakeFormFile : IFormFile
{
    private readonly byte[] _content;

    public FakeFormFile(string fileName, byte[] content, string contentType)
    {
        FileName = fileName;
        _content = content;
        ContentType = contentType;
        Length = content.Length;
    }

    public string ContentType { get; }
    public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{FileName}\"";
    public IHeaderDictionary Headers => new HeaderDictionary();
    public long Length { get; }
    public string Name => "file";
    public string FileName { get; }

    public Stream OpenReadStream() => new MemoryStream(_content);
    public void CopyTo(Stream target) => target.Write(_content, 0, _content.Length);
    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        => await target.WriteAsync(_content, cancellationToken);

    public static IFormFile FromString(string fileName, string content, string contentType)
        => new FakeFormFile(fileName, System.Text.Encoding.UTF8.GetBytes(content), contentType);

    public static IFormFile FromBytes(string fileName, byte[] content, string contentType)
        => new FakeFormFile(fileName, content, contentType);
}
