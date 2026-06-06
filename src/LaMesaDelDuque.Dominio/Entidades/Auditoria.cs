using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Auditoria
{
    private const int LongitudMaximaTabla = 100;
    private const int LongitudMaximaAccion = 10;
    private const int LongitudMaximaIp = 45;
    private static readonly string[] AccionesValidas = ["INSERT", "UPDATE", "DELETE"];

    public long Id { get; private set; }
    public string TablaAfectada { get; private set; }
    public Guid RegistroId { get; private set; }
    public string Accion { get; private set; }
    public string? DatosAnteriores { get; private set; }
    public string? DatosNuevos { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime Fecha { get; private set; }

    private Auditoria()
    {
        TablaAfectada = string.Empty;
        Accion = string.Empty;
        Usuario = null!;
    }

    public Auditoria(
        string tablaAfectada,
        Guid registroId,
        string accion,
        Usuario usuario,
        string? datosAnteriores = null,
        string? datosNuevos = null,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(tablaAfectada))
            throw new ReglaDominioException("La tabla afectada es obligatoria.");

        if (tablaAfectada.Trim().Length > LongitudMaximaTabla)
            throw new ReglaDominioException($"La tabla afectada no puede exceder {LongitudMaximaTabla} caracteres.");

        if (registroId == Guid.Empty)
            throw new ReglaDominioException("El ID del registro auditado es obligatorio.");

        if (string.IsNullOrWhiteSpace(accion) || !Array.Exists(AccionesValidas, a => a == accion.Trim().ToUpperInvariant()))
            throw new ReglaDominioException("La acción de auditoría debe ser 'INSERT', 'UPDATE' o 'DELETE'.");

        if (usuario is null)
            throw new ReglaDominioException("El usuario es obligatorio para el registro de auditoría.");

        if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.Trim().Length > LongitudMaximaIp)
            throw new ReglaDominioException($"La dirección IP no puede exceder {LongitudMaximaIp} caracteres.");

        TablaAfectada = tablaAfectada.Trim();
        RegistroId = registroId;
        Accion = accion.Trim().ToUpperInvariant();
        Usuario = usuario;
        UsuarioId = usuario.Id;
        DatosAnteriores = datosAnteriores;
        DatosNuevos = datosNuevos;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        Fecha = DateTime.UtcNow;
    }
}
