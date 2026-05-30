using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Proveedor
{
    private const int LongitudMaximaNombre = 200;
    private const int LongitudMaximaNit = 32;
    private const int LongitudMaximaContacto = 150;
    private const int LongitudMaximaTelefono = 20;
    private const int LongitudMaximaEmail = 150;
    private const int LongitudMaximaDireccion = 300;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Nit { get; private set; }
    public string? Contacto { get; private set; }
    public string? Telefono { get; private set; }
    public string? Email { get; private set; }
    public string? Direccion { get; private set; }
    public bool Activo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Proveedor()
    {
        Nombre = string.Empty;
        Nit = string.Empty;
    }

    public Proveedor(string nombre, string nit, string? contacto = null, string? telefono = null, string? email = null, string? direccion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del proveedor es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del proveedor no puede exceder {LongitudMaximaNombre} caracteres.");

        if (string.IsNullOrWhiteSpace(nit))
            throw new ReglaDominioException("El NIT del proveedor es obligatorio.");

        var nitNormalizado = nit.Trim();
        if (nitNormalizado.Length > LongitudMaximaNit)
            throw new ReglaDominioException($"El NIT del proveedor no puede exceder {LongitudMaximaNit} caracteres.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(nitNormalizado, @"^\d{4}-\d{6}-\d{3}-\d$") )
            throw new ReglaDominioException("El NIT del proveedor tiene un formato inválido. Formato esperado: 0000-000000-000-0.");

        if (!string.IsNullOrWhiteSpace(contacto) && contacto.Trim().Length > LongitudMaximaContacto)
            throw new ReglaDominioException($"El contacto del proveedor no puede exceder {LongitudMaximaContacto} caracteres.");

        if (!string.IsNullOrWhiteSpace(telefono) && telefono.Trim().Length > LongitudMaximaTelefono)
            throw new ReglaDominioException($"El teléfono del proveedor no puede exceder {LongitudMaximaTelefono} caracteres.");

        if (!string.IsNullOrWhiteSpace(email) && email.Trim().Length > LongitudMaximaEmail)
            throw new ReglaDominioException($"El email del proveedor no puede exceder {LongitudMaximaEmail} caracteres.");

        if (!string.IsNullOrWhiteSpace(email) && !System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ReglaDominioException("El email del proveedor tiene un formato inválido.");

        if (!string.IsNullOrWhiteSpace(direccion) && direccion.Trim().Length > LongitudMaximaDireccion)
            throw new ReglaDominioException($"La dirección del proveedor no puede exceder {LongitudMaximaDireccion} caracteres.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Nit = nitNormalizado;
        Contacto = string.IsNullOrWhiteSpace(contacto) ? null : contacto.Trim();
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
        Activo = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Actualizar(string nombre, string nit, string? contacto, string? telefono, string? email, string? direccion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del proveedor es obligatorio.");
        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre no puede exceder {LongitudMaximaNombre} caracteres.");

        var nitNormalizado = (nit ?? "").Trim();
        if (string.IsNullOrWhiteSpace(nitNormalizado))
            throw new ReglaDominioException("El NIT del proveedor es obligatorio.");
        if (nitNormalizado.Length > LongitudMaximaNit)
            throw new ReglaDominioException($"El NIT no puede exceder {LongitudMaximaNit} caracteres.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(nitNormalizado, @"^\d{4}-\d{6}-\d{3}-\d$"))
            throw new ReglaDominioException("El NIT tiene un formato inválido. Formato esperado: 0000-000000-000-0.");

        if (!string.IsNullOrWhiteSpace(email) && !System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ReglaDominioException("El email tiene un formato inválido.");

        Nombre = nombre.Trim();
        Nit = nitNormalizado;
        Contacto = string.IsNullOrWhiteSpace(contacto) ? null : contacto.Trim();
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
    }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;
}
