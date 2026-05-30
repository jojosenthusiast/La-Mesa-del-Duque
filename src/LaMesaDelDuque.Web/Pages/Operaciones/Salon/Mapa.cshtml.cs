using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Salon;

[Authorize(Roles = "Administrador,Encargado,Mesero")]
public class MapaModel : PageModel
{
    private readonly IMesasServicio _mesasServicio;
    private readonly IZonasSalonServicio _zonasServicio;
    private readonly ILogger<MapaModel> _logger;

    public MapaModel(IMesasServicio mesasServicio, IZonasSalonServicio zonasServicio, ILogger<MapaModel> logger)
    {
        _mesasServicio = mesasServicio;
        _zonasServicio = zonasServicio;
        _logger = logger;
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
        catch (Exception ex)
        {
            return ErrorInesperadoJson(ex, "actualizar posicion de mesa");
        }
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync([FromBody] CambiarEstadoRequest request)
    {
        if (User?.IsInRole("Administrador") != true && User?.IsInRole("Encargado") != true)
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
        catch (Exception ex)
        {
            return ErrorInesperadoJson(ex, "cambiar estado de mesa");
        }
    }

    public async Task<IActionResult> OnGetObtenerDatosAsync()
    {
        await CargarDatosAsync();
        return new JsonResult(new
        {
            zonas = Vm.Zonas,
            mesas = Vm.Mesas,
            puedeEditar = Vm.PuedeEditar,
            totalMesas = Vm.TotalMesas,
            mesasPendientesUbicacion = Vm.MesasPendientesUbicacion,
            usaZonaSugerida = Vm.UsaZonaSugerida
        });
    }

    private JsonResult ErrorInesperadoJson(Exception ex, string accion)
    {
        _logger.LogError(ex, "Error inesperado al {Accion} en mapa de salon.", accion);
        return new JsonResult(new { exito = false, error = "Ocurrio un error interno." }) { StatusCode = 500 };
    }

    private static readonly Guid ZonaSalonPrincipalSugeridaId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    private async Task CargarDatosAsync()
    {
        var zonas = (await _zonasServicio.ListarActivasAsync())
            .OrderBy(z => z.Orden)
            .ThenBy(z => z.Nombre)
            .ToList();
        var mesas = (await _mesasServicio.ListarMesasAsync())
            .OrderBy(m => m.Numero)
            .ToList();

        var usaZonaSugerida = zonas.Count == 0 && mesas.Count > 0;
        if (usaZonaSugerida)
        {
            zonas.Add(new ZonaSalonDto
            {
                Id = ZonaSalonPrincipalSugeridaId,
                Nombre = "Salón principal",
                Orden = 0,
                Activa = true
            });
        }

        var zonaIds = zonas.Select(z => z.Id).ToHashSet();
        var zonaPorDefectoId = zonas.FirstOrDefault()?.Id;
        var mesasMapa = new List<MesaMapaItemVm>(mesas.Count);
        var pendientesUbicacion = 0;

        for (var i = 0; i < mesas.Count; i++)
        {
            var mesa = mesas[i];
            var tieneUbicacionValida = TieneUbicacionValida(mesa, zonaIds);
            if (!tieneUbicacionValida)
            {
                pendientesUbicacion++;
            }

            var (posicionX, posicionY) = tieneUbicacionValida
                ? (mesa.PosicionX!.Value, mesa.PosicionY!.Value)
                : CalcularPosicionSugerida(i);

            mesasMapa.Add(new MesaMapaItemVm
            {
                Id = mesa.Id,
                Numero = mesa.Numero,
                Capacidad = mesa.Capacidad,
                Estado = mesa.Estado,
                Activa = mesa.Activa,
                PosicionX = posicionX,
                PosicionY = posicionY,
                ZonaId = tieneUbicacionValida ? mesa.ZonaId : zonaPorDefectoId,
                Forma = string.IsNullOrWhiteSpace(mesa.Forma) ? "Redonda" : mesa.Forma,
                Rotacion = mesa.Rotacion ?? 0,
                OcupadaDesde = mesa.OcupadaDesde,
                EsUbicacionSugerida = !tieneUbicacionValida
            });
        }

        Vm.Zonas = zonas;
        Vm.Mesas = mesasMapa;
        Vm.TotalMesas = mesas.Count;
        Vm.MesasPendientesUbicacion = pendientesUbicacion;
        Vm.UsaZonaSugerida = usaZonaSugerida;
        Vm.PuedeEditar = (User.IsInRole("Administrador") || User.IsInRole("Encargado")) && !usaZonaSugerida;
    }

    private static bool TieneUbicacionValida(MesaDto mesa, HashSet<Guid> zonasActivas)
    {
        return mesa.PosicionX.HasValue
            && mesa.PosicionY.HasValue
            && mesa.ZonaId.HasValue
            && zonasActivas.Contains(mesa.ZonaId.Value);
    }

    private static (int X, int Y) CalcularPosicionSugerida(int indice)
    {
        const int columnas = 5;
        const int inicioX = 13;
        const int inicioY = 15;
        const int separacionX = 19;
        const int separacionY = 28;

        var columna = indice % columnas;
        var fila = indice / columnas;

        var x = Math.Min(88, inicioX + columna * separacionX);
        var y = Math.Min(72, inicioY + fila * separacionY);
        return (x, y);
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
