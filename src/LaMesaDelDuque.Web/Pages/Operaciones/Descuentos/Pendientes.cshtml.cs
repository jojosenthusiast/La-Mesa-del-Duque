using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Descuentos;

[Authorize(Roles = "Administrador,Encargado")]
public class PendientesModel : PageModel
{
    private readonly IDescuentoServicio _servicio;
    private readonly ILogger<PendientesModel> _logger;

    [TempData] public string? ToastError { get; set; }
    [TempData] public string? ToastSuccess { get; set; }

    public List<DescuentoAplicadoDto> Pendientes { get; set; } = [];

    public PendientesModel(IDescuentoServicio servicio, ILogger<PendientesModel> logger)
    {
        _servicio = servicio;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        Pendientes = await _servicio.ObtenerPendientesAsync();
    }

    public async Task<IActionResult> OnPostAprobarAsync(Guid descuentoId)
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }

        try
        {
            await _servicio.AprobarDescuentoAsync(descuentoId, usuarioId);
            ToastSuccess = "Descuento aprobado correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar descuento {DescuentoId}", descuentoId);
            ToastError = "Error interno al aprobar el descuento.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRechazarAsync(Guid descuentoId, string? nota)
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }

        try
        {
            await _servicio.RechazarDescuentoAsync(descuentoId, usuarioId, nota);
            ToastSuccess = "Descuento rechazado.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar descuento {DescuentoId}", descuentoId);
            ToastError = "Error interno al rechazar el descuento.";
        }

        return RedirectToPage();
    }

    private Guid GetUsuarioId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
    }
}
