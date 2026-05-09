using Npgsql;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

internal static class ConexionHelper
{
    public static string Normalizar(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión es obligatoria.");

        if (connectionString.Contains('='))
            return connectionString;

        // Formato URL → extraer partes manualmente para manejar # en password
        if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var sinPrefijo = connectionString[(connectionString.IndexOf("://", StringComparison.Ordinal) + 3)..];
            var ultimoArroba = sinPrefijo.LastIndexOf('@');
            var creds = sinPrefijo[..ultimoArroba];
            var hostInfo = sinPrefijo[(ultimoArroba + 1)..];
            var dosPuntos = creds.IndexOf(':');
            var usuario = creds[..dosPuntos];
            var password = creds[(dosPuntos + 1)..];
            var partes = hostInfo.Split('/');
            var hp = partes[0].Split(':');
            var host = hp[0];
            var port = hp.Length > 1 ? hp[1] : "5432";
            var db = partes.Length > 1 ? partes[1] : "postgres";

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = int.Parse(port),
                Database = db,
                Username = usuario,
                Password = password,
                SslMode = SslMode.Require
            };

            return builder.ConnectionString;
        }

        return connectionString;
    }
}
