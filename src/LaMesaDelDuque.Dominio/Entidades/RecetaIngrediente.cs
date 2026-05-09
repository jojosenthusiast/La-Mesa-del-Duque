using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class RecetaIngrediente
{
    public Guid Id { get; private set; }
    public Guid IngredienteId { get; private set; }
    public Ingrediente Ingrediente { get; private set; }
    public decimal CantidadRequerida { get; private set; }

    private RecetaIngrediente()
    {
        Ingrediente = null!;
    }

    public RecetaIngrediente(Ingrediente ingrediente, decimal cantidadRequerida)
    {
        if (ingrediente is null)
            throw new ReglaDominioException("El ingrediente es obligatorio en la receta.");

        if (cantidadRequerida <= 0)
            throw new ReglaDominioException("La cantidad requerida debe ser mayor que cero.");

        Id = Guid.NewGuid();
        Ingrediente = ingrediente;
        IngredienteId = ingrediente.Id;
        CantidadRequerida = cantidadRequerida;
    }
}
