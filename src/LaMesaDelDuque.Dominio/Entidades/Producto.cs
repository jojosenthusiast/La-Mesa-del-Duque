using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Producto
{
    private const int LongitudMaximaNombre = 150;
    private const int LongitudMaximaImagenUrl = 500;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public CategoriaProducto Categoria { get; private set; }
    public bool Activo { get; private set; }
    public string? Descripcion { get; private set; }
    public string? ImagenUrl { get; private set; }
    public int TiempoPreparacionMin { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Producto()
    {
        Nombre = string.Empty;
        Categoria = null!;
    }

    public Producto(string nombre, decimal precio, CategoriaProducto categoria, string? imagenUrl = null, int tiempoPreparacionMin = 5)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del producto es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del producto no puede exceder {LongitudMaximaNombre} caracteres.");

        if (precio < 0)
            throw new ReglaDominioException("El precio no puede ser negativo.");

        if (categoria is null)
            throw new ReglaDominioException("El producto debe pertenecer a una categoría.");

        if (tiempoPreparacionMin <= 0)
            throw new ReglaDominioException("El tiempo de preparación debe ser mayor que cero.");

        if (!string.IsNullOrWhiteSpace(imagenUrl) && imagenUrl.Trim().Length > LongitudMaximaImagenUrl)
            throw new ReglaDominioException($"La URL de imagen no puede exceder {LongitudMaximaImagenUrl} caracteres.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Precio = precio;
        Categoria = categoria;
        ImagenUrl = string.IsNullOrWhiteSpace(imagenUrl) ? null : imagenUrl.Trim();
        TiempoPreparacionMin = tiempoPreparacionMin;
        Activo = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarDatos(string nombre, decimal precio, CategoriaProducto categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del producto es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del producto no puede exceder {LongitudMaximaNombre} caracteres.");

        if (precio < 0)
            throw new ReglaDominioException("El precio no puede ser negativo.");

        if (categoria is null)
            throw new ReglaDominioException("El producto debe pertenecer a una categoría.");

        Nombre = nombre.Trim();
        Precio = precio;
        Categoria = categoria;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarDescripcion(string? descripcion)
    {
        Descripcion = descripcion;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarImagen(string? imagenUrl)
    {
        if (!string.IsNullOrWhiteSpace(imagenUrl) && imagenUrl.Trim().Length > LongitudMaximaImagenUrl)
            throw new ReglaDominioException($"La URL de imagen no puede exceder {LongitudMaximaImagenUrl} caracteres.");

        ImagenUrl = string.IsNullOrWhiteSpace(imagenUrl) ? null : imagenUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarTiempoPreparacion(int tiempoPreparacionMin)
    {
        if (tiempoPreparacionMin <= 0)
            throw new ReglaDominioException("El tiempo de preparación debe ser mayor que cero.");

        TiempoPreparacionMin = tiempoPreparacionMin;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }
}
