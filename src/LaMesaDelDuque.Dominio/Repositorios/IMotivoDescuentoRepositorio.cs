using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IMotivoDescuentoRepositorio
{
    Task<List<MotivoDescuento>> ObtenerTodosActivosAsync(CancellationToken cancelacion = default);
    Task<List<MotivoDescuento>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task<MotivoDescuento?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task AgregarAsync(MotivoDescuento motivo, CancellationToken cancelacion = default);
    Task ActualizarAsync(MotivoDescuento motivo, CancellationToken cancelacion = default);
}
