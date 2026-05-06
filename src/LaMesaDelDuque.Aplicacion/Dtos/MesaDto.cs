namespace LaMesaDelDuque.Aplicacion.Dtos;

public class MesaDto
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activa { get; set; }
}
