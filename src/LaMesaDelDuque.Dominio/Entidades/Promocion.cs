using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Promocion
{
    private const int LongitudMaximaNombre = 150;
    private static readonly string[] TiposDescuentoValidos = ["porcentaje", "fijo"];

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public string TipoDescuento { get; private set; }
    public decimal ValorDescuento { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public bool Activo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Promocion()
    {
        Nombre = string.Empty;
        TipoDescuento = string.Empty;
    }

    public Promocion(
        string nombre,
        string tipoDescuento,
        decimal valorDescuento,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre de la promoción es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre de la promoción no puede exceder {LongitudMaximaNombre} caracteres.");

        if (string.IsNullOrWhiteSpace(tipoDescuento) || !Array.Exists(TiposDescuentoValidos, t => t == tipoDescuento.Trim().ToLowerInvariant()))
            throw new ReglaDominioException("El tipo de descuento debe ser 'porcentaje' o 'fijo'.");

        if (valorDescuento <= 0)
            throw new ReglaDominioException("El valor del descuento debe ser mayor que cero.");

        if (tipoDescuento.Trim().ToLowerInvariant() == "porcentaje" && valorDescuento > 100)
            throw new ReglaDominioException("El porcentaje de descuento no puede exceder 100.");

        if (fechaFin <= fechaInicio)
            throw new ReglaDominioException("La fecha de fin debe ser posterior a la fecha de inicio.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Descripcion = descripcion;
        TipoDescuento = tipoDescuento.Trim().ToLowerInvariant();
        ValorDescuento = valorDescuento;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Activo = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;
}
