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
}
