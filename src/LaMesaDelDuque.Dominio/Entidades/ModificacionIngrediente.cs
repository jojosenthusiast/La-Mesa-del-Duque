using System.Text.Json;

namespace LaMesaDelDuque.Dominio.Entidades;

public class ModificacionIngrediente
{
    public Guid IngredienteId { get; set; }
    public string IngredienteNombre { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty; // "quitar", "extra", "intercambiar"
    public string Motivo { get; set; } = string.Empty; // "alergia", "preferencia", "intercambio"
    public string? IngredienteReemplazo { get; set; }
}
