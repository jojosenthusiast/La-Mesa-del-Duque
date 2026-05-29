using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class MotivoDescuento
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MotivoDescuento() { Nombre = string.Empty; }

    public MotivoDescuento(string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaDominioException("El nombre del motivo es obligatorio.");
        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        Activo = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
}
