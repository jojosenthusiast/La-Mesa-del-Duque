namespace LaMesaDelDuque.Aplicacion.Dtos;

public record RecetaIngredienteCreacionDto(Guid IngredienteId, decimal CantidadRequerida);

public class RecetaIngredienteDto
{
    public Guid IngredienteId { get; set; }
    public string IngredienteNombre { get; set; } = string.Empty;
    public decimal CantidadRequerida { get; set; }
}

public class RecetaProductoDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string Instrucciones { get; set; } = string.Empty;
    public List<RecetaIngredienteDto> Ingredientes { get; set; } = [];
}
