using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class ProductoPrecioHistorial
{
    public Guid Id { get; private set; }
    public Guid ProductoId { get; private set; }
    public Producto Producto { get; private set; }
    public decimal PrecioAnterior { get; private set; }
    public decimal PrecioNuevo { get; private set; }
    public string Razon { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; }
    public DateTime FechaCambio { get; private set; }

    private ProductoPrecioHistorial()
    {
        Producto = null!;
        Usuario = null!;
        Razon = string.Empty;
    }

    public ProductoPrecioHistorial(Producto producto, decimal precioAnterior, decimal precioNuevo, string razon, Usuario usuario)
    {
        if (producto is null)
            throw new ReglaDominioException("El producto es obligatorio para historial de precio.");
        if (precioAnterior < 0)
            throw new ReglaDominioException("El precio anterior no puede ser negativo.");
        if (precioNuevo < 0)
            throw new ReglaDominioException("El precio nuevo no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(razon))
            throw new ReglaDominioException("La razón del cambio de precio es obligatoria.");
        if (razon.Trim().Length > 500)
            throw new ReglaDominioException("La razón del cambio de precio no puede exceder 500 caracteres.");
        if (usuario is null)
            throw new ReglaDominioException("El usuario es obligatorio para historial de precio.");

        Id = Guid.NewGuid();
        Producto = producto;
        ProductoId = producto.Id;
        PrecioAnterior = precioAnterior;
        PrecioNuevo = precioNuevo;
        Razon = razon.Trim();
        Usuario = usuario;
        UsuarioId = usuario.Id;
        FechaCambio = DateTime.UtcNow;
    }
}
