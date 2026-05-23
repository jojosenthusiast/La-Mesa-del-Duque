using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IInventarioServicio
{
    Task<List<IngredienteDto>> ListarIngredientesAsync(CancellationToken ct = default);
    Task<IngredienteDto> CrearIngredienteAsync(GuardarIngredienteRequest req, CancellationToken ct = default);
    Task<IngredienteDto> ActualizarIngredienteAsync(Guid id, GuardarIngredienteRequest req, CancellationToken ct = default);
    Task AjustarStockAsync(Guid id, decimal nuevoStock, CancellationToken ct = default);
    Task ToggleIngredienteActivoAsync(Guid id, CancellationToken ct = default);

    Task<List<ProveedorDto>> ListarProveedoresAsync(CancellationToken ct = default);
    Task<ProveedorDetalleDto> ObtenerProveedorAsync(Guid id, CancellationToken ct = default);
    Task<ProveedorDto> CrearProveedorAsync(GuardarProveedorRequest req, CancellationToken ct = default);
    Task<ProveedorDto> ActualizarProveedorAsync(Guid id, GuardarProveedorRequest req, CancellationToken ct = default);
    Task ToggleProveedorActivoAsync(Guid id, CancellationToken ct = default);
}

public class InventarioServicio : IInventarioServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public InventarioServicio(IUnidadDeTrabajo uot) => _uot = uot;

    public async Task<List<IngredienteDto>> ListarIngredientesAsync(CancellationToken ct = default)
    {
        var lista = await _uot.Ingredientes.ObtenerTodosConProveedorAsync(ct);
        return lista.Select(MapIngrediente).ToList();
    }

    public async Task<IngredienteDto> CrearIngredienteAsync(GuardarIngredienteRequest req, CancellationToken ct = default)
    {
        Proveedor? proveedor = null;
        if (req.ProveedorId.HasValue)
            proveedor = await _uot.Proveedores!.ObtenerPorIdAsync(req.ProveedorId.Value, ct)
                ?? throw new ReglaDominioException("Proveedor no encontrado.");

        var ingrediente = new Ingrediente(req.Nombre, req.UnidadMedida, req.StockActual, req.StockMinimo, req.CostoUnitario, proveedor);
        await _uot.Ingredientes.AgregarAsync(ingrediente, ct);
        await _uot.GuardarCambiosAsync(ct);
        await SincronizarProductosAsync(ingrediente.Id, ct);
        return MapIngrediente(ingrediente);
    }

    public async Task<IngredienteDto> ActualizarIngredienteAsync(Guid id, GuardarIngredienteRequest req, CancellationToken ct = default)
    {
        var ingrediente = await _uot.Ingredientes.ObtenerPorIdConProveedorAsync(id, ct)
            ?? throw new ReglaDominioException("Ingrediente no encontrado.");

        Proveedor? proveedor = null;
        if (req.ProveedorId.HasValue)
            proveedor = await _uot.Proveedores!.ObtenerPorIdAsync(req.ProveedorId.Value, ct)
                ?? throw new ReglaDominioException("Proveedor no encontrado.");

        ingrediente.Actualizar(req.Nombre, req.UnidadMedida, req.StockMinimo, req.CostoUnitario);
        ingrediente.AsignarProveedorDefault(proveedor);
        await _uot.GuardarCambiosAsync(ct);
        await SincronizarProductosAsync(id, ct);
        return MapIngrediente(ingrediente);
    }

    public async Task AjustarStockAsync(Guid id, decimal nuevoStock, CancellationToken ct = default)
    {
        var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(id, ct)
            ?? throw new ReglaDominioException("Ingrediente no encontrado.");

        ingrediente.AjustarStock(nuevoStock);
        await _uot.GuardarCambiosAsync(ct);
        await SincronizarProductosAsync(id, ct);
    }

    public async Task ToggleIngredienteActivoAsync(Guid id, CancellationToken ct = default)
    {
        var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(id, ct)
            ?? throw new ReglaDominioException("Ingrediente no encontrado.");

        if (ingrediente.Activo) ingrediente.Desactivar(); else ingrediente.Activar();
        await _uot.GuardarCambiosAsync(ct);
        await SincronizarProductosAsync(id, ct);
    }

    public async Task<List<ProveedorDto>> ListarProveedoresAsync(CancellationToken ct = default)
    {
        var proveedores = await _uot.Proveedores!.ObtenerTodosAsync(ct);
        var ingredientes = await _uot.Ingredientes.ObtenerTodosConProveedorAsync(ct);
        var conteo = ingredientes
            .Where(i => i.ProveedorDefault != null)
            .GroupBy(i => i.ProveedorDefault!.Id)
            .ToDictionary(g => g.Key, g => g.Count());

        return proveedores.Select(p => MapProveedor(p, conteo.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<ProveedorDetalleDto> ObtenerProveedorAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _uot.Proveedores!.ObtenerPorIdAsync(id, ct)
            ?? throw new ReglaDominioException("Proveedor no encontrado.");

        var ingredientes = await _uot.Ingredientes.ObtenerPorProveedorIdAsync(id, ct);

        return new ProveedorDetalleDto
        {
            Id = proveedor.Id,
            Nombre = proveedor.Nombre,
            Nit = proveedor.Nit,
            Contacto = proveedor.Contacto,
            Telefono = proveedor.Telefono,
            Email = proveedor.Email,
            Direccion = proveedor.Direccion,
            Activo = proveedor.Activo,
            TotalIngredientes = ingredientes.Count,
            Ingredientes = ingredientes.Select(MapIngrediente).ToList()
        };
    }

    public async Task<ProveedorDto> CrearProveedorAsync(GuardarProveedorRequest req, CancellationToken ct = default)
    {
        var proveedor = new Proveedor(req.Nombre, req.Nit, req.Contacto, req.Telefono, req.Email, req.Direccion);
        await _uot.Proveedores!.AgregarAsync(proveedor, ct);
        await _uot.GuardarCambiosAsync(ct);
        return MapProveedor(proveedor, 0);
    }

    public async Task<ProveedorDto> ActualizarProveedorAsync(Guid id, GuardarProveedorRequest req, CancellationToken ct = default)
    {
        var proveedor = await _uot.Proveedores!.ObtenerPorIdAsync(id, ct)
            ?? throw new ReglaDominioException("Proveedor no encontrado.");

        proveedor.Actualizar(req.Nombre, req.Nit, req.Contacto, req.Telefono, req.Email, req.Direccion);
        await _uot.GuardarCambiosAsync(ct);
        return MapProveedor(proveedor, 0);
    }

    public async Task ToggleProveedorActivoAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _uot.Proveedores!.ObtenerPorIdAsync(id, ct)
            ?? throw new ReglaDominioException("Proveedor no encontrado.");

        if (proveedor.Activo) proveedor.Desactivar(); else proveedor.Activar();
        await _uot.GuardarCambiosAsync(ct);
    }

    private async Task SincronizarProductosAsync(Guid ingredienteId, CancellationToken ct)
    {
        var recetas = await _uot.RecetasProductos.ObtenerPorIngredienteAsync(ingredienteId, ct);
        if (recetas.Count == 0) return;

        foreach (var receta in recetas)
        {
            bool disponible = receta.Ingredientes.All(ri =>
                ri.Ingrediente.Activo && ri.Ingrediente.StockActual >= ri.CantidadRequerida);

            if (disponible && !receta.Producto.Activo) receta.Producto.Activar();
            else if (!disponible && receta.Producto.Activo) receta.Producto.Desactivar();
        }

        await _uot.GuardarCambiosAsync(ct);
    }

    private static IngredienteDto MapIngrediente(Ingrediente i) => new()
    {
        Id = i.Id,
        Nombre = i.Nombre,
        UnidadMedida = i.UnidadMedida,
        StockActual = i.StockActual,
        StockMinimo = i.StockMinimo,
        CostoUnitario = i.CostoUnitario,
        ProveedorId = i.ProveedorDefault?.Id,
        Proveedor = i.ProveedorDefault?.Nombre,
        Activo = i.Activo
    };

    private static ProveedorDto MapProveedor(Proveedor p, int totalIngredientes) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Nit = p.Nit,
        Contacto = p.Contacto,
        Telefono = p.Telefono,
        Email = p.Email,
        Direccion = p.Direccion,
        Activo = p.Activo,
        TotalIngredientes = totalIngredientes
    };
}

public class IngredienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string UnidadMedida { get; set; } = "";
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public Guid? ProveedorId { get; set; }
    public string? Proveedor { get; set; }
    public bool Activo { get; set; }
    public bool StockBajo => StockActual > 0 && StockActual <= StockMinimo;
    public bool SinStock => StockActual == 0;
}

public class ProveedorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Nit { get; set; } = "";
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; }
    public int TotalIngredientes { get; set; }
}

public class ProveedorDetalleDto : ProveedorDto
{
    public List<IngredienteDto> Ingredientes { get; set; } = [];
}

public class GuardarIngredienteRequest
{
    public string Nombre { get; set; } = "";
    public string UnidadMedida { get; set; } = "";
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public Guid? ProveedorId { get; set; }
}

public class GuardarProveedorRequest
{
    public string Nombre { get; set; } = "";
    public string Nit { get; set; } = "";
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}
