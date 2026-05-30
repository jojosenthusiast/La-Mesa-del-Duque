using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Web.Models.Operaciones;

public class MapaSalonVm
{
    public List<ZonaSalonDto> Zonas { get; set; } = [];
    public List<MesaMapaItemVm> Mesas { get; set; } = [];
    public bool PuedeEditar { get; set; }
}

public class MesaMapaItemVm
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

    public string ClaseUrgencia => Estado switch
    {
        "Ocupada" => CalcularUrgencia(),
        "Disponible" => "lmd-mapa--disponible",
        "Reservada" => "lmd-mapa--reservada",
        "EnMantenimiento" => "lmd-mapa--mantenimiento",
        _ => "lmd-mapa--neutral"
    };

    private string CalcularUrgencia()
    {
        if (!OcupadaDesde.HasValue) return "lmd-mapa--ocupada-verde";
        var minutos = (DateTime.UtcNow - OcupadaDesde.Value).TotalMinutes;
        return minutos switch
        {
            > 30 => "lmd-mapa--ocupada-rojo",
            > 15 => "lmd-mapa--ocupada-amarillo",
            _ => "lmd-mapa--ocupada-verde"
        };
    }
}
