namespace LaMesaDelDuque.Aplicacion.Dtos;

public class MesaDto
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public int? PosicionX { get; set; }
    public int? PosicionY { get; set; }
    public Guid? ZonaId { get; set; }
    public string? Forma { get; set; }
    public int? Rotacion { get; set; }
    public DateTime? OcupadaDesde { get; set; }
}
