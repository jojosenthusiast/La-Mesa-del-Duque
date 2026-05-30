using System.Text.Json;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using Microsoft.Extensions.Logging;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IShiftHandoffServicio
{
    Task<List<MesaAsignadaDto>> ObtenerMesasActivasAsync(Guid usuarioId, CancellationToken cancelacion = default);
    Task TransferirMesaAsync(Guid mesaId, Guid nuevoMeseroId, Guid usuarioResponsableId, CancellationToken cancelacion = default);
}

public class ShiftHandoffServicio : IShiftHandoffServicio
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IUnidadDeTrabajo _uot;
    private readonly ILogger<ShiftHandoffServicio> _logger;

    public ShiftHandoffServicio(IUnidadDeTrabajo uot, ILogger<ShiftHandoffServicio> logger)
    {
        _uot = uot;
        _logger = logger;
    }

    public async Task<List<MesaAsignadaDto>> ObtenerMesasActivasAsync(Guid usuarioId, CancellationToken cancelacion = default)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuario es obligatorio para consultar mesas asignadas.", nameof(usuarioId));

        var pedidos = await _uot.Pedidos.ObtenerActivosPorMeseroAsync(usuarioId, cancelacion);

        return pedidos
            .Where(p => p.Mesa is not null)
            .Select(p => new MesaAsignadaDto
            {
                MesaId = p.Mesa!.Id,
                PedidoId = p.Id,
                MeseroAsignadoId = p.MeseroAsignadoId,
                MesaNumero = p.Mesa.Numero,
                Capacidad = p.Mesa.Capacidad,
                EstadoPedido = p.Estado.ToString(),
                Total = p.Total,
                MinutosOcupada = p.Mesa.OcupadaDesde.HasValue
                    ? Math.Max(0, (int)(DateTime.UtcNow - p.Mesa.OcupadaDesde.Value).TotalMinutes)
                    : 0
            })
            .ToList();
    }

    public async Task TransferirMesaAsync(Guid mesaId, Guid nuevoMeseroId, Guid usuarioResponsableId, CancellationToken cancelacion = default)
    {
        if (mesaId == Guid.Empty)
            throw new ArgumentException("La mesa es obligatoria para transferir.", nameof(mesaId));

        if (nuevoMeseroId == Guid.Empty)
            throw new ArgumentException("El nuevo mesero es obligatorio para transferir la mesa.", nameof(nuevoMeseroId));

        if (usuarioResponsableId == Guid.Empty)
            throw new ArgumentException("El usuario responsable es obligatorio para auditar la transferencia.", nameof(usuarioResponsableId));

        var nuevoMesero = await _uot.Usuarios.ObtenerPorIdAsync(nuevoMeseroId, cancelacion)
            ?? throw new ArgumentException($"No se encontro el mesero con ID {nuevoMeseroId}.", nameof(nuevoMeseroId));

        if (!nuevoMesero.Activo || !string.Equals(nuevoMesero.Rol.Nombre, "Mesero", StringComparison.OrdinalIgnoreCase))
            throw new ReglaDominioException("Solo se puede transferir una mesa a un usuario activo con rol Mesero.");

        var usuarioResponsable = await _uot.Usuarios.ObtenerPorIdAsync(usuarioResponsableId, cancelacion)
            ?? throw new ArgumentException($"No se encontro el usuario responsable con ID {usuarioResponsableId}.", nameof(usuarioResponsableId));

        var pedido = await _uot.Pedidos.ObtenerActivoPorMesaParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ReglaDominioException("La mesa no tiene un pedido activo para transferir.");

        var meseroAnteriorId = pedido.MeseroAsignadoId;
        if (meseroAnteriorId == nuevoMeseroId)
            throw new ReglaDominioException("La mesa ya esta asignada a ese mesero.");

        pedido.AsignarMesero(nuevoMeseroId);

        var datosAnteriores = JsonSerializer.Serialize(new
        {
            pedidoId = pedido.Id,
            mesaId,
            meseroAsignadoId = meseroAnteriorId
        }, JsonOptions);

        var datosNuevos = JsonSerializer.Serialize(new
        {
            pedidoId = pedido.Id,
            mesaId,
            meseroAsignadoId = nuevoMeseroId
        }, JsonOptions);

        var auditoria = new Auditoria(
            tablaAfectada: "pedido",
            registroId: pedido.Id,
            accion: "UPDATE",
            usuario: usuarioResponsable,
            datosAnteriores: datosAnteriores,
            datosNuevos: datosNuevos);

        await _uot.Auditorias.AgregarAsync(auditoria, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        _logger.LogInformation(
            "Mesa {MesaId} transferida del mesero {MeseroAnteriorId} al mesero {MeseroNuevoId} por {UsuarioResponsableId}",
            mesaId,
            meseroAnteriorId,
            nuevoMeseroId,
            usuarioResponsableId);
    }
}

public class MesaAsignadaDto
{
    public Guid MesaId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? MeseroAsignadoId { get; set; }
    public int MesaNumero { get; set; }
    public int Capacidad { get; set; }
    public string EstadoPedido { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int MinutosOcupada { get; set; }
}
