using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Mesa
{
    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public int Capacidad { get; private set; }
    public EstadoMesa Estado { get; private set; }
    public bool Activa { get; private set; }
    public DateTime? OcupadaDesde { get; private set; }

    // Campos de posición para mapa visual (nullable para compatibilidad con mesas legacy)
    public int? PosicionX { get; private set; }
    public int? PosicionY { get; private set; }
    public Guid? ZonaId { get; private set; }
    public ZonaSalon? Zona { get; private set; }
    public FormaMesa? Forma { get; private set; }
    public int? Rotacion { get; private set; }

    private Mesa()
    {
    }

    public Mesa(int numero, int capacidad)
    {
        if (numero <= 0)
            throw new ReglaDominioException("El número de mesa debe ser mayor que cero.");

        if (capacidad <= 0)
            throw new ReglaDominioException("La capacidad de la mesa debe ser mayor que cero.");

        Id = Guid.NewGuid();
        Numero = numero;
        Capacidad = capacidad;
        Estado = EstadoMesa.Disponible;
        Activa = true;
    }

    public void CambiarEstado(EstadoMesa nuevoEstado)
    {
        if (nuevoEstado == EstadoMesa.Ocupada)
            OcupadaDesde = DateTime.UtcNow;
        else if (Estado == EstadoMesa.Ocupada && nuevoEstado != EstadoMesa.Ocupada)
            OcupadaDesde = null;
        Estado = nuevoEstado;
    }

    public void Ocupar()
    {
        Estado = EstadoMesa.Ocupada;
        OcupadaDesde = DateTime.UtcNow;
    }

    public void Liberar()
    {
        Estado = EstadoMesa.Disponible;
        OcupadaDesde = null;
    }

    public void Desactivar()
    {
        Activa = false;
    }

    public void Activar()
    {
        Activa = true;
    }

    public void ActualizarDatos(int numero, int capacidad)
    {
        if (numero <= 0)
            throw new ReglaDominioException("El número de mesa debe ser mayor que cero.");

        if (capacidad <= 0)
            throw new ReglaDominioException("La capacidad de la mesa debe ser mayor que cero.");

        Numero = numero;
        Capacidad = capacidad;
    }

    public void ActualizarPosicion(int posicionX, int posicionY, Guid zonaId, FormaMesa forma, int? rotacion = null)
    {
        if (posicionX < 0)
            throw new ReglaDominioException("La posición X no puede ser negativa.");

        if (posicionY < 0)
            throw new ReglaDominioException("La posición Y no puede ser negativa.");

        if (rotacion is < 0 or > 359)
            throw new ReglaDominioException("La rotación debe estar entre 0 y 359 grados.");

        PosicionX = posicionX;
        PosicionY = posicionY;
        ZonaId = zonaId;
        Forma = forma;
        Rotacion = rotacion ?? 0;
    }

    public void LimpiarPosicion()
    {
        PosicionX = null;
        PosicionY = null;
        ZonaId = null;
        Forma = null;
        Rotacion = null;
    }
}
