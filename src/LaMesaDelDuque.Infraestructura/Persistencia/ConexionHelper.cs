namespace LaMesaDelDuque.Infraestructura.Persistencia;

internal static class ConexionHelper
{
    public static string Normalizar(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión es obligatoria.");

        return connectionString;
    }
}
