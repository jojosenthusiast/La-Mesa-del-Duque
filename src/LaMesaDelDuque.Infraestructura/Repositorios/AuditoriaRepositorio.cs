using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class AuditoriaRepositorio : IAuditoriaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public AuditoriaRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AgregarAsync(Auditoria auditoria, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Auditoria>().AddAsync(auditoria, cancelacion);
    }
}
