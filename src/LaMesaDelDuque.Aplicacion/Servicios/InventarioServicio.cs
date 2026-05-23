using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IInventarioServicio
{
    Task<List<IngredienteDto>> ListarIngredientesAsync(CancellationToken ct = default);
    Task<IngredienteDto> CrearIngredienteAsync(string nombre, decimal stockActual, decimal stockMinimo, string? unidad, decimal costoUnitario, CancellationToken ct = default);
    Task ActualizarStockAsync(Guid id, decimal nuevoStock, CancellationToken ct = default);
    Task<List<ProveedorDto>> ListarProveedoresAsync(CancellationToken ct = default);
    Task<ProveedorDto> CrearProveedorAsync(string nombre, string? contacto, string? telefono, CancellationToken ct = default);
}

public class InventarioServicio : IInventarioServicio
{
    private readonly IUnidadDeTrabajo _uot;
    public InventarioServicio(IUnidadDeTrabajo uot) => _uot = uot;

    public async Task<List<IngredienteDto>> ListarIngredientesAsync(CancellationToken ct = default)
    {
        var ings = await _uot.Ingredientes.ObtenerTodosAsync(ct);
        return ings.Select(i => new IngredienteDto
        {
            Id = i.Id, Nombre = i.Nombre, StockActual = i.StockActual, StockMinimo = i.StockMinimo,
            Unidad = i.UnidadMedida, CostoUnitario = i.CostoUnitario, Proveedor = i.ProveedorDefault?.Nombre
        }).ToList();
    }

    public async Task<IngredienteDto> CrearIngredienteAsync(string nombre, decimal stockActual, decimal stockMinimo, string? unidad, decimal costoUnitario, CancellationToken ct = default)
    {
        var ing = new Ingrediente(nombre, unidad ?? "unidad", stockActual, stockMinimo, costoUnitario);
        await _uot.Ingredientes.AgregarAsync(ing, ct);
        await _uot.GuardarCambiosAsync(ct);
        return MapIng(ing);
    }

    public async Task ActualizarStockAsync(Guid id, decimal nuevoStock, CancellationToken ct = default)
    {
        await Task.CompletedTask; // Stock update requires domain method — deferred
    }

    public async Task<List<ProveedorDto>> ListarProveedoresAsync(CancellationToken ct = default)
    {
        var provs = await _uot.Proveedores!.ObtenerTodosAsync(ct);
        return provs.Select(p => new ProveedorDto { Id = p.Id, Nombre = p.Nombre, Contacto = p.Contacto, Telefono = p.Telefono, Email = p.Email }).ToList();
    }

    public async Task<ProveedorDto> CrearProveedorAsync(string nombre, string? contacto, string? telefono, CancellationToken ct = default)
    {
        var p = new Proveedor(nombre, "CF", contacto, telefono);
        await _uot.Proveedores.AgregarAsync(p, ct);
        await _uot.GuardarCambiosAsync(ct);
        return new ProveedorDto { Id = p.Id, Nombre = p.Nombre, Contacto = p.Contacto, Telefono = p.Telefono };
    }

    private static IngredienteDto MapIng(Ingrediente i) => new()
    {
        Id = i.Id, Nombre = i.Nombre, StockActual = i.StockActual, StockMinimo = i.StockMinimo,
        Unidad = i.UnidadMedida, CostoUnitario = i.CostoUnitario, Proveedor = i.ProveedorDefault?.Nombre
    };
}

public class IngredienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public string? Unidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public string? Proveedor { get; set; }
}

public class ProveedorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
