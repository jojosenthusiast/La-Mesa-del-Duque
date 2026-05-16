namespace LaMesaDelDuque.Web.Models.Operaciones;

public class CrearCuentasConItemsRequest
{
    public Guid PedidoId { get; set; }
    public List<CuentaAsignacionVm> Asignaciones { get; set; } = [];
}

public class CuentaAsignacionVm
{
    public int CuentaNumero { get; set; }
    public List<ItemAsignacionVm> Items { get; set; } = [];
}

public class ItemAsignacionVm
{
    public Guid DetalleId { get; set; }
    public int Cantidad { get; set; }
}
