using System.Text;
using AuditoriaEntidad = LaMesaDelDuque.Dominio.Entidades.Auditoria;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Web.Pages.Admin.Auditoria;

[Authorize(Roles = "Administrador,Gerente")]
public class IndexModel : PageModel
{
    private const int TamañoPagina = 50;

    private readonly LaMesaDelDuqueDbContext _db;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(LaMesaDelDuqueDbContext db, ILogger<IndexModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] public DateTime? FechaDesde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FechaHasta { get; set; }
    [BindProperty(SupportsGet = true)] public string? Usuario { get; set; }
    [BindProperty(SupportsGet = true)] public string? Accion { get; set; }
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public List<AuditoriaVm> Registros { get; set; } = [];
    public int TotalRegistros { get; set; }
    public int TotalPaginas => (int)Math.Ceiling(TotalRegistros / (double)TamañoPagina);

    [TempData] public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Auditoría";
        ViewData["ActiveTab"] = "Auditoría";

        await CargarAsync();
    }

    public async Task<IActionResult> OnGetExportarCsvAsync()
    {
        try
        {
            var query = ConstruirQuery();
            var registros = await query
                .OrderByDescending(a => a.Fecha)
                .Take(5000)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,Usuario,Accion,Tabla,RegistroId,DatosAnteriores,DatosNuevos,IP");

            foreach (var r in registros)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCsv(r.Fecha.ToString("yyyy-MM-dd HH:mm:ss")),
                    EscapeCsv(r.Usuario?.NombreCompleto ?? r.UsuarioId.ToString()),
                    EscapeCsv(r.Accion),
                    EscapeCsv(r.TablaAfectada),
                    EscapeCsv(r.RegistroId.ToString()),
                    EscapeCsv(r.DatosAnteriores ?? ""),
                    EscapeCsv(r.DatosNuevos ?? ""),
                    EscapeCsv(r.IpAddress ?? "")));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"auditoria_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar auditoría CSV");
            ToastError = "Error al generar el CSV.";
            return RedirectToPage();
        }
    }

    private async Task CargarAsync()
    {
        if (Pagina < 1) Pagina = 1;

        var query = ConstruirQuery();
        TotalRegistros = await query.CountAsync();

        Registros = await query
            .OrderByDescending(a => a.Fecha)
            .Skip((Pagina - 1) * TamañoPagina)
            .Take(TamañoPagina)
            .Select(a => new AuditoriaVm
            {
                Id = a.Id,
                Fecha = a.Fecha,
                UsuarioNombre = a.Usuario != null ? a.Usuario.NombreCompleto : a.UsuarioId.ToString(),
                Accion = a.Accion,
                TablaAfectada = a.TablaAfectada,
                RegistroId = a.RegistroId,
                DatosAnteriores = a.DatosAnteriores,
                DatosNuevos = a.DatosNuevos,
                IpAddress = a.IpAddress
            })
            .ToListAsync();
    }

    private IQueryable<AuditoriaEntidad> ConstruirQuery()
    {
        var query = _db.Set<AuditoriaEntidad>()
            .Include(a => a.Usuario)
            .AsQueryable();

        if (FechaDesde.HasValue)
            query = query.Where(a => a.Fecha >= FechaDesde.Value.ToUniversalTime());

        if (FechaHasta.HasValue)
            query = query.Where(a => a.Fecha <= FechaHasta.Value.AddDays(1).ToUniversalTime());

        if (!string.IsNullOrWhiteSpace(Usuario))
        {
            var u = Usuario.ToLower();
            query = query.Where(a => a.Usuario != null &&
                a.Usuario.NombreCompleto.ToLower().Contains(u));
        }

        if (!string.IsNullOrWhiteSpace(Accion))
        {
            var acc = Accion.ToUpper();
            query = query.Where(a => a.Accion.ToUpper().Contains(acc));
        }

        return query;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    public class AuditoriaVm
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string TablaAfectada { get; set; } = string.Empty;
        public Guid RegistroId { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
        public string? IpAddress { get; set; }
    }
}
