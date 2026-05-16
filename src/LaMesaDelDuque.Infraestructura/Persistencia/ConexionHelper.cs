namespace LaMesaDelDuque.Infraestructura.Persistencia;

internal static class ConexionHelper
{
    public static string Normalizar(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión es obligatoria.");

        string kvString = connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
                          connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            ? ConvertirUriAKeyValue(connectionString)
            : connectionString;

        // Parámetros requeridos por Supabase transaction pooler (pgBouncer)
        if (!kvString.Contains("No Reset On Close", StringComparison.OrdinalIgnoreCase))
            kvString += ";No Reset On Close=true";

        if (!kvString.Contains("Ssl Mode", StringComparison.OrdinalIgnoreCase))
            kvString += ";Ssl Mode=Require";

        if (!kvString.Contains("Trust Server Certificate", StringComparison.OrdinalIgnoreCase))
            kvString += ";Trust Server Certificate=true";

        return kvString;
    }

    private static string ConvertirUriAKeyValue(string uri)
    {
        var u = new Uri(uri);
        var userInfo = u.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = u.Host;
        var port = u.Port > 0 ? u.Port : 5432;
        var database = u.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}
