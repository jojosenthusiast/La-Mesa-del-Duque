using System.Security.Claims;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Mesero;

[Authorize(Roles = "Administrador,Encargado,Mesero")]
public class HandoffModel : PageModel
{
    private readonly IShiftHandoffServicio _shiftHandoffServicio;
    private readonly IUsuariosServicio _usuariosServicio;
    private readonly ILogger<HandoffModel> _logger;

    public HandoffModel(
        IShiftHandoffServicio shiftHandoffServicio,
        IUsuariosServicio usuariosServicio,
        ILogger<HandoffModel> logger)
    {
        _shiftHandoffServicio = shiftHandoffServicio;
        _usuariosServicio = usuariosServicio;
        _logger = logger;
    }

    public List<MesaAsignadaDto> MesasActivas { get; private set; } = [];
    public List<UsuarioDto> MeserosActivos { get; private set; } = [];

    [BindProperty]
    public Guid MesaId { get; set; }

    [BindProperty]
    public Guid NuevoMeseroId { get; set; }

    [TempData]
    public string? ToastOk { get; set; }

    [TempData]
    public string? ToastError { get; set; }

    public async Task OnGetAsync(CancellationToken cancelacion = default)
    {
        ConfigurarVista();
        await CargarDatosAsync(cancelacion);
    }

    public async Task<IActionResult> OnPostTransferirAsync(CancellationToken cancelacion = default)
    {
        ConfigurarVista();

        var usuarioResponsableId = ObtenerUsuarioActualId();
        if (usuarioResponsableId == Guid.Empty)
        {
            ToastError = "No se pudo identificar al usuario actual para transferir la mesa.";
            await CargarDatosAsync(cancelacion);
            return Page();
        }

        if (MesaId == Guid.Empty || NuevoMeseroId == Guid.Empty)
        {
            ToastError = "Seleccione una mesa activa y el mesero que recibirá la mesa.";
            await CargarDatosAsync(cancelacion);
            return Page();
        }

        try
        {
            await _shiftHandoffServicio.TransferirMesaAsync(MesaId, NuevoMeseroId, usuarioResponsableId, cancelacion);
            ToastOk = "Mesa transferida correctamente.";
            return RedirectToPage();
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
            await CargarDatosAsync(cancelacion);
            return Page();
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
            await CargarDatosAsync(cancelacion);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al transferir mesa {MesaId} al mesero {MeseroId}", MesaId, NuevoMeseroId);
            ToastError = "Ocurrió un error interno al transferir la mesa.";
            await CargarDatosAsync(cancelacion);
            return Page();
        }
    }


    private void ConfigurarVista()
    {
        if (PageContext?.ViewData is null)
            return;

        PageContext.ViewData["Title"] = "Transferir mesas";
        PageContext.ViewData["ActiveTab"] = "MeseroHandoff";
    }

    private async Task CargarDatosAsync(CancellationToken cancelacion)
    {
        MeserosActivos = await ObtenerMeserosActivosAsync(cancelacion);

        var usuarioId = ObtenerUsuarioActualId();
        if (usuarioId == Guid.Empty)
        {
            MesasActivas = [];
            return;
        }

        try
        {
            MesasActivas = await _shiftHandoffServicio.ObtenerMesasActivasAsync(usuarioId, cancelacion);
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
            MesasActivas = [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cargar mesas activas para handoff");
            ToastError = "Ocurrió un error interno al cargar las mesas activas.";
            MesasActivas = [];
        }
    }

    private async Task<List<UsuarioDto>> ObtenerMeserosActivosAsync(CancellationToken cancelacion)
    {
        var usuarios = await _usuariosServicio.ListarUsuariosAsync(cancelacion);
        return usuarios
            .Where(u => u.Activo && string.Equals(u.RolNombre, "Mesero", StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.NombreCompleto)
            .ThenBy(u => u.Username)
            .ToList();
    }

    private Guid ObtenerUsuarioActualId()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var usuarioId) ? usuarioId : Guid.Empty;
    }
}
