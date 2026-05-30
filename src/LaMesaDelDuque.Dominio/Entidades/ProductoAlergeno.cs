namespace LaMesaDelDuque.Dominio.Entidades;

/// <summary>
/// Relación producto-alérgeno. Justifica por qué un alérgeno aplica a este producto.
/// Puede ser por ingrediente directo ("Camarón en coctel de mariscos")
/// o por cross-contaminación ("Freidora compartida con mariscos").
/// </summary>
public class ProductoAlergeno
{
    public Guid Id { get; private set; }
    public Guid ProductoId { get; private set; }
    public Producto Producto { get; private set; }
    public Guid AlergenoId { get; private set; }
    public Alergeno Alergeno { get; private set; }
    public string? Justificacion { get; private set; }

    private ProductoAlergeno()
    {
        Producto = null!;
        Alergeno = null!;
    }

    public ProductoAlergeno(Producto producto, Alergeno alergeno, string? justificacion = null)
    {
        Id = Guid.NewGuid();
        ProductoId = producto.Id;
        Producto = producto;
        AlergenoId = alergeno.Id;
        Alergeno = alergeno;
        Justificacion = justificacion;
    }
}
