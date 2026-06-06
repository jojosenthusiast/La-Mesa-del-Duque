using System.ComponentModel.DataAnnotations;
using LaMesaDelDuque.Aplicacion.Dtos;
using Microsoft.AspNetCore.Http;

namespace LaMesaDelDuque.Web.Models.Operaciones;

public class ProductosPageVm
{
    public List<ProductoDto> Productos { get; set; } = [];
    public List<CategoriaProductoDto> Categorias { get; set; } = [];
    public string? Buscar { get; set; }
    public Guid? CategoriaId { get; set; }
    public string Estado { get; set; } = "todos";
    public ProductoFormVm Form { get; set; } = new();
    public int TotalProductos { get; set; }
    public int TotalVisibles { get; set; }
    public Guid? ProductoGuardadoId { get; set; }
    public bool ProductoGuardadoVisible { get; set; }
    public bool ProductoGuardadoOcultoPorFiltros => ProductoGuardadoId.HasValue && !ProductoGuardadoVisible;
    public bool TieneProductos => TotalProductos > 0;
    public bool TieneCategorias => Categorias.Count > 0;
    public bool FiltrosAplicados => !string.IsNullOrWhiteSpace(Buscar) || CategoriaId.HasValue || !string.Equals(Estado, "todos", StringComparison.OrdinalIgnoreCase);
    public string DescripcionFiltros { get; set; } = "Sin filtros aplicados.";
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

    [Display(Name = "Foto del producto")]
    public IFormFile? ImagenFile { get; set; }

    public bool EliminarImagen { get; set; }
}
