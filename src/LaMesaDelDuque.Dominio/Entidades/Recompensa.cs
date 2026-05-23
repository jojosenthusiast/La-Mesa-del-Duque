namespace LaMesaDelDuque.Dominio.Entidades;

public class Recompensa
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public int PuntosRequeridos { get; private set; }
    public bool Activo { get; private set; }

    private Recompensa()
    {
        Nombre = string.Empty;
    }

    public Recompensa(string nombre, int puntosRequeridos, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre requerido.");
        if (puntosRequeridos <= 0) throw new ArgumentException("Puntos requeridos debe ser >0.");
        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        PuntosRequeridos = puntosRequeridos;
        Descripcion = descripcion;
        Activo = true;
    }
}
