using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using Microsoft.Extensions.Logging;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ILoyaltyServicio
{
    Task<ClienteDto> CrearClienteAsync(string nombre, string telefono, string? notas = null, CancellationToken cancelacion = default);
    Task<ClienteDto?> ObtenerClienteAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<ClienteDto>> BuscarAsync(string consulta, CancellationToken cancelacion = default);
    Task AcumularPuntosAsync(Guid clienteId, decimal totalPedido, CancellationToken cancelacion = default);
    Task<CanjeResultadoDto> CanjearPuntosAsync(Guid clienteId, Guid recompensaId, CancellationToken cancelacion = default);
    Task<List<RecompensaDto>> ObtenerRecompensasAsync(CancellationToken cancelacion = default);
}

public class LoyaltyServicio : ILoyaltyServicio
{
    private readonly IClienteRepositorio _clientes;
    private readonly IRecompensaRepositorio _recompensas;
    private readonly ILogger<LoyaltyServicio> _logger;
    private const int PuntosPorDolar = 10;

    public LoyaltyServicio(IClienteRepositorio clientes, IRecompensaRepositorio recompensas, ILogger<LoyaltyServicio> logger)
    {
        _clientes = clientes;
        _recompensas = recompensas;
        _logger = logger;
    }

    public async Task<ClienteDto> CrearClienteAsync(string nombre, string telefono, string? notas = null, CancellationToken cancelacion = default)
    {
        var cliente = new Cliente(nombre, telefono, notas);
        await _clientes.AgregarAsync(cliente, cancelacion);
        return MapCliente(cliente);
    }

    public async Task<ClienteDto?> ObtenerClienteAsync(Guid id, CancellationToken cancelacion = default)
    {
        var c = await _clientes.ObtenerPorIdAsync(id, cancelacion);
        return c is null ? null : MapCliente(c);
    }

    public async Task<List<ClienteDto>> BuscarAsync(string consulta, CancellationToken cancelacion = default)
    {
        var clientes = await _clientes.BuscarAsync(consulta, cancelacion);
        return clientes.Select(MapCliente).ToList();
    }

    public async Task AcumularPuntosAsync(Guid clienteId, decimal totalPedido, CancellationToken cancelacion = default)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(clienteId, cancelacion)
            ?? throw new ReglaDominioException("Cliente no encontrado.");
        var puntos = (int)(totalPedido * PuntosPorDolar);
        cliente.AcumularPuntos(puntos);
        cliente.RegistrarVisita();
        _logger.LogInformation("Cliente {Id} acumulo {Puntos} pts (total: {Total})", clienteId, puntos, cliente.PuntosAcumulados);
    }

    public async Task<CanjeResultadoDto> CanjearPuntosAsync(Guid clienteId, Guid recompensaId, CancellationToken cancelacion = default)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(clienteId, cancelacion)
            ?? throw new ReglaDominioException("Cliente no encontrado.");
        var recompensa = await _recompensas.ObtenerPorIdAsync(recompensaId, cancelacion)
            ?? throw new ReglaDominioException("Recompensa no encontrada.");
        cliente.RestarPuntos(recompensa.PuntosRequeridos);
        return new CanjeResultadoDto { Recompensa = recompensa.Nombre, PuntosRestantes = cliente.PuntosAcumulados };
    }

    public async Task<List<RecompensaDto>> ObtenerRecompensasAsync(CancellationToken cancelacion = default)
    {
        var recs = await _recompensas.ObtenerActivasAsync(cancelacion);
        return recs.Select(r => new RecompensaDto { Id = r.Id, Nombre = r.Nombre, Descripcion = r.Descripcion, PuntosRequeridos = r.PuntosRequeridos }).ToList();
    }

    private static ClienteDto MapCliente(Cliente c) => new()
    {
        Id = c.Id, Nombre = c.Nombre, Telefono = c.Telefono, Notas = c.Notas,
        Puntos = c.PuntosAcumulados, Visitas = c.VisitasTotales, UltimaVisita = c.UltimaVisita
    };
}

public class ClienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Notas { get; set; }
    public int Puntos { get; set; }
    public int Visitas { get; set; }
    public DateTime? UltimaVisita { get; set; }
}

public class RecompensaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int PuntosRequeridos { get; set; }
}

public class CanjeResultadoDto
{
    public string Recompensa { get; set; } = string.Empty;
    public int PuntosRestantes { get; set; }
}
