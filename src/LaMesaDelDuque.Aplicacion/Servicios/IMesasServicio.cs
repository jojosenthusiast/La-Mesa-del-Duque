using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IMesasServicio
{
    Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default);
    Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default);
    Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default);
    Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default);
    Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default);
    Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default);
    Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default);
    Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default);
}
