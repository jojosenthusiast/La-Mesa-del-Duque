using System.Security.Claims;
using System.Text.Json;
using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

public class AuditoriaInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> EntidadesAuditadas =
    [
        nameof(Pedido),
        nameof(Pago),
        nameof(Producto),
        nameof(Ingrediente),
        nameof(Usuario),
        nameof(RestauranteConfig)
    ];

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditoriaInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, ct);

        var cambios = eventData.Context.ChangeTracker.Entries()
            .Where(e =>
                EntidadesAuditadas.Contains(e.Entity.GetType().Name) &&
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (cambios.Count == 0)
            return await base.SavingChangesAsync(eventData, result, ct);

        var usuario = ObtenerUsuarioDesdeTracker(eventData.Context);
        if (usuario is null)
            return await base.SavingChangesAsync(eventData, result, ct);

        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        foreach (var entrada in cambios)
        {
            var accionEf = entrada.State;
            var accionStr = accionEf == EntityState.Added ? "INSERT"
                : accionEf == EntityState.Modified ? "UPDATE"
                : "DELETE";

            var registroId = ObtenerIdEntidad(entrada);
            if (registroId == Guid.Empty) continue;

            var anterior = accionEf == EntityState.Added ? null : SerializarOriginal(entrada);
            var nuevo = accionEf == EntityState.Deleted ? null : SerializarActual(entrada);

            var auditoria = new Auditoria(
                tablaAfectada: entrada.Entity.GetType().Name,
                registroId: registroId,
                accion: accionStr,
                usuario: usuario,
                datosAnteriores: anterior,
                datosNuevos: nuevo,
                ipAddress: ip);

            eventData.Context.Set<Auditoria>().Add(auditoria);
        }

        return await base.SavingChangesAsync(eventData, result, ct);
    }

    private Usuario? ObtenerUsuarioDesdeTracker(DbContext contexto)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var usuarioId))
            return null;

        return contexto.ChangeTracker.Entries<Usuario>()
            .Select(e => e.Entity)
            .FirstOrDefault(u => u.Id == usuarioId);
    }

    private static Guid ObtenerIdEntidad(EntityEntry entrada)
    {
        var idProp = entrada.Properties
            .FirstOrDefault(p => string.Equals(p.Metadata.Name, "Id", StringComparison.OrdinalIgnoreCase));

        if (idProp?.CurrentValue is Guid g) return g;
        if (idProp?.OriginalValue is Guid go) return go;
        return Guid.Empty;
    }

    private static string? SerializarOriginal(EntityEntry entrada)
    {
        try
        {
            var valores = entrada.OriginalValues.Properties
                .ToDictionary(p => p.Name, p => entrada.OriginalValues[p]);
            return JsonSerializer.Serialize(valores, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializarActual(EntityEntry entrada)
    {
        try
        {
            var valores = entrada.CurrentValues.Properties
                .ToDictionary(p => p.Name, p => entrada.CurrentValues[p]);
            return JsonSerializer.Serialize(valores, JsonOpts);
        }
        catch
        {
            return null;
        }
    }
}
