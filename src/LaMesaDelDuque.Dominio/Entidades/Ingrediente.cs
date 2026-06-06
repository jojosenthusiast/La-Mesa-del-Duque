using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Ingrediente
{
    private const int LongitudMaximaNombre = 150;
    private const int LongitudMaximaUnidadMedida = 20;

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string UnidadMedida { get; private set; }
    public decimal StockActual { get; private set; }
    public decimal StockMinimo { get; private set; }
    public decimal CostoUnitario { get; private set; }
    public Proveedor? ProveedorDefault { get; private set; }
    public bool Activo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Ingrediente()
    {
        Nombre = string.Empty;
        UnidadMedida = string.Empty;
    }

    public Ingrediente(
        string nombre,
        string unidadMedida,
        decimal stockActual,
        decimal stockMinimo,
        decimal costoUnitario,
        Proveedor? proveedorDefault = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del ingrediente es obligatorio.");

        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre del ingrediente no puede exceder {LongitudMaximaNombre} caracteres.");

        if (string.IsNullOrWhiteSpace(unidadMedida))
            throw new ReglaDominioException("La unidad de medida es obligatoria.");

        if (unidadMedida.Trim().Length > LongitudMaximaUnidadMedida)
            throw new ReglaDominioException($"La unidad de medida no puede exceder {LongitudMaximaUnidadMedida} caracteres.");

        if (stockActual < 0)
            throw new ReglaDominioException("El stock actual no puede ser negativo.");

        if (stockMinimo < 0)
            throw new ReglaDominioException("El stock mínimo no puede ser negativo.");

        if (costoUnitario < 0)
            throw new ReglaDominioException("El costo unitario no puede ser negativo.");

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        UnidadMedida = unidadMedida.Trim();
        StockActual = stockActual;
        StockMinimo = stockMinimo;
        CostoUnitario = costoUnitario;
        ProveedorDefault = proveedorDefault;
        Activo = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Actualizar(string nombre, string unidadMedida, decimal stockMinimo, decimal costoUnitario)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDominioException("El nombre del ingrediente es obligatorio.");
        if (nombre.Trim().Length > LongitudMaximaNombre)
            throw new ReglaDominioException($"El nombre no puede exceder {LongitudMaximaNombre} caracteres.");
        if (string.IsNullOrWhiteSpace(unidadMedida))
            throw new ReglaDominioException("La unidad de medida es obligatoria.");
        if (unidadMedida.Trim().Length > LongitudMaximaUnidadMedida)
            throw new ReglaDominioException($"La unidad de medida no puede exceder {LongitudMaximaUnidadMedida} caracteres.");
        if (stockMinimo < 0)
            throw new ReglaDominioException("El stock mínimo no puede ser negativo.");
        if (costoUnitario < 0)
            throw new ReglaDominioException("El costo unitario no puede ser negativo.");

        Nombre = nombre.Trim();
        UnidadMedida = unidadMedida.Trim();
        StockMinimo = stockMinimo;
        CostoUnitario = costoUnitario;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AjustarStock(decimal nuevoStock)
    {
        if (nuevoStock < 0)
            throw new ReglaDominioException("El stock no puede ser negativo.");
        StockActual = nuevoStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    public void AsignarProveedorDefault(Proveedor? proveedor)
    {
        ProveedorDefault = proveedor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DescontarStock(decimal cantidad)
    {
        if (cantidad <= 0)
            throw new ReglaDominioException("La cantidad a descontar debe ser mayor a cero.");

        if (StockActual < cantidad)
            throw new ReglaDominioException($"Stock insuficiente de {Nombre}: disponible {StockActual} {UnidadMedida}, solicitado {cantidad} {UnidadMedida}.");

        StockActual -= cantidad;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DevolverStock(decimal cantidad)
    {
        if (cantidad <= 0) throw new ReglaDominioException("La cantidad a devolver debe ser mayor a cero.");
        StockActual += cantidad;
        UpdatedAt = DateTime.UtcNow;
    }
}
