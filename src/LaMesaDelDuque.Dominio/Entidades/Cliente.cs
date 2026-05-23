using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Cliente
{
    private const int MaxNombre = 200;
    private const int MaxTelefono = 20;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Telefono { get; private set; }
    public string? Notas { get; private set; }
    public bool Activo { get; private set; }
    public int PuntosAcumulados { get; private set; }
    public int VisitasTotales { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime? UltimaVisita { get; private set; }

    private Cliente()
    {
        Nombre = string.Empty;
        Telefono = string.Empty;
    }

    public Cliente(string nombre, string telefono, string? notas = null)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > MaxNombre)
            throw new ReglaDominioException($"El nombre es obligatorio y no puede exceder {MaxNombre} caracteres.");
        if (string.IsNullOrWhiteSpace(telefono) || telefono.Trim().Length > MaxTelefono)
            throw new ReglaDominioException("El teléfono es obligatorio.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Telefono = telefono.Trim();
        Notas = notas?.Trim();
        Activo = true;
        PuntosAcumulados = 0;
        VisitasTotales = 0;
        CreadoEn = DateTime.UtcNow;
    }

    public void AcumularPuntos(int puntos)
    {
        if (puntos < 0) throw new ReglaDominioException("Los puntos no pueden ser negativos.");
        PuntosAcumulados += puntos;
    }

    public void RestarPuntos(int puntos)
    {
        if (puntos < 0) throw new ReglaDominioException("Los puntos no pueden ser negativos.");
        if (puntos > PuntosAcumulados) throw new ReglaDominioException("Puntos insuficientes.");
        PuntosAcumulados -= puntos;
    }

    public void RegistrarVisita()
    {
        VisitasTotales++;
        UltimaVisita = DateTime.UtcNow;
    }

    public void Desactivar() => Activo = false;
    public void ActivarCliente() => Activo = true;
}
