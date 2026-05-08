using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class OrdenCompra
{
    private static readonly string[] EstadosValidos = ["solicitado", "en_camino", "recibido", "fallo"];

    public Guid Id { get; private set; }
    public Guid ProveedorId { get; private set; }
    public Proveedor Proveedor { get; private set; }
    public string Estado { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public DateTime? FechaRecepcion { get; private set; }
    public string? Notas { get; private set; }
    public string? ImpactoFallo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OrdenCompra()
    {
        Proveedor = null!;
        Estado = string.Empty;
        Usuario = null!;
    }

    public OrdenCompra(Proveedor proveedor, Usuario usuario, string? notas = null)
    {
        if (proveedor is null)
            throw new ReglaDominioException("El proveedor es obligatorio para la orden de compra.");

        if (usuario is null)
            throw new ReglaDominioException("El usuario es obligatorio para la orden de compra.");

        Id = Guid.NewGuid();
        Proveedor = proveedor;
        ProveedorId = proveedor.Id;
        Usuario = usuario;
        UsuarioId = usuario.Id;
        Estado = "solicitado";
        FechaSolicitud = DateTime.UtcNow;
        Notas = notas;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarEstado(string nuevoEstado, DateTime? fechaRecepcion = null, string? impactoFallo = null)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado) || !Array.Exists(EstadosValidos, e => e == nuevoEstado.Trim().ToLowerInvariant()))
            throw new ReglaDominioException("El estado de la orden debe ser 'solicitado', 'en_camino', 'recibido' o 'fallo'.");

        Estado = nuevoEstado.Trim().ToLowerInvariant();

        if (Estado == "recibido" && fechaRecepcion.HasValue)
            FechaRecepcion = fechaRecepcion;

        if (Estado == "fallo")
            ImpactoFallo = impactoFallo;

        UpdatedAt = DateTime.UtcNow;
    }
}
