using LaMesaDelDuque.Dominio.Repositorios;
using Microsoft.Extensions.Logging;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IAlergenoServicio
{
    Task<List<AlergenoDto>> ObtenerActivosAsync(CancellationToken cancelacion = default);
    Task<List<AlergenoDto>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancelacion = default);
}

public class AlergenoServicio : IAlergenoServicio
{
    private readonly IAlergenoRepositorio _repo;
    private readonly ILogger<AlergenoServicio> _logger;

    public AlergenoServicio(IAlergenoRepositorio repo, ILogger<AlergenoServicio> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<AlergenoDto>> ObtenerActivosAsync(CancellationToken cancelacion = default)
    {
        var alergenos = await _repo.ObtenerActivosAsync(cancelacion);
        return alergenos.Select(a => new AlergenoDto { Id = a.Id, Nombre = a.Nombre, Icono = a.Icono }).ToList();
    }

    public async Task<List<AlergenoDto>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancelacion = default)
    {
        var pas = await _repo.ObtenerPorProductoAsync(productoId, cancelacion);
        return pas.Select(pa => new AlergenoDto { Id = pa.AlergenoId, Nombre = pa.Alergeno.Nombre, Icono = pa.Alergeno.Icono, Justificacion = pa.Justificacion }).ToList();
    }
}

public class AlergenoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? Justificacion { get; set; }
}
