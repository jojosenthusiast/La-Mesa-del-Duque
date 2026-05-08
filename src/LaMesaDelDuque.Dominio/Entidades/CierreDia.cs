using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class CierreDia
{
    public Guid Id { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal TotalVentas { get; private set; }
    public decimal TotalVentasEfectivo { get; private set; }
    public decimal TotalVentasTarjeta { get; private set; }
    public int TotalPedidos { get; private set; }
    public int TotalPedidosCancelados { get; private set; }
    public decimal TotalMermaValorizada { get; private set; }
    public string? ResumenJson { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CierreDia()
    {
        Usuario = null!;
    }

    public CierreDia(
        DateOnly fecha,
        decimal totalVentas,
        decimal totalVentasEfectivo,
        decimal totalVentasTarjeta,
        int totalPedidos,
        int totalPedidosCancelados,
        decimal totalMermaValorizada,
        Usuario usuario,
        string? resumenJson = null)
    {
        if (usuario is null)
            throw new ReglaDominioException("El usuario que realiza el cierre es obligatorio.");

        if (totalVentas < 0)
            throw new ReglaDominioException("El total de ventas no puede ser negativo.");

        if (totalVentasEfectivo < 0)
            throw new ReglaDominioException("El total de ventas en efectivo no puede ser negativo.");

        if (totalVentasTarjeta < 0)
            throw new ReglaDominioException("El total de ventas en tarjeta no puede ser negativo.");

        if (totalPedidos < 0)
            throw new ReglaDominioException("El total de pedidos no puede ser negativo.");

        if (totalPedidosCancelados < 0)
            throw new ReglaDominioException("El total de pedidos cancelados no puede ser negativo.");

        if (totalMermaValorizada < 0)
            throw new ReglaDominioException("El total de merma valorizada no puede ser negativo.");

        Id = Guid.NewGuid();
        Fecha = fecha;
        TotalVentas = totalVentas;
        TotalVentasEfectivo = totalVentasEfectivo;
        TotalVentasTarjeta = totalVentasTarjeta;
        TotalPedidos = totalPedidos;
        TotalPedidosCancelados = totalPedidosCancelados;
        TotalMermaValorizada = totalMermaValorizada;
        Usuario = usuario;
        UsuarioId = usuario.Id;
        ResumenJson = resumenJson;
        CreatedAt = DateTime.UtcNow;
    }
}
