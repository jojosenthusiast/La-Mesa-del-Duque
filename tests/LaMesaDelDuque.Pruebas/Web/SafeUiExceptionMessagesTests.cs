using LaMesaDelDuque.Pruebas.Calidad;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Modelos;
using LaMesaDelDuque.Web.Pages.Operaciones.Inventario;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using DashboardPageModel = LaMesaDelDuque.Web.Pages.Admin.Dashboard.DashboardModel;
using InventarioPageModel = LaMesaDelDuque.Web.Pages.Operaciones.Inventario.IndexModel;
using MapaPageModel = LaMesaDelDuque.Web.Pages.Operaciones.Salon.MapaModel;

namespace LaMesaDelDuque.Pruebas.Web;

public class SafeUiExceptionMessagesTests
{
    [Fact]
    public async Task Dashboard_OnGetAsync_ErrorInesperado_NoExponeDetalleYLoguea()
    {
        var logger = new RecordingLogger<DashboardPageModel>();
        var page = new DashboardPageModel(
            new ThrowingMetricaServicio(new InvalidOperationException("password=secret; host=internal")),
            logger);

        await page.OnGetAsync();

        Assert.NotNull(page.ToastError);
        Assert.DoesNotContain("password=secret", page.ToastError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("host=internal", page.ToastError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dashboard", page.ToastError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception is InvalidOperationException);
    }

    [Fact]
    public async Task Inventario_OnPostCrearIngrediente_ErrorDeDominio_MuestraMensajeDeNegocio()
    {
        var logger = new RecordingLogger<InventarioPageModel>();
        var page = new InventarioPageModel(
            new ThrowingInventarioServicio(new ReglaDominioException("Proveedor no encontrado.")),
            new EmptyMermaServicio(),
            logger);

        await page.OnPostCrearIngredienteAsync("Azucar", 1m, 1m, "kg", 1m, Guid.NewGuid());

        Assert.Equal("Proveedor no encontrado.", page.ToastError);
        Assert.DoesNotContain(logger.Entries, entry => entry.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task Inventario_OnPostCrearIngrediente_ErrorInesperado_NoExponeDetalleYLoguea()
    {
        var logger = new RecordingLogger<InventarioPageModel>();
        var page = new InventarioPageModel(
            new ThrowingInventarioServicio(new InvalidOperationException("connection string leaked: password=secret")),
            new EmptyMermaServicio(),
            logger);

        await page.OnPostCrearIngredienteAsync("Azucar", 1m, 1m, "kg", 1m, null);

        Assert.NotNull(page.ToastError);
        Assert.DoesNotContain("password=secret", page.ToastError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection string", page.ToastError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("error interno", page.ToastError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception is InvalidOperationException);
    }

    [Fact]
    public async Task Inventario_OnPostRegistrarMerma_CierreNoAbierto_MuestraMensajeOperativo()
    {
        const string mensaje = "No hay cierre de d\u00eda abierto. Abr\u00ed el d\u00eda antes de registrar mermas.";
        var logger = new RecordingLogger<InventarioPageModel>();
        var page = new InventarioPageModel(
            new ThrowingInventarioServicio(new InvalidOperationException("no debe usarse")),
            new ThrowingMermaServicio(new InvalidOperationException(mensaje)),
            logger)
        {
            MermaForm = new MermaFormVm
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = 1m
            }
        };
        SetUserId(page, Guid.NewGuid());

        await page.OnPostRegistrarMermaAsync();

        Assert.Equal(mensaje, page.ToastError);
        Assert.DoesNotContain(logger.Entries, entry => entry.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task Mapa_OnPostCambiarEstado_ErrorInesperado_Retorna500GenericoYLoguea()
    {
        var logger = new RecordingLogger<MapaPageModel>();
        var page = new MapaPageModel(
            new ThrowingMapaMesasServicio(new InvalidOperationException("sql host=internal; password=secret")),
            new EmptyZonasSalonServicio(),
            logger);
        SetUserRoles(page, "Encargado");

        var result = await page.OnPostCambiarEstadoAsync(new MapaPageModel.CambiarEstadoRequest
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = "Ocupada"
        });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(500, json.StatusCode);
        var data = JsonToDict(json.Value);
        Assert.False((bool)data["exito"]!);
        var error = Assert.IsType<string>(data["error"]);
        Assert.DoesNotContain("password=secret", error, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("host=internal", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("error interno", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception is InvalidOperationException);
    }

    [Fact]
    public void SelectedPageModels_NoDebenAsignarToastErrorConExceptionMessageGenerico()
    {
        var files = new[]
        {
            "src/LaMesaDelDuque.Web/Pages/Admin/Dashboard/Dashboard.cshtml.cs",
            "src/LaMesaDelDuque.Web/Pages/Operaciones/Inventario/Index.cshtml.cs"
        };

        var violations = files
            .Select(path => Path.Combine(ProjectPaths.RepoRoot, path.Replace('/', Path.DirectorySeparatorChar)))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => new { Path = path, LineNumber = index + 1, Line = line })
                .Where(item => item.Line.Contains("ToastError = ex.Message", StringComparison.Ordinal)
                    || item.Line.Contains("{ex.Message}", StringComparison.Ordinal))
                .Select(item => $"{Path.GetRelativePath(ProjectPaths.RepoRoot, item.Path)}:{item.LineNumber}: {item.Line.Trim()}"))
            .ToList();

        Assert.True(violations.Count == 0,
            "Los errores inesperados no deben exponer Exception.Message en ToastError:" + Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    private static void SetUserId(InventarioPageModel page, Guid usuarioId)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())], "TestAuth");
        page.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static void SetUserRoles(MapaPageModel page, params string[] roles)
    {
        var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
        claims.Add(new Claim(ClaimTypes.Name, "testuser"));
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        page.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static Dictionary<string, object?> JsonToDict(object? value)
    {
        if (value is null) return [];
        return value.GetType().GetProperties()
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p.GetValue(value));
    }

    private sealed class ThrowingMetricaServicio(Exception exception) : IMetricaServicio
    {
        public Task<MetricasOperativasDto> ObtenerMetricasOperativasAsync(CancellationToken cancelacion = default)
            => Task.FromException<MetricasOperativasDto>(exception);

        public Task<List<VentaPorHoraDto>> ObtenerVentasPorHoraAsync(CancellationToken cancelacion = default)
            => Task.FromException<List<VentaPorHoraDto>>(exception);
    }

    private sealed class ThrowingInventarioServicio(Exception exception) : IInventarioServicio
    {
        public Task<List<IngredienteDto>> ListarIngredientesAsync(CancellationToken ct = default)
            => Task.FromResult(new List<IngredienteDto>());

        public Task<IngredienteDto> CrearIngredienteAsync(GuardarIngredienteRequest req, CancellationToken ct = default)
            => Task.FromException<IngredienteDto>(exception);

        public Task<IngredienteDto> ActualizarIngredienteAsync(Guid id, GuardarIngredienteRequest req, CancellationToken ct = default)
            => Task.FromException<IngredienteDto>(exception);

        public Task AjustarStockAsync(Guid id, decimal nuevoStock, CancellationToken ct = default)
            => Task.FromException(exception);

        public Task ToggleIngredienteActivoAsync(Guid id, CancellationToken ct = default)
            => Task.FromException(exception);

        public Task<List<ProveedorDto>> ListarProveedoresAsync(CancellationToken ct = default)
            => Task.FromResult(new List<ProveedorDto>());

        public Task<ProveedorDetalleDto> ObtenerProveedorAsync(Guid id, CancellationToken ct = default)
            => Task.FromException<ProveedorDetalleDto>(exception);

        public Task<ProveedorDto> CrearProveedorAsync(GuardarProveedorRequest req, CancellationToken ct = default)
            => Task.FromException<ProveedorDto>(exception);

        public Task<ProveedorDto> ActualizarProveedorAsync(Guid id, GuardarProveedorRequest req, CancellationToken ct = default)
            => Task.FromException<ProveedorDto>(exception);

        public Task ToggleProveedorActivoAsync(Guid id, CancellationToken ct = default)
            => Task.FromException(exception);
    }

    private sealed class ThrowingMermaServicio(Exception exception) : IMermaServicio
    {
        public Task<MermaDiariaDto> RegistrarMermaAsync(RegistrarMermaRequest req, Guid usuarioId, CancellationToken ct = default)
            => Task.FromException<MermaDiariaDto>(exception);

        public Task<List<MermaDiariaDto>> ObtenerMermasDelDiaAsync(CancellationToken ct = default)
            => Task.FromResult(new List<MermaDiariaDto>());
    }

    private sealed class EmptyMermaServicio : IMermaServicio
    {
        public Task<MermaDiariaDto> RegistrarMermaAsync(RegistrarMermaRequest req, Guid usuarioId, CancellationToken ct = default)
            => Task.FromResult(new MermaDiariaDto());

        public Task<List<MermaDiariaDto>> ObtenerMermasDelDiaAsync(CancellationToken ct = default)
            => Task.FromResult(new List<MermaDiariaDto>());
    }

    private sealed class ThrowingMapaMesasServicio(Exception exception) : IMesasServicio
    {
        public Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default)
            => Task.FromResult(new List<MesaDto>());

        public Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default)
            => Task.FromResult<MesaDto?>(null);

        public Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default)
            => Task.FromResult(new MesaDto { Id = Guid.NewGuid(), Numero = numero, Capacidad = capacidad });

        public Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default)
            => Task.FromResult(new MesaDto { Id = mesaId, Numero = numero, Capacidad = capacidad });

        public Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default)
            => Task.FromException(exception);

