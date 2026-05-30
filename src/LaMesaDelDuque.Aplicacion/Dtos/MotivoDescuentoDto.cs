namespace LaMesaDelDuque.Aplicacion.Dtos;

public class MotivoDescuentoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}
