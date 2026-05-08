using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class MermaDiaria
{
    private const int LongitudMaximaNotas = 500;

    public Guid Id { get; private set; }
    public Guid CierreDiaId { get; private set; }
    public CierreDia CierreDia { get; private set; }
    public Guid IngredienteId { get; private set; }
    public Ingrediente Ingrediente { get; private set; }
    public decimal CantidadDescartada { get; private set; }
    public decimal CostoEstimado { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; }
    public string? Notas { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MermaDiaria()
    {
        CierreDia = null!;
        Ingrediente = null!;
        Usuario = null!;
    }

    public MermaDiaria(
        CierreDia cierreDia,
        Ingrediente ingrediente,
        decimal cantidadDescartada,
        Usuario usuario,
        decimal costoEstimado = 0,
        string? notas = null)
    {
        if (cierreDia is null)
            throw new ReglaDominioException("El cierre de día es obligatorio para la merma.");

        if (ingrediente is null)
            throw new ReglaDominioException("El ingrediente es obligatorio para la merma.");

        if (cantidadDescartada <= 0)
            throw new ReglaDominioException("La cantidad descartada debe ser mayor que cero.");

        if (usuario is null)
            throw new ReglaDominioException("El usuario es obligatorio para registrar la merma.");

        if (costoEstimado < 0)
            throw new ReglaDominioException("El costo estimado no puede ser negativo.");

        if (!string.IsNullOrWhiteSpace(notas) && notas.Trim().Length > LongitudMaximaNotas)
            throw new ReglaDominioException($"Las notas no pueden exceder {LongitudMaximaNotas} caracteres.");

        Id = Guid.NewGuid();
        CierreDia = cierreDia;
        CierreDiaId = cierreDia.Id;
        Ingrediente = ingrediente;
        IngredienteId = ingrediente.Id;
        CantidadDescartada = cantidadDescartada;
        CostoEstimado = costoEstimado;
        Usuario = usuario;
        UsuarioId = usuario.Id;
        Notas = string.IsNullOrWhiteSpace(notas) ? null : notas.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
