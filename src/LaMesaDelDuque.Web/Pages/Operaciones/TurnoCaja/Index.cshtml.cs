using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Operaciones.TurnoCaja;

[Authorize(Roles = "Administrador,Encargado,Cajero")]
public class IndexModel : PageModel
{
    private readonly ITurnoCajaServicio _servicio;
    private readonly ILogger<IndexModel> _logger;

    public TurnoCajaDto? TurnoActivo { get; set; }

    [TempData] public string? ToastError { get; set; }
    [TempData] public string? ToastSuccess { get; set; }

    public IndexModel(ITurnoCajaServicio servicio, ILogger<IndexModel> logger)
    {
        _servicio = servicio;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        TurnoActivo = await _servicio.ObtenerTurnoActivoAsync();
    }

    public async Task<IActionResult> OnPostAbrirAsync(decimal fondoInicial)
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }
        try
        {
            await _servicio.AbrirTurnoAsync(usuarioId, fondoInicial);
            ToastSuccess = "Turno de caja abierto correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir turno de caja");
            ToastError = "Error interno al abrir el turno.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCerrarAsync(Guid turnoId, decimal efectivoContado, string? observacion)
    {
        try
        {
            await _servicio.CerrarTurnoAsync(turnoId, efectivoContado, observacion);
            ToastSuccess = "Turno de caja cerrado correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar turno de caja");
            ToastError = "Error interno al cerrar el turno.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMovimientoAsync(Guid turnoId, string tipo, decimal monto, string motivo)
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }
        try
        {
            await _servicio.RegistrarMovimientoAsync(turnoId, tipo, monto, motivo, usuarioId);
            ToastSuccess = "Movimiento registrado correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar movimiento de caja");
            ToastError = "Error interno al registrar el movimiento.";
        }
        return RedirectToPage();
    }

    private Guid GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
