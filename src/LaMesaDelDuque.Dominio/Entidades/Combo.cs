using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Combo
{
    private const int LongitudMaximaNombre = 150;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public decimal PrecioCombo { get; private set; }
    public bool Activo { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly? FechaFin { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Combo()
    {
        Nombre = string.Empty;
    }

    public Combo(
        string nombre,
        decimal precioCombo,
        DateOnly fechaInicio,
        string? descripcion = null,
        DateOnly? fechaFin = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del combo es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del combo no puede exceder {LongitudMaximaNombre} caracteres.");

        if (precioCombo <= 0)
            throw new ReglaDominioException("El precio del combo debe ser mayor que cero.");

        if (fechaFin.HasValue && fechaFin.Value <= fechaInicio)
            throw new ReglaDominioException("La fecha de fin debe ser posterior a la fecha de inicio.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Descripcion = descripcion;
        PrecioCombo = precioCombo;
        Activo = true;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        CreatedAt = DateTime.UtcNow;
    }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;
}
