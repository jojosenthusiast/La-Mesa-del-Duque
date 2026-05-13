using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IOrdenCocinaRepositorio
{
    Task<OrdenCocina?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<OrdenCocina?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<OrdenCocina>> ListarPendientesAsync(EstacionCocina? estacion = null, CancellationToken cancelacion = default);
    Task AgregarAsync(OrdenCocina orden, CancellationToken cancelacion = default);
    void Eliminar(OrdenCocina orden);
}
