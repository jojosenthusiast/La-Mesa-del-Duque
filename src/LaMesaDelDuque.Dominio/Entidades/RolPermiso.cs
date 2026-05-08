using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class RolPermiso
{
    public Guid RolId { get; private set; }
    public Rol Rol { get; private set; }
    public Guid PermisoId { get; private set; }
    public Permiso Permiso { get; private set; }

    private RolPermiso()
    {
        Rol = null!;
        Permiso = null!;
    }

    public RolPermiso(Rol rol, Permiso permiso)
    {
        if (rol is null)
            throw new ReglaDominioException("El rol es obligatorio.");
        if (permiso is null)
            throw new ReglaDominioException("El permiso es obligatorio.");

        Rol = rol;
        Permiso = permiso;
        RolId = rol.Id;
        PermisoId = permiso.Id;
    }
}
