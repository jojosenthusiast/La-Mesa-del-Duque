using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class RecetaProducto
{
    private readonly List<RecetaIngrediente> _ingredientes = [];

    public Guid Id { get; private set; }
    public Guid ProductoId { get; private set; }
    public Producto Producto { get; private set; }
    public string Instrucciones { get; private set; }
    public IReadOnlyList<RecetaIngrediente> Ingredientes => _ingredientes.AsReadOnly();

    private RecetaProducto()
    {
        Producto = null!;
        Instrucciones = string.Empty;
    }

    public RecetaProducto(Producto producto, string instrucciones, IEnumerable<RecetaIngrediente> ingredientes)
    {
        if (producto is null)
            throw new ReglaDominioException("El producto es obligatorio para la receta.");
        if (string.IsNullOrWhiteSpace(instrucciones))
            throw new ReglaDominioException("Las instrucciones de la receta son obligatorias.");

        var ingredientesLista = ingredientes?.ToList() ?? [];
        if (ingredientesLista.Count == 0)
            throw new ReglaDominioException("La receta debe tener al menos un ingrediente.");

        Id = Guid.NewGuid();
        Producto = producto;
        ProductoId = producto.Id;
        Instrucciones = instrucciones.Trim();

        foreach (var ingrediente in ingredientesLista)
        {
            AgregarIngrediente(ingrediente);
        }
    }

    public void AgregarIngrediente(RecetaIngrediente ingrediente)
    {
        if (ingrediente is null)
            throw new ReglaDominioException("El ingrediente de receta es obligatorio.");
        if (_ingredientes.Any(x => x.IngredienteId == ingrediente.IngredienteId))
            throw new ReglaDominioException("El ingrediente ya existe en la receta.");

        _ingredientes.Add(ingrediente);
    }
}
