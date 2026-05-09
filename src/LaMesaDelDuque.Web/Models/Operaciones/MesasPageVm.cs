using System.ComponentModel.DataAnnotations;
using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Web.Models.Operaciones;

public class MesasPageVm
{
    public List<MesaDto> Mesas { get; set; } = [];
    public Dictionary<string, int> ResumenPorEstado { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public MesaFormVm Form { get; set; } = new();
}

public class MesaFormVm
{
    public Guid? Id { get; set; }

    [Range(1, 999, ErrorMessage = "El número debe estar entre 1 y 999.")]
    public int Numero { get; set; }

    [Range(1, 30, ErrorMessage = "La capacidad debe estar entre 1 y 30.")]
    public int Capacidad { get; set; }
}
