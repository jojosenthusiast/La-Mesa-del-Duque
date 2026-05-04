namespace LaMesaDelDuque.Aplicacion.Dtos;

public class CategoriaProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
