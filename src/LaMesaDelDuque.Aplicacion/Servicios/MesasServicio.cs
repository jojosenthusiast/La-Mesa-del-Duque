using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class MesasServicio : IMesasServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly INotificadorSalon? _notificador;

    public MesasServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public MesasServicio(IUnidadDeTrabajo uot, INotificadorSalon notificador)
    {
        _uot = uot;
        _notificador = notificador;
    }

    public async Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default)
    {
        var mesas = await _uot.Mesas.ObtenerTodasAsync(cancelacion);
        return mesas.Select(MapToDto).ToList();
    }

    public async Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default)
    {
        var mesa = await _uot.Mesas.ObtenerPorNumeroAsync(numero, cancelacion);
        return mesa is null ? null : MapToDto(mesa);
    }

    public async Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default)
    {
        var existente = await _uot.Mesas.ObtenerPorNumeroAsync(numero, cancelacion);
        if (existente is not null)
            throw new InvalidOperationException($"Ya existe una mesa con el número {numero}.");

        var mesa = new Mesa(numero, capacidad);
        await _uot.Mesas.AgregarAsync(mesa, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(mesa);
    }

    public async Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default)
    {
        if (!Enum.TryParse<EstadoMesa>(nuevoEstado, ignoreCase: true, out var estado))
            throw new ArgumentException($"Estado de mesa no válido: {nuevoEstado}.", nameof(nuevoEstado));

        var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId}.", nameof(mesaId));

        if (estado == EstadoMesa.Ocupada)
        {
            mesa.Ocupar();
        }
        else if (mesa.Estado == EstadoMesa.Ocupada && estado == EstadoMesa.Disponible)
        {
            var pedidos = await _uot.Pedidos.ObtenerPorMesaAsync(mesaId, cancelacion);
            if (pedidos.Any(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion || p.Estado == EstadoPedido.EnCobro))
                throw new ReglaDominioException("No se puede liberar la mesa porque tiene pedidos activos.");

            mesa.Liberar();
        }
        else
        {
            mesa.CambiarEstado(estado);
        }

        await _uot.GuardarCambiosAsync(cancelacion);

        if (_notificador is not null)
            await _notificador.NotificarMesaActualizadaAsync(mesaId, mesa.Estado.ToString(), cancelacion);
    }

    public async Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default)
    {
        var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId}.", nameof(mesaId));

        mesa.ActualizarDatos(numero, capacidad);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(mesa);
    }

    public async Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default)
    {
        var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId}.", nameof(mesaId));

        var pedidos = await _uot.Pedidos.ObtenerPorMesaAsync(mesaId, cancelacion);
        if (pedidos.Any(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion || p.Estado == EstadoPedido.EnCobro))
            throw new ReglaDominioException("No se puede desactivar la mesa porque tiene pedidos activos.");

        mesa.Desactivar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    public async Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default)
    {
        if (!Enum.TryParse<FormaMesa>(forma, ignoreCase: true, out var formaEnum))
            throw new ArgumentException($"Forma de mesa no válida: {forma}.", nameof(forma));

        var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId}.", nameof(mesaId));

        mesa.ActualizarPosicion(posicionX, posicionY, zonaId, formaEnum, rotacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        if (_notificador is not null)
            await _notificador.NotificarMesaMovidaAsync(mesaId, posicionX, posicionY, cancelacion);

        return MapToDto(mesa);
    }

    public async Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default)
    {
        var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId, cancelacion)
            ?? throw new ArgumentException($"No se encontró la mesa con ID {mesaId}.", nameof(mesaId));

        mesa.LimpiarPosicion();
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(mesa);
    }

    private static MesaDto MapToDto(Mesa mesa)
    {
        return new MesaDto
        {
            Id = mesa.Id,
            Numero = mesa.Numero,
            Capacidad = mesa.Capacidad,
            Estado = mesa.Estado.ToString(),
            Activa = mesa.Activa,
            PosicionX = mesa.PosicionX,
            PosicionY = mesa.PosicionY,
            ZonaId = mesa.ZonaId,
            Forma = mesa.Forma?.ToString(),
            Rotacion = mesa.Rotacion,
            OcupadaDesde = mesa.OcupadaDesde,
            GraciaHasta  = mesa.GraciaHasta
        };
    }
}
