using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class RestauranteConfig
{
    private const int LongitudMaximaNombre = 150;
    private const int LongitudMaximaDireccion = 300;
    private const int LongitudMaximaTelefono = 20;

    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string Direccion { get; private set; }
    public string? Telefono { get; private set; }
    public TimeOnly HorarioApertura { get; private set; }
    public TimeOnly HorarioCierre { get; private set; }
    public int CantidadMesas { get; private set; }
    public string? DatosTicketJson { get; private set; }
    public int PeriodoGraciaMinutos { get; private set; } = 5;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private RestauranteConfig()
    {
        Nombre = string.Empty;
        Direccion = string.Empty;
    }

    public RestauranteConfig(
        string nombre,
        string direccion,
        TimeOnly horarioApertura,
        TimeOnly horarioCierre,
        int cantidadMesas,
        string? telefono = null,
        string? datosTicketJson = null,
        int periodoGraciaMinutos = 5)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del restaurante es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del restaurante no puede exceder {LongitudMaximaNombre} caracteres.");

        if (string.IsNullOrWhiteSpace(direccion))
            throw new ReglaDominioException("La dirección del restaurante es obligatoria.");

        if (direccion.Trim().Length > LongitudMaximaDireccion)
            throw new ReglaDominioException($"La dirección no puede exceder {LongitudMaximaDireccion} caracteres.");

        if (!string.IsNullOrWhiteSpace(telefono) && telefono.Trim().Length > LongitudMaximaTelefono)
            throw new ReglaDominioException($"El teléfono no puede exceder {LongitudMaximaTelefono} caracteres.");

        if (cantidadMesas <= 0)
            throw new ReglaDominioException("La cantidad de mesas debe ser mayor que cero.");

        if (horarioCierre <= horarioApertura)
            throw new ReglaDominioException("El horario de cierre debe ser posterior al horario de apertura.");

        Id = 1;
        Nombre = nombre.Trim();
        Direccion = direccion.Trim();
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        HorarioApertura = horarioApertura;
        HorarioCierre = horarioCierre;
        CantidadMesas = cantidadMesas;
        DatosTicketJson = datosTicketJson;
        PeriodoGraciaMinutos = periodoGraciaMinutos >= 0 && periodoGraciaMinutos <= 60 ? periodoGraciaMinutos : 5;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EstablecerGracia(int minutos)
    {
        if (minutos < 0 || minutos > 60)
            throw new ReglaDominioException("El período de gracia debe estar entre 0 y 60 minutos.");
        PeriodoGraciaMinutos = minutos;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarDatos(string nombre, string direccion, int cantidadMesas)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del restaurante es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del restaurante no puede exceder {LongitudMaximaNombre} caracteres.");

        if (string.IsNullOrWhiteSpace(direccion))
            throw new ReglaDominioException("La dirección del restaurante es obligatoria.");

        if (cantidadMesas <= 0)
            throw new ReglaDominioException("La cantidad de mesas debe ser mayor que cero.");

        Nombre = nombre.Trim();
        Direccion = direccion.Trim();
        CantidadMesas = cantidadMesas;
        UpdatedAt = DateTime.UtcNow;
    }
}