        public Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default)
            => Task.CompletedTask;

        public Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default)
            => Task.FromException<MesaDto>(exception);

        public Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default)
            => Task.FromResult(new MesaDto { Id = mesaId });
    }

    private sealed class EmptyZonasSalonServicio : IZonasSalonServicio
    {
        public Task<List<ZonaSalonDto>> ListarActivasAsync(CancellationToken cancelacion = default)
            => Task.FromResult(new List<ZonaSalonDto>());

        public Task<List<ZonaSalonDto>> ListarTodasAsync(CancellationToken cancelacion = default)
            => Task.FromResult(new List<ZonaSalonDto>());

        public Task<ZonaSalonDto> CrearAsync(string nombre, int orden, CancellationToken cancelacion = default)
            => Task.FromResult(new ZonaSalonDto { Id = Guid.NewGuid(), Nombre = nombre, Orden = orden, Activa = true });

        public Task<ZonaSalonDto> ActualizarAsync(Guid id, string nombre, int orden, CancellationToken cancelacion = default)
            => Task.FromResult(new ZonaSalonDto { Id = id, Nombre = nombre, Orden = orden, Activa = true });

        public Task DesactivarAsync(Guid id, CancellationToken cancelacion = default)
            => Task.CompletedTask;

        public Task ActivarAsync(Guid id, CancellationToken cancelacion = default)
            => Task.CompletedTask;
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, eventId, formatter(state, exception), exception));
        }
    }

    private sealed record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
