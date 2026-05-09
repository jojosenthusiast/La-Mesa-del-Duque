using Npgsql;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

/// <summary>
/// Normaliza cadenas de conexión PostgreSQL de Supabase.
/// Detecta automáticamente URLs de pooler y las convierte
/// al formato clave-valor que Npgsql puede procesar correctamente
/// con contraseñas que contienen caracteres especiales.
/// </summary>
internal static class ConexionHelper
{
    public static string Normalizar(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión es obligatoria.");

        if (connectionString.Contains('='))
            return connectionString;

        if (!connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var (host, port, database, usuario, password) = ParsearUrl(connectionString);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.Parse(port),
            Database = database,
            Username = usuario,
            Password = password,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }

    private static (string host, string port, string database, string usuario, string password) ParsearUrl(string url)
    {
        var sinPrefijo = url[(url.IndexOf("://", StringComparison.Ordinal) + 3)..];

        var indiceArroba = sinPrefijo.LastIndexOf('@');
        var credenciales = sinPrefijo[..indiceArroba];
        var hostInfo = sinPrefijo[(indiceArroba + 1)..];

        var indiceDosPuntos = credenciales.IndexOf(':');
        var usuario = credenciales[..indiceDosPuntos];
        var password = credenciales[(indiceDosPuntos + 1)..];

        var partesHost = hostInfo.Split('/');
        var hostPuerto = partesHost[0].Split(':');
        var host = hostPuerto[0];
        var port = hostPuerto.Length > 1 ? hostPuerto[1] : "5432";
        var database = partesHost.Length > 1 ? partesHost[1] : "postgres";

        return (host, port, database, usuario, password);
    }
}
