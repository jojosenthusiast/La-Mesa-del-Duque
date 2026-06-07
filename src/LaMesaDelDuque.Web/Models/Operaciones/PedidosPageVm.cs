using System.ComponentModel.DataAnnotations;
using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Web.Models.Operaciones;

public class PedidosPageVm
{
    public List<MesaDto> MesasDisponibles { get; set; } = [];
    public List<ProductoDto> ProductosDisponibles { get; set; } = [];
    public PedidoDto? PedidoActual { get; set; }
    public List<PedidoDto> PedidosActivos { get; set; } = [];
    public List<PedidoDto> PedidosPendientesDespacho { get; set; } = [];
    public CrearPedidoFormVm CrearPedido { get; set; } = new();
    public PagoFormVm Pago { get; set; } = new();
    public bool MostrarPago { get; set; }
}

public class CrearPedidoFormVm
{
    [Required]
    public string TipoServicio { get; set; } = "ComerAqui";

    public Guid? MesaId { get; set; }

    [MaxLength(120)]
    public string? ClienteDeliveryNombre { get; set; }

    [MaxLength(40)]
    public string? ClienteDeliveryTelefono { get; set; }

    [MaxLength(300)]
    public string? ClienteDeliveryDireccion { get; set; }

    [MaxLength(200)]
    public string? ClienteDeliveryReferencia { get; set; }

    [MaxLength(300)]
    public string? ClienteDeliveryNotas { get; set; }

    [MinLength(1, ErrorMessage = "Debe incluir al menos una línea en el pedido.")]
    public List<LineaPedidoFormVm> Lineas { get; set; } = [];
}

public class LineaPedidoFormVm
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(1, 999)]
    public int Cantidad { get; set; } = 1;

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal PrecioUnitario { get; set; }

    [MaxLength(250)]
    public string? Notas { get; set; }

    public string? ModificacionesJson { get; set; }
}

public class PagoFormVm
{
    [Range(0, 999999, ErrorMessage = "El monto recibido debe ser mayor o igual a 0.")]
    public decimal? EfectivoRecibido { get; set; }

    public decimal? Cambio { get; set; }
}
