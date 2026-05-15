using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class OrdenCocina
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Guid? DetallePedidoId { get; private set; }
    public string ProductoNombre { get; private set; } = string.Empty;
    public int Cantidad { get; private set; }
    public string? Notas { get; private set; }
    public string? Alergenos { get; private set; }
    public string? IngredientesQuitados { get; private set; }
    public string? IngredientesExtra { get; private set; }
    public int? CocineroId { get; private set; }
    public EstacionCocina Estacion { get; private set; }
    public EstadoLineaCocina Estado { get; private set; }
    public DateTime HoraRecibido { get; private set; }
    public DateTime? HoraListo { get; private set; }
    public int? MesaNumero { get; private set; }
    public string? TipoServicio { get; private set; }

    private OrdenCocina()
    {
    }

    public OrdenCocina(Guid pedidoId, Guid? detallePedidoId, string productoNombre,
        int cantidad, EstacionCocina estacion, int? mesaNumero, string? tipoServicio,
        string? notas = null, string? alergenos = null, string? ingredientesQuitados = null,
        string? ingredientesExtra = null, int? cocineroId = null)
    {
        if (string.IsNullOrWhiteSpace(productoNombre))
            throw new ReglaDominioException("El nombre del producto es obligatorio.");

        if (cantidad <= 0)
            throw new ReglaDominioException("La cantidad debe ser mayor que cero.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        DetallePedidoId = detallePedidoId;
        ProductoNombre = productoNombre.Trim();
        Cantidad = cantidad;
        Notas = notas;
        Alergenos = alergenos;
        IngredientesQuitados = ingredientesQuitados;
        IngredientesExtra = ingredientesExtra;
        CocineroId = cocineroId;
        Estacion = estacion;
        Estado = EstadoLineaCocina.Pendiente;
        HoraRecibido = DateTime.UtcNow;
        MesaNumero = mesaNumero;
        TipoServicio = tipoServicio;
    }

    public void MarcarEnPreparacion()
    {
        if (Estado != EstadoLineaCocina.Pendiente)
            throw new ReglaDominioException("Solo pendiente puede pasar a en preparación.");

        Estado = EstadoLineaCocina.EnPreparacion;
    }

    public void MarcarComoListo()
    {
        if (Estado == EstadoLineaCocina.Listo)
            throw new ReglaDominioException("Ya está listo.");

        Estado = EstadoLineaCocina.Listo;
        HoraListo = DateTime.UtcNow;
    }

    public void Recuperar()
    {
        if (Estado != EstadoLineaCocina.Listo)
            throw new ReglaDominioException("Solo se puede recuperar un item listo.");

        Estado = EstadoLineaCocina.EnPreparacion;
        HoraListo = null;
    }
}
