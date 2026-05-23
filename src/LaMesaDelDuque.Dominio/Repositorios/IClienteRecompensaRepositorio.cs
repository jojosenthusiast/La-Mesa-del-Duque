using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IClienteRepositorio
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Cliente>> BuscarAsync(string consulta, CancellationToken cancelacion = default);
    Task AgregarAsync(Cliente cliente, CancellationToken cancelacion = default);
}

public interface IRecompensaRepositorio
{
    Task<Recompensa?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Recompensa>> ObtenerActivasAsync(CancellationToken cancelacion = default);
}
