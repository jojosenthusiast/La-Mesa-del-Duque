using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class TurnoCaja
{
    public Guid Id { get; private set; }
    public Guid CajeroId { get; private set; }
    public Usuario Cajero { get; private set; } = null!;
    public decimal FondoInicial { get; private set; }
    public DateTime FechaApertura { get; private set; }
    public DateTime? FechaCierre { get; private set; }
    public bool Cerrado { get; private set; }

    public decimal? EfectivoEsperado { get; private set; }
    public decimal? EfectivoContado { get; private set; }
    public decimal? Diferencia { get; private set; }
    public string? ObservacionCierre { get; private set; }
    public string? FirmaDigital { get; private set; }

    public IReadOnlyList<MovimientoCaja> Movimientos => _movimientos.AsReadOnly();
    private readonly List<MovimientoCaja> _movimientos = [];

    private TurnoCaja() { }

    public TurnoCaja(Guid cajeroId, decimal fondoInicial)
    {
        if (fondoInicial < 0) throw new ReglaDominioException("El fondo inicial no puede ser negativo.");
        Id = Guid.NewGuid();
        CajeroId = cajeroId;
        FondoInicial = fondoInicial;
        FechaApertura = DateTime.UtcNow;
        Cerrado = false;
    }

    public void Cerrar(decimal efectivoContado, string? observacion)
    {
        if (Cerrado) throw new ReglaDominioException("El turno ya está cerrado.");
        if (efectivoContado < 0) throw new ReglaDominioException("El efectivo contado no puede ser negativo.");
        var diferencia = efectivoContado - (EfectivoEsperado ?? 0);
        if (diferencia != 0 && string.IsNullOrWhiteSpace(observacion))
            throw new ReglaDominioException("La observación es obligatoria cuando hay diferencia de caja.");
        FechaCierre = DateTime.UtcNow;
        EfectivoContado = efectivoContado;
        Diferencia = diferencia;
        ObservacionCierre = observacion?.Trim();
        FirmaDigital = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{CajeroId}|{FechaCierre}|{diferencia}")));
        Cerrado = true;
    }

    public void EstablecerEfectivoEsperado(decimal monto) => EfectivoEsperado = monto;

    public void RegistrarMovimiento(MovimientoCaja movimiento)
    {
        if (Cerrado) throw new ReglaDominioException("No se pueden registrar movimientos en un turno cerrado.");
        _movimientos.Add(movimiento);
    }
}
