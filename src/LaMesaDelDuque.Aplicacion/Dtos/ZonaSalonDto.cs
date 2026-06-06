namespace LaMesaDelDuque.Aplicacion.Dtos;

public class ZonaSalonDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activa { get; set; }
}
