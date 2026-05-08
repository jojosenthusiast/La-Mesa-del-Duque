using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class NuevasEntidadesJoinTests
{
    private static Rol _rol = new("cocinero");
    private static CategoriaProducto _categoria = new("Comidas");
    private static Usuario CrearUsuario() =>
        new("chef01", "chef@lmd.local", "$2a$12$hashValido", "Chef Principal", _rol);

    private static Producto CrearProducto() =>
        new("Hamburguesa", 8.50m, _categoria);

    private static Ingrediente CrearIngrediente() =>
        new("Carne molida", "kg", 10m, 1m, 5m);

    // ComboProducto tests
    [Fact]
    public void CrearComboProducto_CuandoCantidadEsCero_DebeLanzarExcepcion()
    {
        var combo = new Combo("Combo Clásico", 12m, new DateOnly(2026, 1, 1));
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new ComboProducto(combo, CrearProducto(), 0));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearComboProducto_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var combo = new Combo("Combo Familiar", 20m, new DateOnly(2026, 1, 1));
        var producto = CrearProducto();
        var cp = new ComboProducto(combo, producto, 2);

        Assert.Equal(combo.Id, cp.ComboId);
        Assert.Equal(producto.Id, cp.ProductoId);
        Assert.Equal(2, cp.Cantidad);
    }

    // PromocionProducto tests
    [Fact]
    public void CrearPromocionProducto_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var promo = new Promocion("Promo Fin de Semana", "fijo", 2m,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        var producto = CrearProducto();
        var pp = new PromocionProducto(promo, producto);

        Assert.Equal(promo.Id, pp.PromocionId);
        Assert.Equal(producto.Id, pp.ProductoId);
    }

    // OrdenCompraDetalle tests
    [Fact]
    public void CrearOrdenCompraDetalle_CuandoCantidadSolicitadaEsCero_DebeLanzarExcepcion()
    {
        var proveedor = new Proveedor("Dist.", "0614-250890-102-3");
        var orden = new OrdenCompra(proveedor, CrearUsuario());

        var ex = Assert.Throws<ReglaDominioException>(() =>
            new OrdenCompraDetalle(orden, CrearIngrediente(), 0m, 5m));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // MermaDiaria tests
    [Fact]
    public void CrearMermaDiaria_CuandoCantidadDescartadaEsCero_DebeLanzarExcepcion()
    {
        var usuario = CrearUsuario();
        var cierre = new CierreDia(new DateOnly(2026, 5, 1), 0m, 0m, 0m, 0, 0, 0m, usuario);

        var ex = Assert.Throws<ReglaDominioException>(() =>
            new MermaDiaria(cierre, CrearIngrediente(), 0m, usuario));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
