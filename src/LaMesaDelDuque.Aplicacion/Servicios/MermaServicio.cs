using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IMermaServicio
{
    Task<MermaDiariaDto> RegistrarMermaAsync(RegistrarMermaRequest req, Guid usuarioId, CancellationToken ct = default);
    Task<List<MermaDiariaDto>> ObtenerMermasDelDiaAsync(CancellationToken ct = default);
}

public class MermaServicio : IMermaServicio
{
    private readonly IUnidadDeTrabajo _uot;
    public MermaServicio(IUnidadDeTrabajo uot) => _uot = uot;

    public async Task<MermaDiariaDto> RegistrarMermaAsync(RegistrarMermaRequest req, Guid usuarioId, CancellationToken ct = default)
    {
        var ingrediente = await _uot.Ingredientes.ObtenerPorIdAsync(req.IngredienteId, ct)
            ?? throw new ArgumentException("Ingrediente no encontrado.");

        var cierre = await _uot.CierresDia.ObtenerAbiertoAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct)
            ?? throw new InvalidOperationException("No hay cierre de día abierto. Abrí el día antes de registrar mermas.");

        var usuario = await _uot.Usuarios.ObtenerPorIdAsync(usuarioId, ct)
            ?? throw new ArgumentException("Usuario no encontrado para registrar la merma.");

        var costoEstimado = ingrediente.CostoUnitario * req.Cantidad;
        var merma = new MermaDiaria(cierre, ingrediente, req.Cantidad, usuario, req.Tipo, costoEstimado, req.Notas, req.Lote);

        ingrediente.DescontarStock(req.Cantidad);

        await _uot.Mermas.AgregarAsync(merma, ct);
        await _uot.GuardarCambiosAsync(ct);
        return Map(merma);
    }

    public async Task<List<MermaDiariaDto>> ObtenerMermasDelDiaAsync(CancellationToken ct = default)
    {
        var mermas = await _uot.Mermas.ObtenerDelDiaAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);
        return mermas.Select(Map).ToList();
    }

    private static MermaDiariaDto Map(MermaDiaria m) => new()
    {
        Id = m.Id,
        IngredienteNombre = m.Ingrediente?.Nombre ?? "",
        Cantidad = m.CantidadDescartada,
        Costo = m.CostoEstimado,
        Tipo = m.Tipo.ToString(),
        Lote = m.Lote,
        Notas = m.Notas,
        Fecha = m.CreatedAt
    };
}

public class RegistrarMermaRequest
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public TipoMerma Tipo { get; set; } = TipoMerma.Otro;
    public string? Notas { get; set; }
    public string? Lote { get; set; }
}

public class MermaDiariaDto
{
    public Guid Id { get; set; }
    public string IngredienteNombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Costo { get; set; }
    public string Tipo { get; set; } = "";
    public string? Lote { get; set; }
    public string? Notas { get; set; }
    public DateTime Fecha { get; set; }
}
