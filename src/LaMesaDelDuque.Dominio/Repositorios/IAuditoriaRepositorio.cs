using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IAuditoriaRepositorio
{
    Task AgregarAsync(Auditoria auditoria, CancellationToken cancelacion = default);
}
