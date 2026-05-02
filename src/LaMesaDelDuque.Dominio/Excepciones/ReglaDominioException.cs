namespace LaMesaDelDuque.Dominio.Excepciones;

/// <summary>
/// Excepción base para violaciones de reglas de negocio en el dominio.
/// </summary>
public class ReglaDominioException : Exception
{
    public ReglaDominioException(string mensaje) : base(mensaje) { }

    public ReglaDominioException(string mensaje, Exception excepcionInterna)
        : base(mensaje, excepcionInterna) { }
}
