using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IZonasSalonServicio
{
    Task<List<ZonaSalonDto>> ListarActivasAsync(CancellationToken cancelacion = default);
    Task<List<ZonaSalonDto>> ListarTodasAsync(CancellationToken cancelacion = default);
    Task<ZonaSalonDto> CrearAsync(string nombre, int orden, CancellationToken cancelacion = default);
    Task<ZonaSalonDto> ActualizarAsync(Guid id, string nombre, int orden, CancellationToken cancelacion = default);
    Task DesactivarAsync(Guid id, CancellationToken cancelacion = default);
    Task ActivarAsync(Guid id, CancellationToken cancelacion = default);
}
