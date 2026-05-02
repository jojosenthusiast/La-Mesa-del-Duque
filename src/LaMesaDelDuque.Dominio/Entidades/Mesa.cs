using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Mesa
{
    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public int Capacidad { get; private set; }
    public EstadoMesa Estado { get; private set; }

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
    }

    public void CambiarEstado(EstadoMesa nuevoEstado)
    {
        Estado = nuevoEstado;
    }
}
