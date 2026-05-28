using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Salon;

[Authorize(Roles = "Administrador,Encargado")]
public class MapaModel : PageModel
{
    private readonly IMesasServicio _mesasServicio;
    private readonly IZonasSalonServicio _zonasServicio;

    public MapaModel(IMesasServicio mesasServicio, IZonasSalonServicio zonasServicio)
    {
        _mesasServicio = mesasServicio;
        _zonasServicio = zonasServicio;
    }

    [BindProperty]
    public MapaSalonVm Vm { get; set; } = new();

    public async Task OnGetAsync()
    {
        SetUiContext();
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnPostActualizarPosicionAsync([FromBody] ActualizarPosicionRequest request)
    {
        if (User?.IsInRole("Administrador") != true && User?.IsInRole("Encargado") != true)
        {
            return new JsonResult(new { exito = false, error = "No autorizado." }) { StatusCode = 403 };
        }

        try
        {
            var dto = await _mesasServicio.ActualizarPosicionAsync(
                request.MesaId, request.PosicionX, request.PosicionY,
                request.ZonaId, request.Forma, request.Rotacion);

            return new JsonResult(new { exito = true, mesa = dto });
        }
        catch (ReglaDominioException ex)
        {
            return new JsonResult(new { exito = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return new JsonResult(new { exito = false, error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync([FromBody] CambiarEstadoRequest request)
    {
        if (User?.IsInRole("Administrador") != true && User?.IsInRole("Encargado") != true && User?.IsInRole("Mesero") != true)
        {
            return new JsonResult(new { exito = false, error = "No autorizado." }) { StatusCode = 403 };
        }

        try
        {
            await _mesasServicio.CambiarEstadoMesaAsync(request.MesaId, request.NuevoEstado);
            return new JsonResult(new { exito = true });
        }
        catch (ReglaDominioException ex)
        {
            return new JsonResult(new { exito = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return new JsonResult(new { exito = false, error = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetObtenerDatosAsync()
    {
        await CargarDatosAsync();
        return new JsonResult(new
        {
            zonas = Vm.Zonas,
            mesas = Vm.Mesas,
            puedeEditar = Vm.PuedeEditar
        });
    }

    private async Task CargarDatosAsync()
    {
        var zonas = await _zonasServicio.ListarActivasAsync();
        var mesas = await _mesasServicio.ListarMesasAsync();

        Vm.Zonas = zonas;
        Vm.Mesas = mesas
            .Where(m => m.PosicionX.HasValue && m.PosicionY.HasValue && m.ZonaId.HasValue)
            .Select(m => new MesaMapaItemVm
            {
                Id = m.Id,
                Numero = m.Numero,
                Capacidad = m.Capacidad,
                Estado = m.Estado,
                Activa = m.Activa,
                PosicionX = m.PosicionX,
                PosicionY = m.PosicionY,
                ZonaId = m.ZonaId,
                Forma = m.Forma,
                Rotacion = m.Rotacion,
                OcupadaDesde = m.OcupadaDesde
            })
            .ToList();

        Vm.PuedeEditar = User.IsInRole("Administrador") || User.IsInRole("Encargado");
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Mapa";
        }
    }

    public class ActualizarPosicionRequest
    {
        public Guid MesaId { get; set; }
        public int PosicionX { get; set; }
        public int PosicionY { get; set; }
        public Guid ZonaId { get; set; }
        public string Forma { get; set; } = string.Empty;
        public int? Rotacion { get; set; }
    }

    public class CambiarEstadoRequest
    {
        public Guid MesaId { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
