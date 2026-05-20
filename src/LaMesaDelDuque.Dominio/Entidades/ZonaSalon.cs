using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class ZonaSalon
{
    private const int LongitudMaximaNombre = 100;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public int Orden { get; private set; }
    public bool Activa { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ZonaSalon()
    {
        Nombre = string.Empty;
    }

    public ZonaSalon(string nombre, int orden = 0)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre de la zona es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre de la zona no puede exceder {LongitudMaximaNombre} caracteres.");

        if (orden < 0)
            throw new ReglaDominioException("El orden de la zona no puede ser negativo.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Orden = orden;
        Activa = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void ActualizarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre de la zona es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre de la zona no puede exceder {LongitudMaximaNombre} caracteres.");

        Nombre = nombre.Trim();
    }

    public void ActualizarOrden(int orden)
    {
        if (orden < 0)
            throw new ReglaDominioException("El orden de la zona no puede ser negativo.");

        Orden = orden;
    }

    public void Desactivar()
    {
        Activa = false;
    }

    public void Activar()
    {
        Activa = true;
    }
}
