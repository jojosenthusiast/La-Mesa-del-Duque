using System.Text.Json;
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
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task CargarDatosAsync()
    {
        var zonas = await _zonasServicio.ListarActivasAsync();
        var mesas = await _mesasServicio.ListarMesasAsync();

        // El parche anterior dependía de ZonaId/PosicionX/PosicionY ya sembrados.
        // En SQLite existente eso dejaba el mapa vacío. Aquí la vista siempre dibuja
        // las mesas activas usando posiciones de respaldo en porcentaje si faltan datos
        // o si quedaron posiciones viejas en pixeles (ej. 260, 440).
        if (zonas.Count == 0)
        {
            zonas.Add(new ZonaSalonDto
            {
                Id = Guid.Empty,
                Nombre = "Salón Principal",
                Orden = 1,
                Activa = true
            });
        }

        var zonaIds = zonas.Select(z => z.Id).ToHashSet();
        var zonaPrincipal = zonas.OrderBy(z => z.Orden).First();

        Vm.Zonas = zonas;
        Vm.Mesas = mesas
            .Where(m => m.Activa)
            .OrderBy(m => m.Numero)
            .Select((m, index) =>
            {
                var tieneZonaValida = m.ZonaId.HasValue && zonaIds.Contains(m.ZonaId.Value);
                var tienePosicionValida = m.PosicionX is >= 3 and <= 94 && m.PosicionY is >= 4 and <= 92;
                var col = index % 4;
                var fila = index / 4;
                var fallbackX = 12 + col * 23;
                var fallbackY = 18 + fila * 24;

                return new MesaMapaItemVm
                {
                    Id = m.Id,
                    Numero = m.Numero,
                    Capacidad = m.Capacidad,
                    Estado = m.Estado,
                    Activa = m.Activa,
                    PosicionX = tienePosicionValida ? m.PosicionX : fallbackX,
                    PosicionY = tienePosicionValida ? m.PosicionY : fallbackY,
                    ZonaId = tieneZonaValida ? m.ZonaId : zonaPrincipal.Id,
                    Forma = string.IsNullOrWhiteSpace(m.Forma) ? (m.Capacidad >= 8 ? "Cuadrada" : "Redonda") : m.Forma,
                    Rotacion = m.Rotacion ?? 0,
                    OcupadaDesde = m.OcupadaDesde
                };
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
