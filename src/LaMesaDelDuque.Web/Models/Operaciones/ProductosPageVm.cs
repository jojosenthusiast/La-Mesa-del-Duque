using System.ComponentModel.DataAnnotations;
using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Web.Models.Operaciones;

public class ProductosPageVm
{
    public List<ProductoDto> Productos { get; set; } = [];
    public List<CategoriaProductoDto> Categorias { get; set; } = [];
    public string? Buscar { get; set; }
    public Guid? CategoriaId { get; set; }
    public string Estado { get; set; } = "todos";
    public ProductoFormVm Form { get; set; } = new();
}

public class ProductoFormVm
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.01, 999999, ErrorMessage = "El precio debe ser mayor que cero.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public Guid CategoriaId { get; set; }

    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public int? TiempoPreparacionMin { get; set; }
}
