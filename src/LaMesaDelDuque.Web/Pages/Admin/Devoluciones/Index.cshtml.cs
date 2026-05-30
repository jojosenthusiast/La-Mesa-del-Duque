using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Admin.Devoluciones;

[Authorize(Roles = "Administrador,Gerente")]
public class IndexModel : PageModel
{
    private readonly IDevolucionServicio _devolucionServicio;
    private readonly IUnidadDeTrabajo _uot;
    private readonly ILogger<IndexModel> _logger;

    [TempData] public string? ToastError { get; set; }
    [TempData] public string? ToastSuccess { get; set; }

    public string? BusquedaPagoId { get; set; }
    public PagoDto? PagoEncontrado { get; set; }
    public List<DevolucionPagoDto> DevolucionesDelDia { get; set; } = [];

    public IndexModel(
        IDevolucionServicio devolucionServicio,
        IUnidadDeTrabajo uot,
        ILogger<IndexModel> logger)
    {
        _devolucionServicio = devolucionServicio;
        _uot = uot;
        _logger = logger;
    }

    public async Task OnGetAsync(string? pagoId)
    {
        BusquedaPagoId = pagoId;
        DevolucionesDelDia = await _devolucionServicio.ObtenerDelDiaAsync();

        if (!string.IsNullOrWhiteSpace(pagoId) && Guid.TryParse(pagoId, out var id))
        {
            var pago = await _uot.Pagos.ObtenerPorIdAsync(id);
            if (pago is not null)
            {
                PagoEncontrado = new PagoDto
                {
                    Id = pago.Id,
                    CuentaId = pago.CuentaId,
                    Monto = pago.Monto,
                    PropinaMonto = pago.PropinaMonto,
                    Metodo = pago.Metodo.ToString(),
                    FechaPago = pago.FechaPago,
                };
            }
        }
    }

    public async Task<IActionResult> OnPostProcesarDevolucionAsync(
        Guid pagoOriginalId,
        decimal montoDevuelto,
        string metodoDevolucion,
        string motivo)
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }

        try
        {
            await _devolucionServicio.ProcesarDevolucionAsync(
                pagoOriginalId, montoDevuelto, metodoDevolucion, motivo, usuarioId);

            ToastSuccess = $"Devolución de {montoDevuelto:C} procesada correctamente.";
            return RedirectToPage();
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar devolución del pago {PagoId}", pagoOriginalId);
            ToastError = "Error interno al procesar la devolución.";
        }

        // Volver a cargar datos en caso de error
        DevolucionesDelDia = await _devolucionServicio.ObtenerDelDiaAsync();
        if (pagoOriginalId != Guid.Empty)
        {
            var pago = await _uot.Pagos.ObtenerPorIdAsync(pagoOriginalId);
            if (pago is not null)
            {
                BusquedaPagoId = pagoOriginalId.ToString();
                PagoEncontrado = new PagoDto
                {
                    Id = pago.Id,
                    CuentaId = pago.CuentaId,
                    Monto = pago.Monto,
                    PropinaMonto = pago.PropinaMonto,
                    Metodo = pago.Metodo.ToString(),
                    FechaPago = pago.FechaPago,
                };
            }
        }

        return Page();
    }

    private Guid GetUsuarioId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
    }
}
