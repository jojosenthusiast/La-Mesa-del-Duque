using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IMermaRepositorio
{
    Task AgregarAsync(MermaDiaria merma, CancellationToken ct = default);
    Task<List<MermaDiaria>> ObtenerDelDiaAsync(DateOnly fecha, CancellationToken ct = default);
}

public interface ICierreDiaRepositorio
{
    Task<CierreDia?> ObtenerAbiertoAsync(DateOnly fecha, CancellationToken ct = default);
    Task AgregarAsync(CierreDia cierre, CancellationToken ct = default);
    Task<List<CierreDia>> ObtenerTodosAsync(CancellationToken ct = default);
}
