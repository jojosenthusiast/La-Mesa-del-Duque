using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IDescuentoServicio
{
    Task<DescuentoAplicadoDto> SolicitarDescuentoAsync(
        Guid pedidoId,
        Guid motivoId,
        string tipoDescuento,
        decimal valor,
        decimal montoAplicado,
        Guid usuarioSolicitaId,
        Guid? detallePedidoId = null,
        CancellationToken ct = default);

    Task<DescuentoAplicadoDto> AprobarDescuentoAsync(
        Guid descuentoId,
        Guid usuarioAutorizaId,
        string? nota = null,
        CancellationToken ct = default);

    Task<DescuentoAplicadoDto> RechazarDescuentoAsync(
        Guid descuentoId,
        Guid usuarioAutorizaId,
        string? nota = null,
        CancellationToken ct = default);

    Task<List<DescuentoAplicadoDto>> ObtenerPendientesAsync(CancellationToken ct = default);
    Task<List<DescuentoAplicadoDto>> ObtenerPorPedidoAsync(Guid pedidoId, CancellationToken ct = default);
    Task<List<MotivoDescuentoDto>> ListarMotivosAsync(CancellationToken ct = default);
    Task<List<MotivoDescuentoDto>> ListarTodosMotivosAsync(CancellationToken ct = default);
    Task<MotivoDescuentoDto> CrearMotivoAsync(string nombre, string? descripcion = null, CancellationToken ct = default);
    Task<MotivoDescuentoDto> ToggleMotivoAsync(Guid motivoId, CancellationToken ct = default);
}

public class DescuentoServicio : IDescuentoServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public DescuentoServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<DescuentoAplicadoDto> SolicitarDescuentoAsync(
        Guid pedidoId,
        Guid motivoId,
        string tipoDescuento,
        decimal valor,
        decimal montoAplicado,
        Guid usuarioSolicitaId,
        Guid? detallePedidoId = null,
        CancellationToken ct = default)
    {
        var motivo = await _uot.MotivosDescuento.ObtenerPorIdAsync(motivoId, ct)
            ?? throw new ReglaDominioException("El motivo de descuento no fue encontrado.");

        if (!motivo.Activo)
            throw new ReglaDominioException("El motivo de descuento no está activo.");

        var descuento = new DescuentoAplicado(pedidoId, motivoId, tipoDescuento, valor, montoAplicado, usuarioSolicitaId, detallePedidoId);
        await _uot.Descuentos.AgregarAsync(descuento, ct);
        await _uot.GuardarCambiosAsync(ct);

        var guardado = await _uot.Descuentos.ObtenerPorIdAsync(descuento.Id, ct)
            ?? throw new ReglaDominioException("Error al recuperar el descuento creado.");

        return Map(guardado);
    }

    public async Task<DescuentoAplicadoDto> AprobarDescuentoAsync(
        Guid descuentoId,
        Guid usuarioAutorizaId,
        string? nota = null,
        CancellationToken ct = default)
    {
        var descuento = await _uot.Descuentos.ObtenerPorIdAsync(descuentoId, ct)
            ?? throw new ReglaDominioException("Descuento no encontrado.");

        descuento.Aprobar(usuarioAutorizaId, nota);
        await _uot.GuardarCambiosAsync(ct);
        return Map(descuento);
    }

    public async Task<DescuentoAplicadoDto> RechazarDescuentoAsync(
        Guid descuentoId,
        Guid usuarioAutorizaId,
        string? nota = null,
        CancellationToken ct = default)
    {
        var descuento = await _uot.Descuentos.ObtenerPorIdAsync(descuentoId, ct)
            ?? throw new ReglaDominioException("Descuento no encontrado.");

        descuento.Rechazar(usuarioAutorizaId, nota);
        await _uot.GuardarCambiosAsync(ct);
        return Map(descuento);
    }

    public async Task<List<DescuentoAplicadoDto>> ObtenerPendientesAsync(CancellationToken ct = default)
    {
        var pendientes = await _uot.Descuentos.ObtenerPendientesAsync(ct);
        return pendientes.Select(Map).ToList();
    }

    public async Task<List<DescuentoAplicadoDto>> ObtenerPorPedidoAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var descuentos = await _uot.Descuentos.ObtenerPorPedidoAsync(pedidoId, ct);
        return descuentos.Select(Map).ToList();
    }

    public async Task<List<MotivoDescuentoDto>> ListarMotivosAsync(CancellationToken ct = default)
    {
        var motivos = await _uot.MotivosDescuento.ObtenerTodosActivosAsync(ct);
        return motivos.Select(MapMotivo).ToList();
    }

    public async Task<List<MotivoDescuentoDto>> ListarTodosMotivosAsync(CancellationToken ct = default)
    {
        var motivos = await _uot.MotivosDescuento.ObtenerTodosAsync(ct);
        return motivos.Select(MapMotivo).ToList();
    }

    public async Task<MotivoDescuentoDto> CrearMotivoAsync(string nombre, string? descripcion = null, CancellationToken ct = default)
    {
        var motivo = new MotivoDescuento(nombre, descripcion);
        await _uot.MotivosDescuento.AgregarAsync(motivo, ct);
        await _uot.GuardarCambiosAsync(ct);
        return MapMotivo(motivo);
    }

    public async Task<MotivoDescuentoDto> ToggleMotivoAsync(Guid motivoId, CancellationToken ct = default)
    {
        var motivo = await _uot.MotivosDescuento.ObtenerPorIdAsync(motivoId, ct)
            ?? throw new ReglaDominioException("Motivo de descuento no encontrado.");

        if (motivo.Activo)
            motivo.Desactivar();
        else
            motivo.Activar();

        await _uot.MotivosDescuento.ActualizarAsync(motivo, ct);
        await _uot.GuardarCambiosAsync(ct);
        return MapMotivo(motivo);
    }

    private static DescuentoAplicadoDto Map(DescuentoAplicado d) => new()
    {
        Id = d.Id,
        PedidoId = d.PedidoId,
        DetallePedidoId = d.DetallePedidoId,
        Motivo = MapMotivo(d.Motivo),
        TipoDescuento = d.TipoDescuento,
        Valor = d.Valor,
        MontoAplicado = d.MontoAplicado,
        Estado = d.Estado.ToString(),
        UsuarioSolicitaId = d.UsuarioSolicitaId,
        UsuarioAutorizaId = d.UsuarioAutorizaId,
        FechaSolicitud = d.FechaSolicitud,
        FechaResolucion = d.FechaResolucion,
        NotaAutorizador = d.NotaAutorizador,
    };

    private static MotivoDescuentoDto MapMotivo(MotivoDescuento m) => new()
    {
        Id = m.Id,
        Nombre = m.Nombre,
        Descripcion = m.Descripcion,
        Activo = m.Activo,
    };
}
