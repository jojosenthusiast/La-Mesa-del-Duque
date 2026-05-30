namespace LaMesaDelDuque.Aplicacion.Dtos;

public class ProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public int TiempoPreparacionMin { get; set; }
    public string? PromoNombre { get; set; }
    public decimal? PromoDescuento { get; set; }
    public string? PromoTipo { get; set; }
}
