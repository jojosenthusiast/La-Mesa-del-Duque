using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface ICuentaRepositorio
{
    Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Cuenta?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Cuenta>> ObtenerPorPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task AgregarAsync(Cuenta cuenta, CancellationToken cancelacion = default);
    void Eliminar(Cuenta cuenta);
}
