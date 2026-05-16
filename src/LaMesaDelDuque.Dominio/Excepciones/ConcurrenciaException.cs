namespace LaMesaDelDuque.Dominio.Excepciones;

public class ConcurrenciaException : Exception
{
    public ConcurrenciaException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }
}
