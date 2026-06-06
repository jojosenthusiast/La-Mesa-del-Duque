using System.Text.Json;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Web.Pages.Admin.Configuracion;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly LaMesaDelDuqueDbContext _db;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(LaMesaDelDuqueDbContext db, ILogger<IndexModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public ConfigFormVm Form { get; set; } = new();

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Configuración";
        ViewData["ActiveTab"] = "Configuración";

        var config = await _db.Set<RestauranteConfig>().FirstOrDefaultAsync();
        if (config is null) return;

        Form.Nombre = config.Nombre;
        Form.Direccion = config.Direccion;
        Form.Telefono = config.Telefono ?? string.Empty;
        Form.HorarioApertura = config.HorarioApertura.ToString("HH:mm");
        Form.HorarioCierre = config.HorarioCierre.ToString("HH:mm");
        Form.PeriodoGraciaMinutos = config.PeriodoGraciaMinutos;

        if (!string.IsNullOrWhiteSpace(config.DatosTicketJson))
        {
            try
            {
                var extra = JsonSerializer.Deserialize<DatosTicketExtra>(config.DatosTicketJson);
                if (extra is not null)
                {
                    Form.Nit = extra.Nit ?? string.Empty;
                    Form.IvaPorcentaje = extra.Iva;
                    Form.PropinaPorcentaje = extra.PropinaPct;
                    Form.MensajeTicket = extra.MensajeTicket ?? string.Empty;
                }
            }
            catch { }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["Title"] = "Configuración";
        ViewData["ActiveTab"] = "Configuración";

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var config = await _db.Set<RestauranteConfig>().FirstOrDefaultAsync();
            if (config is null)
            {
                ToastError = "No se encontró la configuración del restaurante.";
                return Page();
            }

            config.ActualizarDatos(Form.Nombre, Form.Direccion, config.CantidadMesas);
            config.EstablecerGracia(Form.PeriodoGraciaMinutos);

            if (!string.IsNullOrWhiteSpace(Form.Telefono))
                config.ActualizarTelefono(Form.Telefono);

            if (TimeOnly.TryParse(Form.HorarioApertura, out var apertura) &&
                TimeOnly.TryParse(Form.HorarioCierre, out var cierre))
            {
                config.ActualizarHorario(apertura, cierre);
            }

            var extra = new DatosTicketExtra
            {
                Nit = Form.Nit,
                Iva = Form.IvaPorcentaje,
                PropinaPct = Form.PropinaPorcentaje,
                MensajeTicket = Form.MensajeTicket
            };
            config.ActualizarDatosTicket(JsonSerializer.Serialize(extra));

            await _db.SaveChangesAsync();
            ToastSuccess = "Configuración guardada correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración del restaurante");
            ToastError = "Ocurrió un error al guardar la configuración.";
        }

        return RedirectToPage();
    }

    public class ConfigFormVm
    {
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string HorarioApertura { get; set; } = "11:00";
        public string HorarioCierre { get; set; } = "23:00";
        public int PeriodoGraciaMinutos { get; set; } = 5;
        public string Nit { get; set; } = string.Empty;
        public int IvaPorcentaje { get; set; } = 13;
        public int PropinaPorcentaje { get; set; } = 10;
        public string MensajeTicket { get; set; } = string.Empty;
    }

    public class DatosTicketExtra
    {
        public string? Nit { get; set; }
        public int Iva { get; set; } = 13;
        public int PropinaPct { get; set; } = 10;
        public string? MensajeTicket { get; set; }
    }
}
