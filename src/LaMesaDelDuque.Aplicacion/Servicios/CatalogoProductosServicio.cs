using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class CatalogoProductosServicio : ICatalogoProductosServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public CatalogoProductosServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default)
    {
        var categorias = await _uot.Categorias.ObtenerTodasAsync(cancelacion);
        return categorias.Select(c => new CategoriaProductoDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Activo = c.Activo
        }).ToList();
    }

    public async Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la categoría es obligatorio.", nameof(nombre));

        var categoria = new CategoriaProducto(nombre.Trim());
        await _uot.Categorias.AgregarAsync(categoria, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return new CategoriaProductoDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Activo = categoria.Activo
        };
    }

    public async Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default)
    {
        var productos = await _uot.Productos.ObtenerTodosAsync(cancelacion);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var dtos = new List<ProductoDto>(productos.Count);
        foreach (var producto in productos)
        {
            var dto = MapToDto(producto);
            var promos = await _uot.Promociones.ObtenerActivasPorProductoAsync(producto.Id, hoy, cancelacion);
            if (promos.Count > 0)
            {
                var mejor = promos.OrderByDescending(pp =>
                    pp.Promocion.TipoDescuento == "porcentaje"
                        ? producto.Precio * pp.Promocion.ValorDescuento / 100
                        : pp.Promocion.ValorDescuento
                ).First();
                dto.PromoNombre = mejor.Promocion.Nombre;
                dto.PromoDescuento = mejor.Promocion.ValorDescuento;
                dto.PromoTipo = mejor.Promocion.TipoDescuento;
            }
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<List<ProductoDto>> ListarProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default)
    {
        var productos = await _uot.Productos.ObtenerPorCategoriaAsync(categoriaId, cancelacion);
        return productos.Select(MapToDto).ToList();
    }

    public async Task<ProductoDto> CrearProductoAsync(string nombre, decimal precio, Guid categoriaId, string? descripcion = null, string? imagenUrl = null, int tiempoPreparacionMin = 5, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del producto es obligatorio.", nameof(nombre));

        if (precio <= 0)
            throw new ArgumentException("El precio del producto debe ser mayor que cero.", nameof(precio));

        // Usamos tracking para evitar conflicto de identidad con la categoría
        // ya trackeada en el contexto tras crear el producto que la referencia.
        var categoria = await _uot.Categorias.ObtenerConTrackingAsync(categoriaId, cancelacion);
        if (categoria is null)
            throw new ArgumentException($"No se encontró la categoría con ID {categoriaId}.", nameof(categoriaId));

        var producto = new Producto(nombre.Trim(), precio, categoria, imagenUrl, tiempoPreparacionMin);
        producto.ActualizarDescripcion(descripcion);
        await _uot.Productos.AgregarAsync(producto, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(producto);
    }

    public async Task<ProductoDto> ActualizarProductoAsync(Guid productoId, string nombre, decimal precio, Guid categoriaId, string? descripcion, string? imagenUrl = null, int? tiempoPreparacionMin = null, CancellationToken cancelacion = default)
    {
        var producto = await _uot.Productos.ObtenerConTrackingAsync(productoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el producto con ID {productoId}.", nameof(productoId));

        var categoria = await _uot.Categorias.ObtenerConTrackingAsync(categoriaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la categoría con ID {categoriaId}.", nameof(categoriaId));

        producto.ActualizarDatos(nombre, precio, categoria);
        producto.ActualizarDescripcion(descripcion);
        producto.ActualizarImagen(imagenUrl);
        if (tiempoPreparacionMin.HasValue)
            producto.ActualizarTiempoPreparacion(tiempoPreparacionMin.Value);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(producto);
    }

    public async Task DesactivarProductoAsync(Guid productoId, CancellationToken cancelacion = default)
    {
        var producto = await _uot.Productos.ObtenerConTrackingAsync(productoId, cancelacion)
            ?? throw new ArgumentException($"No se encontró el producto con ID {productoId}.", nameof(productoId));

        var tienePedidosActivos = await _uot.Productos.ExisteEnPedidosActivosAsync(productoId, cancelacion);
        if (tienePedidosActivos)
            throw new ReglaDominioException("No se puede desactivar el producto porque tiene pedidos activos.");

        producto.Desactivar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    public async Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la categoría es obligatorio.", nameof(nombre));

        var categoria = await _uot.Categorias.ObtenerConTrackingAsync(categoriaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la categoría con ID {categoriaId}.", nameof(categoriaId));

        categoria.ActualizarNombre(nombre.Trim());
        await _uot.GuardarCambiosAsync(cancelacion);

        return new CategoriaProductoDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Activo = categoria.Activo
        };
    }

    public async Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default)
    {
        var categoria = await _uot.Categorias.ObtenerConTrackingAsync(categoriaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la categoría con ID {categoriaId}.", nameof(categoriaId));

        categoria.Desactivar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    private static ProductoDto MapToDto(Producto producto)
    {
        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            CategoriaId = producto.Categoria.Id,
            CategoriaNombre = producto.Categoria.Nombre,
            Activo = producto.Activo,
            Descripcion = producto.Descripcion,
            ImagenUrl = producto.ImagenUrl,
            TiempoPreparacionMin = producto.TiempoPreparacionMin
        };
    }
}
