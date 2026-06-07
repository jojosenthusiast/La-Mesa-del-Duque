using System.Text.Json;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class CocinaServicio : ICocinaServicio
{
    private readonly IUnidadDeTrabajo _uot;
    private readonly INotificadorPedidos _notificador;

    public CocinaServicio(IUnidadDeTrabajo uot, INotificadorPedidos notificador)
    {
        _uot = uot;
        _notificador = notificador;
    }

    public async Task GenerarOrdenesAsync(Guid pedidoId, IEnumerable<Guid>? soloDetalles = null, CancellationToken ct = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesAsync(pedidoId, ct)
            ?? throw new ArgumentException($"No se encontró el pedido con ID {pedidoId}.", nameof(pedidoId));

        var filtro = soloDetalles?.ToHashSet();
        var detallesAFiltrar = filtro is not null
            ? pedido.Detalles.Where(d => filtro.Contains(d.Id)).ToList()
            : pedido.Detalles.ToList();

        var detallesYaEnCocina = (await _uot.OrdenesCocina.ListarPorPedidoAsync(pedidoId, ct))
            .Where(o => o.DetallePedidoId.HasValue)
            .Select(o => o.DetallePedidoId!.Value)
            .ToHashSet();

        var ordenesCreadas = new List<OrdenCocina>();

        foreach (var detalle in detallesAFiltrar)
        {
            if (detallesYaEnCocina.Contains(detalle.Id))
                continue;

            var estacion = detalle.Producto.Categoria?.EstacionCocina ?? EstacionCocina.Expo;

            var (alergenos, quitados, extras, curso) = ParsearModificaciones(detalle.ModificacionesJson);

            var notasCombinadas = CombinarNotas(detalle.Notas, alergenos, quitados, extras);

            CursoCocina? cursoCocina = null;
            if (!string.IsNullOrWhiteSpace(curso) && Enum.TryParse<CursoCocina>(curso, out var cursoEnum))
                cursoCocina = cursoEnum;

            var orden = new OrdenCocina(
                pedidoId,
                detalle.Id,
                detalle.Producto.Nombre,
                detalle.Cantidad,
                estacion,
                pedido.Mesa?.Numero,
                pedido.TipoServicio.ToString(),
                notasCombinadas,
                alergenos,
                quitados,
                extras,
                null,
                detalle.Producto.Id,
                cursoCocina,
                detalle.Producto.TiempoPreparacionMin);

            await _uot.OrdenesCocina.AgregarAsync(orden, ct);
            detallesYaEnCocina.Add(detalle.Id);
            ordenesCreadas.Add(orden);
        }

        await _uot.GuardarCambiosAsync(ct);

        // Notificar a cada estación
        foreach (var orden in ordenesCreadas)
        {
            await _notificador.NotificarOrdenCocinaAsync(orden.Estacion.ToString(), MapToDto(orden), ct);
        }
    }

    public async Task<List<OrdenCocinaDto>> ListarPendientesAsync(EstacionCocina? estacion = null, CancellationToken ct = default)
    {
        var ordenes = await _uot.OrdenesCocina.ListarPendientesAsync(estacion, ct);
        return ordenes.Select(MapToDto).ToList();
    }

    public async Task<OrdenCocinaDto> MarcarListoAsync(Guid ordenId, CancellationToken ct = default)
    {
        var orden = await _uot.OrdenesCocina.ObtenerParaActualizarAsync(ordenId, ct)
            ?? throw new ArgumentException($"No se encontró la orden de cocina con ID {ordenId}.", nameof(ordenId));

        orden.MarcarComoListo();
        await _uot.GuardarCambiosAsync(ct);
        await _notificador.NotificarItemListoAsync(orden.Estacion.ToString(), ordenId, ct);

        // Si todas las órdenes del pedido están listas, marcar el pedido como Listo
        var ordenesPedido = await _uot.OrdenesCocina.ListarPorPedidoAsync(orden.PedidoId, ct);
        if (ordenesPedido.Count > 0 && ordenesPedido.All(o => o.Estado == EstadoLineaCocina.Listo))
        {
            var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(orden.PedidoId, ct);
            if (pedido is not null && pedido.Estado == EstadoPedido.EnPreparacion)
            {
                pedido.MarcarListo();
                await _uot.GuardarCambiosAsync(ct);
                await _notificador.NotificarEstadoCambiadoAsync(pedido.Id, pedido.Estado, ct);
            }
        }

        return MapToDto(orden);
    }

    public async Task<OrdenCocinaDto> RecuperarAsync(Guid ordenId, CancellationToken ct = default)
    {
        var orden = await _uot.OrdenesCocina.ObtenerParaActualizarAsync(ordenId, ct)
            ?? throw new ArgumentException($"No se encontró la orden de cocina con ID {ordenId}.", nameof(ordenId));

        orden.Recuperar();
        await _uot.GuardarCambiosAsync(ct);
        await _notificador.NotificarItemRecuperadoAsync(orden.Estacion.ToString(), MapToDto(orden), ct);

        return MapToDto(orden);
    }

    private static (string? alergenos, string? quitados, string? extras, string? curso) ParsearModificaciones(string? modificacionesJson)
    {
        if (string.IsNullOrWhiteSpace(modificacionesJson))
            return (null, null, null, null);

        try
        {
            var modificaciones = JsonSerializer.Deserialize<List<ModificacionIngrediente>>(modificacionesJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (modificaciones is null || modificaciones.Count == 0)
                return (null, null, null, null);

            var alergenosList = new List<string>();
            var quitadosList = new List<string>();
            var extrasList = new List<string>();
            string? curso = null;

            foreach (var m in modificaciones)
            {
                if (m.Accion == "curso" && !string.IsNullOrWhiteSpace(m.IngredienteNombre))
                {
                    curso = m.IngredienteNombre;
                    continue;
                }

                if (m.Motivo == "alergia" && !string.IsNullOrWhiteSpace(m.IngredienteNombre))
                {
                    alergenosList.Add(m.IngredienteNombre);
                }
                else if (m.Accion == "quitar")
                {
                    quitadosList.Add(m.IngredienteNombre);
                }
                else if (m.Accion == "intercambiar")
                {
                    var reemplazo = m.IngredienteReemplazoNombre ?? "otro";
                    quitadosList.Add($"{m.IngredienteNombre} → {reemplazo}");
                }
                else if (m.Accion == "extra")
                {
                    extrasList.Add(m.IngredienteNombre);
                }
            }

            return (
                alergenosList.Count > 0 ? string.Join(", ", alergenosList) : null,
                quitadosList.Count > 0 ? string.Join(", ", quitadosList) : null,
                extrasList.Count > 0 ? string.Join(", ", extrasList) : null,
                curso
            );
        }
        catch
        {
            return (null, null, null, null);
        }
    }

    private static string? CombinarNotas(string? notasBase, string? alergenos, string? quitados, string? extras)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(notasBase)) partes.Add(notasBase);
        if (!string.IsNullOrWhiteSpace(alergenos)) partes.Add($"Alergias: {alergenos}");
        if (!string.IsNullOrWhiteSpace(quitados)) partes.Add($"Sin: {quitados}");
        if (!string.IsNullOrWhiteSpace(extras)) partes.Add($"Extra: {extras}");

        return partes.Count > 0 ? string.Join(" | ", partes) : null;
    }

    private static OrdenCocinaDto MapToDto(OrdenCocina orden)
    {
        return new OrdenCocinaDto
        {
            Id = orden.Id,
            PedidoId = orden.PedidoId,
            ProductoNombre = orden.ProductoNombre,
            Cantidad = orden.Cantidad,
            Notas = orden.Notas,
            Alergenos = orden.Alergenos,
            IngredientesQuitados = orden.IngredientesQuitados,
            IngredientesExtra = orden.IngredientesExtra,
            CocineroId = orden.CocineroId,
            Estacion = orden.Estacion.ToString(),
            Estado = orden.Estado.ToString(),
            HoraRecibido = orden.HoraRecibido,
            MesaNumero = orden.MesaNumero,
            TipoServicio = orden.TipoServicio,
            Curso = orden.Curso?.ToString(),
            ProductoId = orden.ProductoId,
            TiempoPreparacionMin = orden.TiempoPreparacionMin
        };
    }
}
