using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

internal class ZonasSalonServicio : IZonasSalonServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public ZonasSalonServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<List<ZonaSalonDto>> ListarActivasAsync(CancellationToken cancelacion = default)
    {
        var zonas = await _uot.ZonasSalon.ObtenerActivasOrdenadasAsync(cancelacion);
        return zonas.Select(MapToDto).ToList();
    }

    public async Task<List<ZonaSalonDto>> ListarTodasAsync(CancellationToken cancelacion = default)
    {
        var zonas = await _uot.ZonasSalon.ObtenerTodasAsync(cancelacion);
        return zonas.Select(MapToDto).ToList();
    }

    public async Task<ZonaSalonDto> CrearAsync(string nombre, int orden, CancellationToken cancelacion = default)
    {
        var zona = new ZonaSalon(nombre, orden);
        await _uot.ZonasSalon.AgregarAsync(zona, cancelacion);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(zona);
    }

    public async Task<ZonaSalonDto> ActualizarAsync(Guid id, string nombre, int orden, CancellationToken cancelacion = default)
    {
        var zona = await _uot.ZonasSalon.ObtenerParaActualizarAsync(id, cancelacion)
            ?? throw new ArgumentException($"No se encontró la zona con ID {id}.", nameof(id));

        zona.ActualizarNombre(nombre);
        zona.ActualizarOrden(orden);
        await _uot.GuardarCambiosAsync(cancelacion);

        return MapToDto(zona);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken cancelacion = default)
    {
        var zona = await _uot.ZonasSalon.ObtenerParaActualizarAsync(id, cancelacion)
            ?? throw new ArgumentException($"No se encontró la zona con ID {id}.", nameof(id));

        zona.Desactivar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    public async Task ActivarAsync(Guid id, CancellationToken cancelacion = default)
    {
        var zona = await _uot.ZonasSalon.ObtenerParaActualizarAsync(id, cancelacion)
            ?? throw new ArgumentException($"No se encontró la zona con ID {id}.", nameof(id));

        zona.Activar();
        await _uot.GuardarCambiosAsync(cancelacion);
    }

    private static ZonaSalonDto MapToDto(ZonaSalon zona)
    {
        return new ZonaSalonDto
        {
            Id = zona.Id,
            Nombre = zona.Nombre,
            Orden = zona.Orden,
            Activa = zona.Activa
        };
    }
}
