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
    public decimal EfectivoReal { get; private set; }
    public decimal TarjetaReal { get; private set; }
    public decimal DiferenciaEfectivo { get; private set; }
    public decimal DiferenciaTarjeta { get; private set; }
    public bool EsCerrado { get; private set; }
    public DateTime? CerradoEn { get; private set; }
    public string? Observacion { get; private set; }
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
        Usuario? usuario = null,
        string? resumenJson = null)
    {
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
        Usuario = usuario!;
        UsuarioId = usuario?.Id ?? Guid.Empty;
        ResumenJson = resumenJson;
        EsCerrado = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Cerrar(
        decimal totalVentas,
        decimal totalVentasEfectivo,
        decimal totalVentasTarjeta,
        int totalPedidos,
        int totalPedidosCancelados,
        decimal totalMermaValorizada,
        decimal efectivoReal,
        decimal tarjetaReal,
        string? observacion = null)
    {
        if (EsCerrado)
            throw new ReglaDominioException("El cierre de día ya fue cerrado.");

        if (totalVentas < 0 || totalVentasEfectivo < 0 || totalVentasTarjeta < 0
            || totalPedidos < 0 || totalPedidosCancelados < 0 || totalMermaValorizada < 0
            || efectivoReal < 0 || tarjetaReal < 0)
            throw new ReglaDominioException("Los totales no pueden ser negativos.");

        TotalVentas = totalVentas;
        TotalVentasEfectivo = totalVentasEfectivo;
        TotalVentasTarjeta = totalVentasTarjeta;
        TotalPedidos = totalPedidos;
        TotalPedidosCancelados = totalPedidosCancelados;
        TotalMermaValorizada = totalMermaValorizada;
        EfectivoReal = efectivoReal;
        TarjetaReal = tarjetaReal;
        DiferenciaEfectivo = efectivoReal - totalVentasEfectivo;
        DiferenciaTarjeta = tarjetaReal - totalVentasTarjeta;

        var hayDescuadre = DiferenciaEfectivo != 0m || DiferenciaTarjeta != 0m;
        if (hayDescuadre && string.IsNullOrWhiteSpace(observacion))
            throw new ReglaDominioException("La observacion es obligatoria cuando existe descuadre de caja.");

        Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
        EsCerrado = true;
        CerradoEn = DateTime.UtcNow;
    }

    public void Reabrir()
    {
        if (!EsCerrado)
            throw new ReglaDominioException("El cierre de día ya está abierto.");

        EsCerrado = false;
        CerradoEn = null;
        EfectivoReal = 0m;
        TarjetaReal = 0m;
        DiferenciaEfectivo = 0m;
        DiferenciaTarjeta = 0m;
        Observacion = null;
        TotalVentas = 0m;
        TotalVentasEfectivo = 0m;
        TotalVentasTarjeta = 0m;
        TotalPedidos = 0;
        TotalPedidosCancelados = 0;
        TotalMermaValorizada = 0m;
    }
}
