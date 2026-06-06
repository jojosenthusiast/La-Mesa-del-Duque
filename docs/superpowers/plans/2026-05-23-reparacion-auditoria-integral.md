# Reparación Auditoría Integral Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reparar los bloqueos detectados por la auditoría integral y dejar el sistema verificable localmente, con persistencia consistente, UI legible, workflows alineados a SDR-101..SDR-125 y evidencia final en `lastobservations.md`.

**Architecture:** La reparación se ejecuta de abajo hacia arriba: entorno/persistencia primero, dominio/aplicación después, UI/Razor al final, y recién entonces auditoría browser. NO se debe “maquillar” la interfaz antes de arreglar arranque, migraciones y reglas de negocio; eso sería pintar una pared con grietas estructurales.

**Tech Stack:** ASP.NET Core 8 Razor Pages, EF Core 8, SQLite para desarrollo local reproducible, PostgreSQL/Supabase para entorno remoto, xUnit, SignalR, Bootstrap 5, PowerShell.

---

## Contexto verificado de auditoría

La última auditoría encontró:

- `dotnet test LaMesaDelDuque.slnx --no-restore` pasa con `283/283` tests.
- El arranque local falla por conexión Supabase/SSL desde `appsettings.Development.json`.
- `appsettings.Development.json` contiene una cadena Supabase con credenciales.
- `InyeccionInfraestructura.cs` imprime la cadena de conexión en consola.
- El `ModelSnapshot` contiene campos nuevos de Merma/Cierre que NO tienen migración formal real.
- Persisten textos corruptos tipo `DÃ­a`, `MÃ©todo`, `contraseÃ±a`.
- Hay handlers que devuelven `BadRequest(ex.Message)`.
- La mesa se libera al pagar, pero SDR-116 exige liberarla al despachar.
- Los pagos pueden quedar con `Guid.Empty` como usuario.
- Home no expone todos los módulos que sí aparecen en `_Layout`.

## File Structure

### Configuración y arranque

- Modify: `src/LaMesaDelDuque.Web/appsettings.Development.json`
- Modify: `src/LaMesaDelDuque.Web/appsettings.json`
- Modify: `src/LaMesaDelDuque.Infraestructura/InyeccionInfraestructura.cs`
- Create: `docs/runbook-local.md`

### Persistencia y migraciones

- Modify/Create: `src/LaMesaDelDuque.Infraestructura/Migrations/*.cs`
- Verify: `src/LaMesaDelDuque.Infraestructura/Migrations/LaMesaDelDuqueDbContextModelSnapshot.cs`
- Test: `tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaModelTests.cs`

### Dominio y aplicación

- Modify: `src/LaMesaDelDuque.Dominio/Entidades/Pago.cs`
- Modify: `src/LaMesaDelDuque.Dominio/Entidades/CierreDia.cs`
- Modify: `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs`
- Modify: `src/LaMesaDelDuque.Dominio/Enums/EstadoPedido.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs`
- Create/Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/DespachoServicio.cs`
- Create/Modify: `src/LaMesaDelDuque.Aplicacion/Interfaces/IDespachoServicio.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/InyeccionAplicacion.cs`

### UI / Razor

- Modify: `src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Cierre/Index.cshtml`
- Modify: `src/LaMesaDelDuque.Web/Pages/Pedidos/Index.cshtml.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Cocina/KDS.cshtml.cs`
- Modify: textos corruptos en `src/LaMesaDelDuque.Web/Pages/**/*.cshtml*`

### Auditoría y documentación

- Create: `docs/auditoria-browser-checklist.md`
- Modify: `C:\Users\frenzied\Desktop\SoftwareGestionCalidad\lastobservations.md`

---

## Task 1: Hacer el arranque local reproducible y eliminar secretos de Development

**Files:**

- Modify: `src/LaMesaDelDuque.Web/appsettings.Development.json`
- Modify: `src/LaMesaDelDuque.Infraestructura/InyeccionInfraestructura.cs`
- Create: `docs/runbook-local.md`
- Test: `tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaConfigTests.cs`

- [ ] **Step 1: Crear test de fallback SQLite en Development**

Crear `tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaConfigTests.cs`:

```csharp
using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public sealed class PersistenciaConfigTests
{
    [Fact]
    public void AgregarPersistencia_DevelopmentSinConnectionString_UsaSqliteLocal()
    {
        var servicios = new ServiceCollection();

        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ""
            })
            .Build();

        var ambiente = new FakeHostEnvironment { EnvironmentName = Environments.Development };

        servicios.AgregarPersistencia(configuracion, ambiente);

        using var proveedor = servicios.BuildServiceProvider();
        using var scope = proveedor.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<LaMesaDelDuqueDbContext>();

        Assert.True(contexto.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LaMesaDelDuque.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
```

- [ ] **Step 2: Ejecutar test y confirmar fallo**

Run:

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~PersistenciaConfigTests
```

Expected:

```text
Failed! - Failed: 1
```

Debe fallar porque la configuración actual no garantiza SQLite local si Development trae una cadena remota.

- [ ] **Step 3: Limpiar `appsettings.Development.json`**

Reemplazar la cadena Supabase por una cadena vacía o SQLite explícita:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

- [ ] **Step 4: Eliminar logging de secretos y fijar fallback**

En `src/LaMesaDelDuque.Infraestructura/InyeccionInfraestructura.cs`, dejar la decisión así:

```csharp
var connectionString = configuracion.GetConnectionString("DefaultConnection");

if (ambiente.IsDevelopment() && string.IsNullOrWhiteSpace(connectionString))
{
    servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
        opciones.UseSqlite("Data Source=la-mesa-del-duque-dev.db"));
}
else
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("La cadena de conexión DefaultConnection no está configurada.");
    }

    servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
        opciones.UseNpgsql(connectionString));
}
```

Eliminar cualquier línea equivalente a:

```csharp
Console.WriteLine($"[DBG] ... cs='{connectionString}' ...");
```

- [ ] **Step 5: Documentar ejecución local**

Crear `docs/runbook-local.md`:

```markdown
# Runbook local - La Mesa del Duque

## Objetivo

Ejecutar la aplicación localmente sin depender de Supabase ni exponer credenciales.

## Desarrollo local

1. Verificar SDK:
   `dotnet --version`

2. Restaurar:
   `dotnet restore LaMesaDelDuque.slnx`

3. Ejecutar tests:
   `dotnet test LaMesaDelDuque.slnx --no-restore`

4. Levantar aplicación:
   `dotnet run --project src/LaMesaDelDuque.Web/LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile`

## Base local

En Development, si `ConnectionStrings:DefaultConnection` está vacío, el sistema usa:

`Data Source=la-mesa-del-duque-dev.db`

## Producción / remoto

La cadena PostgreSQL debe venir desde secretos de entorno o configuración segura, nunca desde `appsettings.Development.json`.
```

- [ ] **Step 6: Verificar test y arranque**

Run:

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~PersistenciaConfigTests
dotnet run --project src\LaMesaDelDuque.Web\LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile
```

Expected:

```text
Passed! - Failed: 0
Now listening on: http://localhost:5103
```

- [ ] **Step 7: Commit**

```powershell
git add src/LaMesaDelDuque.Web/appsettings.Development.json src/LaMesaDelDuque.Infraestructura/InyeccionInfraestructura.cs docs/runbook-local.md tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaConfigTests.cs
git commit -m "fix: make local development database reproducible"
```

---

## Task 2: Crear migración formal para Merma/Cierre/Pedido

**Files:**

- Create: `tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaModelTests.cs`
- Create: `src/LaMesaDelDuque.Infraestructura/Migrations/*Audit4CamposCierreMermaPedido*.cs`
- Modify: `src/LaMesaDelDuque.Infraestructura/Migrations/LaMesaDelDuqueDbContextModelSnapshot.cs`

- [ ] **Step 1: Escribir test de modelo EF**

Crear `tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaModelTests.cs`:

```csharp
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public sealed class PersistenciaModelTests
{
    [Fact]
    public void ModeloEf_ContieneCamposDeMermaYCierreAudit4()
    {
        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var contexto = new LaMesaDelDuqueDbContext(opciones);
        var modelo = contexto.Model;

        var merma = modelo.FindEntityType(typeof(MermaDiaria));
        Assert.NotNull(merma);
        Assert.NotNull(merma!.FindProperty("Tipo"));
        Assert.NotNull(merma.FindProperty("Lote"));

        var cierre = modelo.FindEntityType(typeof(CierreDia));
        Assert.NotNull(cierre);
        Assert.NotNull(cierre!.FindProperty("EfectivoReal"));
        Assert.NotNull(cierre.FindProperty("TarjetaReal"));
        Assert.NotNull(cierre.FindProperty("DiferenciaEfectivo"));
        Assert.NotNull(cierre.FindProperty("DiferenciaTarjeta"));
        Assert.NotNull(cierre.FindProperty("EsCerrado"));
        Assert.NotNull(cierre.FindProperty("CerradoEn"));
    }
}
```

- [ ] **Step 2: Ejecutar test**

Run:

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~PersistenciaModelTests
```

Expected:

```text
Passed! - Failed: 0
```

Si falla, primero configurar entidades en `LaMesaDelDuqueDbContext` antes de generar migración.

- [ ] **Step 3: Verificar que no exista migración real**

Run:

```powershell
Select-String -Path src\LaMesaDelDuque.Infraestructura\Migrations\*.cs -Pattern "EfectivoReal|TarjetaReal|DiferenciaEfectivo|DiferenciaTarjeta|EsCerrado|CerradoEn|Lote|Tipo" | Select-Object Path, LineNumber, Line
```

Expected actual antes de reparar:

```text
Solo debe aparecer ModelSnapshot; si no aparece en una clase Migration, falta migración formal.
```

- [ ] **Step 4: Generar migración formal**

Run:

```powershell
dotnet ef migrations add Audit4CamposCierreMermaPedido --project src\LaMesaDelDuque.Infraestructura --startup-project src\LaMesaDelDuque.Web --output-dir Migrations
```

Expected:

```text
Done. To undo this action, use 'ef migrations remove'
```

Si la migración sale vacía porque el snapshot fue editado manualmente, NO aceptar esa basura. Restaurar el snapshot al último estado migrado real y volver a generar:

```powershell
git checkout HEAD~1 -- src\LaMesaDelDuque.Infraestructura\Migrations\LaMesaDelDuqueDbContextModelSnapshot.cs
dotnet ef migrations add Audit4CamposCierreMermaPedido --project src\LaMesaDelDuque.Infraestructura --startup-project src\LaMesaDelDuque.Web --output-dir Migrations
```

- [ ] **Step 5: Inspeccionar migración**

La migración debe tener `AddColumn` o equivalente para:

```csharp
Tipo
Lote
EfectivoReal
TarjetaReal
DiferenciaEfectivo
DiferenciaTarjeta
EsCerrado
CerradoEn
```

- [ ] **Step 6: Validar migración en SQLite local**

Eliminar base local si existe y arrancar:

```powershell
Remove-Item -LiteralPath .\la-mesa-del-duque-dev.db -ErrorAction SilentlyContinue
dotnet run --project src\LaMesaDelDuque.Web\LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile
```

Expected:

```text
Now listening on: http://localhost:5103
```

- [ ] **Step 7: Commit**

```powershell
git add src/LaMesaDelDuque.Infraestructura/Migrations tests/LaMesaDelDuque.Pruebas/Infraestructura/PersistenciaModelTests.cs
git commit -m "fix: add formal migration for audit4 persistence changes"
```

---

## Task 3: Corregir mojibake y bloquear regresiones de encoding

**Files:**

- Create: `tests/LaMesaDelDuque.Pruebas/Web/EncodingTests.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/**/*.cshtml`
- Modify: `src/LaMesaDelDuque.Web/Pages/**/*.cs`
- Modify: `src/LaMesaDelDuque.Web/Program.cs`

- [ ] **Step 1: Crear test anti-mojibake**

Crear `tests/LaMesaDelDuque.Pruebas/Web/EncodingTests.cs`:

```csharp
namespace LaMesaDelDuque.Pruebas.Web;

public sealed class EncodingTests
{
    [Fact]
    public void FuentesWeb_NoContienenMojibakeComun()
    {
        var raiz = EncontrarRaizRepositorio();
        var archivos = Directory.EnumerateFiles(Path.Combine(raiz, "src", "LaMesaDelDuque.Web"), "*.*", SearchOption.AllDirectories)
            .Where(ruta => ruta.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        || ruta.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                        || ruta.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var patrones = new[] { "Ã", "Â¿", "Â¡", "â€”", "â€œ", "â€", "â”" };
        var ofensores = archivos
            .SelectMany(archivo =>
            {
                var contenido = File.ReadAllText(archivo);
                return patrones
                    .Where(contenido.Contains)
                    .Select(patron => $"{Path.GetRelativePath(raiz, archivo)} contiene '{patron}'");
            })
            .ToArray();

        Assert.True(ofensores.Length == 0, string.Join(Environment.NewLine, ofensores));
    }

    private static string EncontrarRaizRepositorio()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LaMesaDelDuque.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("No se encontró LaMesaDelDuque.slnx.");
    }
}
```

- [ ] **Step 2: Ejecutar test y listar ofensores**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~EncodingTests
```

Expected:

```text
Failed! - el mensaje debe listar archivos con mojibake.
```

- [ ] **Step 3: Reemplazar textos corruptos**

Aplicar reemplazos verificados:

```text
DÃ­a -> Día
dÃ­a -> día
AbrÃ­ -> Abrí
MÃ©todo -> Método
mÃ©todo -> método
CatÃ¡logo -> Catálogo
GestiÃ³n -> Gestión
MarÃ­a -> María
contraseÃ±a -> contraseña
ContraseÃ±a -> Contraseña
NÃºmero -> Número
DirecciÃ³n -> Dirección
TelÃ©fono -> Teléfono
CategorÃ­a -> Categoría
MenÃº -> Menú
```

Cuando aparezcan caracteres de caja corruptos (`â”`), reemplazarlos por comentarios simples ASCII o por texto UTF-8 válido.

- [ ] **Step 4: Ejecutar test**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~EncodingTests
```

Expected:

```text
Passed! - Failed: 0
```

- [ ] **Step 5: Commit**

```powershell
git add src/LaMesaDelDuque.Web tests/LaMesaDelDuque.Pruebas/Web/EncodingTests.cs
git commit -m "fix: normalize Spanish text encoding"
```

---

## Task 4: Evitar filtrado de errores internos en JSON/Razor handlers

**Files:**

- Modify: `src/LaMesaDelDuque.Web/Pages/Pedidos/Index.cshtml.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Cocina/KDS.cshtml.cs`
- Create: `tests/LaMesaDelDuque.Pruebas/Web/ErrorHandlingTests.cs`

- [ ] **Step 1: Crear test estático anti-leak**

Crear `tests/LaMesaDelDuque.Pruebas/Web/ErrorHandlingTests.cs`:

```csharp
using System.Text.RegularExpressions;

namespace LaMesaDelDuque.Pruebas.Web;

public sealed class ErrorHandlingTests
{
    [Fact]
    public void RazorPageHandlers_NoDevuelvenExceptionMessageCrudo()
    {
        var raiz = EncontrarRaizRepositorio();
        var archivos = Directory.EnumerateFiles(Path.Combine(raiz, "src", "LaMesaDelDuque.Web", "Pages"), "*.cs", SearchOption.AllDirectories);

        var regex = new Regex(@"BadRequest\s*\(\s*ex\.Message\s*\)", RegexOptions.Compiled);
        var ofensores = archivos
            .Where(archivo => regex.IsMatch(File.ReadAllText(archivo)))
            .Select(archivo => Path.GetRelativePath(raiz, archivo))
            .ToArray();

        Assert.True(ofensores.Length == 0, string.Join(Environment.NewLine, ofensores));
    }

    private static string EncontrarRaizRepositorio()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LaMesaDelDuque.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("No se encontró LaMesaDelDuque.slnx.");
    }
}
```

- [ ] **Step 2: Ejecutar test y confirmar fallo**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~ErrorHandlingTests
```

Expected:

```text
Failed! - lista handlers que devuelven ex.Message crudo.
```

- [ ] **Step 3: Agregar helper de error seguro**

En cada PageModel afectado, reemplazar catches genéricos por este patrón:

```csharp
private static object ErrorSeguro(Exception ex)
{
    var mensaje = ex switch
    {
        ArgumentException => ex.Message,
        InvalidOperationException => ex.Message,
        _ => "Ocurrió un error interno al procesar la solicitud."
    };

    return new { error = mensaje };
}
```

Y reemplazar:

```csharp
return BadRequest(ex.Message);
```

por:

```csharp
return BadRequest(ErrorSeguro(ex));
```

Si existe una excepción de dominio propia, incluirla explícitamente:

```csharp
ReglaDominioException => ex.Message,
```

- [ ] **Step 4: Ejecutar tests**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~ErrorHandlingTests
dotnet test LaMesaDelDuque.slnx --no-restore
```

Expected:

```text
Passed! - Failed: 0
```

- [ ] **Step 5: Commit**

```powershell
git add src/LaMesaDelDuque.Web/Pages/Pedidos/Index.cshtml.cs src/LaMesaDelDuque.Web/Pages/Cocina/KDS.cshtml.cs tests/LaMesaDelDuque.Pruebas/Web/ErrorHandlingTests.cs
git commit -m "fix: avoid leaking internal exception messages"
```

---

## Task 5: Corregir workflow de mesas: liberar al despachar, no al pagar

**Files:**

- Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs`
- Create/Modify: `src/LaMesaDelDuque.Aplicacion/Interfaces/IDespachoServicio.cs`
- Create/Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/DespachoServicio.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/InyeccionAplicacion.cs`
- Modify: `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs`
- Modify: `src/LaMesaDelDuque.Dominio/Enums/EstadoPedido.cs`
- Test: `tests/LaMesaDelDuque.Pruebas/Aplicacion/DespachoMesaTests.cs`

- [ ] **Step 1: Crear test que demuestre que pagar NO libera mesa**

Crear `tests/LaMesaDelDuque.Pruebas/Aplicacion/DespachoMesaTests.cs` con un caso explícito:

```csharp
namespace LaMesaDelDuque.Pruebas.Aplicacion;

public sealed class DespachoMesaTests
{
    [Fact]
    public async Task PagarPedido_ComerAqui_NoLiberaMesaHastaDespacho()
    {
        // Arrange: crear mesa disponible, pedido ComerAquí asignado y pagable.
        // Usar builders/fakes existentes del proyecto; si no existen, crear fakes mínimos en este archivo.

        // Act: ejecutar PagarPedidoAsync.

        // Assert: la mesa sigue Ocupada después del pago.
        // Assert: el pedido queda pagado/enviado a cocina según flujo actual.
    }
}
```

Este test debe completarse usando las fábricas/repositorios ya existentes en la suite. NO dejar comentarios dentro del código final.

- [ ] **Step 2: Ejecutar test y confirmar fallo**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~DespachoMesaTests
```

Expected:

```text
Failed! - la mesa se libera al pagar.
```

- [ ] **Step 3: Eliminar liberación de mesa del pago**

En `PedidosServicio.PagarPedidoAsync` y `PagarCuentaAsync`, eliminar llamadas equivalentes a:

```csharp
await LiberarMesaSiCorrespondeAsync(pedido, cancelacion);
```

La regla correcta:

```text
Pago confirma venta y envía a cocina.
Despacho entrega al cliente y libera mesa.
```

- [ ] **Step 4: Agregar servicio de despacho si no existe**

Crear `src/LaMesaDelDuque.Aplicacion/Interfaces/IDespachoServicio.cs`:

```csharp
namespace LaMesaDelDuque.Aplicacion.Interfaces;

public interface IDespachoServicio
{
    Task DespacharPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
}
```

Crear `src/LaMesaDelDuque.Aplicacion/Servicios/DespachoServicio.cs`:

```csharp
using LaMesaDelDuque.Aplicacion.Interfaces;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public sealed class DespachoServicio : IDespachoServicio
{
    private readonly IPedidosRepositorio _pedidos;
    private readonly IMesasRepositorio _mesas;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public DespachoServicio(
        IPedidosRepositorio pedidos,
        IMesasRepositorio mesas,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _pedidos = pedidos;
        _mesas = mesas;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task DespacharPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, cancelacion)
            ?? throw new InvalidOperationException("Pedido no encontrado.");

        pedido.MarcarDespachado();

        if (pedido.MesaId.HasValue)
        {
            var mesa = await _mesas.ObtenerPorIdAsync(pedido.MesaId.Value, cancelacion)
                ?? throw new InvalidOperationException("Mesa no encontrada.");

            mesa.Liberar();
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancelacion);
    }
}
```

Ajustar nombres de repositorios/métodos si el proyecto ya usa otros nombres, manteniendo la intención exacta.

- [ ] **Step 5: Agregar transición de dominio**

En `Pedido.cs`:

```csharp
public void MarcarDespachado()
{
    if (Estado != EstadoPedido.Listo)
    {
        throw new InvalidOperationException("Solo un pedido listo puede ser despachado.");
    }

    Estado = EstadoPedido.Despachado;
    ActualizadoEn = DateTime.UtcNow;
}
```

En `EstadoPedido.cs`, asegurar:

```csharp
Despachado = 5
```

Usar el valor que preserve compatibilidad con los estados existentes.

- [ ] **Step 6: Registrar servicio**

En `InyeccionAplicacion.cs`:

```csharp
servicios.AddScoped<IDespachoServicio, DespachoServicio>();
```

- [ ] **Step 7: Crear test de despacho que libera mesa**

En `DespachoMesaTests.cs`, agregar:

```csharp
[Fact]
public async Task DespacharPedido_ComerAqui_LiberaMesa()
{
    // Arrange: mesa ocupada + pedido en estado Listo con MesaId.
    // Act: DespacharPedidoAsync.
    // Assert: pedido Despachado y mesa Disponible.
}
```

Completar con builders/fakes reales de la suite.

- [ ] **Step 8: Ejecutar tests**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~DespachoMesaTests
dotnet test LaMesaDelDuque.slnx --no-restore
```

Expected:

```text
Passed! - Failed: 0
```

- [ ] **Step 9: Commit**

```powershell
git add src/LaMesaDelDuque.Aplicacion src/LaMesaDelDuque.Dominio tests/LaMesaDelDuque.Pruebas/Aplicacion/DespachoMesaTests.cs
git commit -m "fix: release tables only on dispatch"
```

---

## Task 6: Bloquear pagos sin usuario trazable

**Files:**

- Modify: `src/LaMesaDelDuque.Dominio/Entidades/Pago.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs`
- Test: `tests/LaMesaDelDuque.Pruebas/Dominio/PagoTests.cs`

- [ ] **Step 1: Crear test de dominio**

En `tests/LaMesaDelDuque.Pruebas/Dominio/PagoTests.cs`:

```csharp
[Fact]
public void CrearPago_UsuarioVacio_DebeLanzarExcepcion()
{
    var excepcion = Assert.Throws<ArgumentException>(() =>
        new Pago(
            pedidoId: Guid.NewGuid(),
            monto: 1000m,
            metodo: MetodoPago.Efectivo,
            usuarioId: Guid.Empty,
            referenciaPos: null));

    Assert.Contains("usuario", excepcion.Message, StringComparison.OrdinalIgnoreCase);
}
```

Ajustar firma exacta del constructor si difiere, pero NO cambiar la regla: `Guid.Empty` es inválido.

- [ ] **Step 2: Ejecutar test y confirmar fallo**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~CrearPago_UsuarioVacio_DebeLanzarExcepcion
```

Expected:

```text
Failed!
```

- [ ] **Step 3: Validar en `Pago`**

En el constructor de `Pago`:

```csharp
if (usuarioId == Guid.Empty)
{
    throw new ArgumentException("El usuario del pago es obligatorio para auditoría.", nameof(usuarioId));
}
```

- [ ] **Step 4: Validar en aplicación**

En `PedidosServicio`, si `ObtenerUsuarioIdActual()` retorna vacío:

```csharp
var usuarioId = ObtenerUsuarioIdActual();
if (usuarioId == Guid.Empty)
{
    throw new InvalidOperationException("No se pudo identificar el usuario actual para registrar el pago.");
}
```

- [ ] **Step 5: Ajustar tests existentes**

Todo test que cree pagos debe pasar un `Guid.NewGuid()` real:

```csharp
var usuarioId = Guid.NewGuid();
```

- [ ] **Step 6: Ejecutar suite**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore
```

Expected:

```text
Passed! - Failed: 0
```

- [ ] **Step 7: Commit**

```powershell
git add src/LaMesaDelDuque.Dominio/Entidades/Pago.cs src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs tests/LaMesaDelDuque.Pruebas/Dominio/PagoTests.cs
git commit -m "fix: require user traceability for payments"
```

---

## Task 7: Exigir observación cuando hay descuadre de cierre

**Files:**

- Modify: `src/LaMesaDelDuque.Dominio/Entidades/CierreDia.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/DTOs/CierreCajaRequest.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/CierreServicio.cs`
- Modify: `src/LaMesaDelDuque.Infraestructura/Persistencia/LaMesaDelDuqueDbContext.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Cierre/Index.cshtml`
- Modify: `src/LaMesaDelDuque.Web/Pages/Cierre/Index.cshtml.cs`
- Test: `tests/LaMesaDelDuque.Pruebas/Aplicacion/CierreServicioTests.cs`

- [ ] **Step 1: Crear test de descuadre sin observación**

En `CierreServicioTests.cs`:

```csharp
[Fact]
public async Task CerrarDia_ConDescuadreYSinObservacion_DebeRechazar()
{
    // Arrange: cierre abierto con efectivo sistema 1000 y efectivo real 900.

    // Act
    var accion = async () => await servicio.CerrarDiaAsync(new CierreCajaRequest
    {
        EfectivoReal = 900m,
        TarjetaReal = 0m,
        Observacion = ""
    });

    // Assert
    var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(accion);
    Assert.Contains("observación", excepcion.Message, StringComparison.OrdinalIgnoreCase);
}
```

Completar con helpers reales del archivo.

- [ ] **Step 2: Agregar propiedad de dominio**

En `CierreDia.cs`:

```csharp
public string? Observacion { get; private set; }
```

En método de cierre:

```csharp
public void Cerrar(decimal efectivoReal, decimal tarjetaReal, string? observacion)
{
    EfectivoReal = efectivoReal;
    TarjetaReal = tarjetaReal;
    DiferenciaEfectivo = efectivoReal - TotalEfectivo;
    DiferenciaTarjeta = tarjetaReal - TotalTarjeta;

    var hayDescuadre = DiferenciaEfectivo != 0m || DiferenciaTarjeta != 0m;
    if (hayDescuadre && string.IsNullOrWhiteSpace(observacion))
    {
        throw new InvalidOperationException("La observación es obligatoria cuando existe descuadre de caja.");
    }

    Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
    EsCerrado = true;
    CerradoEn = DateTime.UtcNow;
}
```

Usar nombres exactos ya existentes para totales del sistema.

- [ ] **Step 3: Extender request**

En `CierreCajaRequest.cs`:

```csharp
public string? Observacion { get; init; }
```

- [ ] **Step 4: Configurar EF**

En `LaMesaDelDuqueDbContext.cs`:

```csharp
builder.Entity<CierreDia>()
    .Property(c => c.Observacion)
    .HasMaxLength(500);
```

- [ ] **Step 5: Agregar campo UI**

En `Pages/Cierre/Index.cshtml`, agregar textarea:

```html
<div class="mb-3">
    <label asp-for="Input.Observacion" class="form-label">Observación del cierre</label>
    <textarea asp-for="Input.Observacion" class="form-control" rows="3" maxlength="500"></textarea>
    <div class="form-text">Obligatoria si hay diferencia entre efectivo/tarjeta real y sistema.</div>
</div>
```

En PageModel, mapear `Input.Observacion` hacia `CierreCajaRequest.Observacion`.

- [ ] **Step 6: Generar migración**

```powershell
dotnet ef migrations add CierreObservacionDescuadre --project src\LaMesaDelDuque.Infraestructura --startup-project src\LaMesaDelDuque.Web --output-dir Migrations
```

- [ ] **Step 7: Ejecutar tests**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~CierreServicioTests
dotnet test LaMesaDelDuque.slnx --no-restore
```

Expected:

```text
Passed! - Failed: 0
```

- [ ] **Step 8: Commit**

```powershell
git add src/LaMesaDelDuque.Dominio src/LaMesaDelDuque.Aplicacion src/LaMesaDelDuque.Infraestructura src/LaMesaDelDuque.Web/Pages/Cierre tests/LaMesaDelDuque.Pruebas/Aplicacion/CierreServicioTests.cs
git commit -m "feat: require closing note for cash discrepancies"
```

---

## Task 8: Alinear Home con módulos por rol

**Files:**

- Modify: `src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs`
- Test: `tests/LaMesaDelDuque.Pruebas/Web/HomeNavigationTests.cs`

- [ ] **Step 1: Crear tests de navegación por rol**

Crear `tests/LaMesaDelDuque.Pruebas/Web/HomeNavigationTests.cs`:

```csharp
namespace LaMesaDelDuque.Pruebas.Web;

public sealed class HomeNavigationTests
{
    [Theory]
    [InlineData("admin", "Productos", "Mesas", "Pedidos", "Cocina", "Usuarios", "Inventario", "Cierre")]
    [InlineData("encargado", "Productos", "Mesas", "Pedidos", "Cocina", "Inventario", "Cierre")]
    [InlineData("mesero", "Mesas", "Pedidos")]
    [InlineData("cocinero", "Cocina")]
    public void Index_ModulosPorRol_CoincidenConLayout(string rol, params string[] esperados)
    {
        // Construir ClaimsPrincipal con rol.
        // Ejecutar OnGet.
        // Assert: los módulos visibles contienen exactamente los esperados.
    }
}
```

Completar con el API real de `IndexModel`. Si `IndexModel` no permite testear sin Razor, extraer una clase interna/servicio puro `HomeModuleProvider`.

- [ ] **Step 2: Ejecutar test y confirmar fallo**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~HomeNavigationTests
```

- [ ] **Step 3: Corregir `Index.cshtml.cs`**

La matriz mínima debe quedar:

```text
admin: productos, mesas, pedidos, cocina, usuarios, inventario, cierre
encargado: productos, mesas, pedidos, cocina, inventario, cierre
mesero: mesas, pedidos
cocinero: cocina
```

No mostrar dashboard/backup/config si no están implementados. Mostrar módulos inexistentes es mentirle al usuario.

- [ ] **Step 4: Ejecutar tests**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore --filter FullyQualifiedName~HomeNavigationTests
dotnet test LaMesaDelDuque.slnx --no-restore
```

- [ ] **Step 5: Commit**

```powershell
git add src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs tests/LaMesaDelDuque.Pruebas/Web/HomeNavigationTests.cs
git commit -m "fix: align home navigation with role modules"
```

---

## Task 9: Auditoría browser real post-reparación

**Files:**

- Create: `docs/auditoria-browser-checklist.md`
- Modify: `C:\Users\frenzied\Desktop\SoftwareGestionCalidad\lastobservations.md`

- [ ] **Step 1: Crear checklist E2E**

Crear `docs/auditoria-browser-checklist.md`:

```markdown
# Checklist auditoría browser post-reparación

## Ambiente

- URL local: http://localhost:5103
- Base: SQLite local limpia o seed conocido

## Flujos obligatorios

### Login
- Login válido por rol.
- Login inválido muestra error seguro.
- Usuario sin rol no accede a módulos.

### Cajero / Mesero
- Crear pedido Para llevar.
- Crear pedido Comer aquí con mesa.
- Agregar/quitar/cambiar cantidades antes del pago.
- Pago efectivo rechaza monto menor al total.
- Pago efectivo genera ticket.
- Pago tarjeta exige referencia POS.
- Pago de pedido con mesa NO libera mesa.

### Cocina
- Pedido pagado aparece en KDS sin recargar.
- Pedido muestra número, tipo de servicio y detalle.
- Marcar Listo retira de cocina.

### Despacho
- Pedido Listo aparece en despacho.
- Despachar pedido con mesa libera mesa.

### Inventario / Merma
- Registrar merma con tipo.
- Registrar merma con lote opcional.
- Costo se calcula automáticamente.
- Merma aparece en log del día.

### Cierre
- Abrir día si no hay cierre abierto.
- Cerrar sin descuadre.
- Cerrar con descuadre exige observación.
- Totales efectivo/tarjeta usan pagos reales.

### Autorización
- Cajero no accede a Inventario/Cierre/Usuarios.
- Cocinero no accede a Pedidos/Usuarios.
- Encargado no accede a Usuarios si no corresponde.
```

- [ ] **Step 2: Levantar app**

```powershell
dotnet run --project src\LaMesaDelDuque.Web\LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile
```

Expected:

```text
Now listening on: http://localhost:5103
```

- [ ] **Step 3: Ejecutar browser audit con agent-browser**

Guardar evidencias en:

```text
C:\Users\frenzied\Desktop\SoftwareGestionCalidad\audit-output\post-repair-browser\
```

Capturas mínimas:

```text
01-login.png
02-home-admin.png
03-pos-pedido-mesa.png
04-pago-ticket.png
05-mesa-sigue-ocupada-post-pago.png
06-kds-pedido.png
07-despacho-listo.png
08-mesa-liberada-post-despacho.png
09-inventario-merma.png
10-cierre-descuadre-observacion.png
```

- [ ] **Step 4: Registrar resultado en `lastobservations.md`**

Agregar sección:

```markdown
## Auditoría browser post-reparación - 2026-05-23

### Resultado

- Estado: APROBADO / RECHAZADO
- URL: http://localhost:5103
- Evidencias: `audit-output/post-repair-browser/`

### Hallazgos

1. ...

### Decisión

...
```

- [ ] **Step 5: Commit**

```powershell
git add docs/auditoria-browser-checklist.md
git commit -m "docs: add post-repair browser audit checklist"
```

El archivo `lastobservations.md` vive en el vault, fuera del repo de app; no incluirlo en este commit salvo que sea parte del repo actual.

---

## Task 10: Puerta final de verificación

**Files:**

- Modify: `C:\Users\frenzied\Desktop\SoftwareGestionCalidad\lastobservations.md`

- [ ] **Step 1: Ejecutar suite completa**

```powershell
dotnet test LaMesaDelDuque.slnx --no-restore
```

Expected:

```text
Passed! - Failed: 0
```

- [ ] **Step 2: Verificar arranque local**

```powershell
dotnet run --project src\LaMesaDelDuque.Web\LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile
```

Expected:

```text
Now listening on: http://localhost:5103
```

- [ ] **Step 3: Escanear secretos**

```powershell
Select-String -Path src\**\*.json,src\**\*.cs -Pattern "postgresql://|Password=|pwd=|supabase.com|3mp985" -CaseSensitive:$false
```

Expected:

```text
Sin resultados en archivos versionados de desarrollo.
```

- [ ] **Step 4: Escanear mojibake**

```powershell
Select-String -Path src\**\*.cs,src\**\*.cshtml,src\**\*.json -Pattern "Ã|Â¿|Â¡|â€”|â€œ|â€|â”"
```

Expected:

```text
Sin resultados.
```

- [ ] **Step 5: Escanear errores inseguros**

```powershell
Select-String -Path src\LaMesaDelDuque.Web\Pages\**\*.cs -Pattern "BadRequest\(ex\.Message\)|catch \(Exception ex\)"
```

Expected:

```text
Sin BadRequest(ex.Message). Los catch genéricos restantes deben estar justificados y no filtrar ex.Message.
```

- [ ] **Step 6: Confirmar migraciones**

```powershell
Select-String -Path src\LaMesaDelDuque.Infraestructura\Migrations\*.cs -Pattern "EfectivoReal|TarjetaReal|DiferenciaEfectivo|DiferenciaTarjeta|EsCerrado|CerradoEn|Lote|Tipo|Observacion" | Select-Object Path, LineNumber, Line
```

Expected:

```text
Los campos aparecen en clases Migration reales y en ModelSnapshot.
```

- [ ] **Step 7: Actualizar reporte final**

En `C:\Users\frenzied\Desktop\SoftwareGestionCalidad\lastobservations.md`, agregar:

```markdown
## Verificación final post-plan - 2026-05-23

### Comandos ejecutados

- `dotnet test LaMesaDelDuque.slnx --no-restore`
- `dotnet run --project src\LaMesaDelDuque.Web\LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile`
- scan de secretos
- scan de mojibake
- scan de errores inseguros
- scan de migraciones

### Resultado

- Tests: ...
- Arranque local: ...
- Browser audit: ...
- Bloqueadores restantes: ...

### Decisión

El sistema queda APROBADO / RECHAZADO para demo según evidencia anterior.
```

- [ ] **Step 8: Commit final si hubo cambios repo**

```powershell
git status --short
git add .
git commit -m "chore: complete audit repair verification"
```

Si `git status` solo muestra archivos del vault fuera del repo o evidencias no versionables, NO forzar commit vacío.

---

## Orden estricto de ejecución

1. Task 1: arranque local y secretos.
2. Task 2: migración formal.
3. Task 3: encoding.
4. Task 4: errores seguros.
5. Task 5: mesas/despacho.
6. Task 6: usuario obligatorio en pagos.
7. Task 7: observación de cierre con descuadre.
8. Task 8: navegación por rol.
9. Task 9: browser audit.
10. Task 10: verificación final.

NO ejecutar Task 9 antes de que Task 1 pase. Auditar en browser una app que no arranca localmente es teatro, no ingeniería.

## Riesgos y decisiones

- **Migración vacía:** si EF no detecta cambios porque el snapshot fue editado manualmente, restaurar snapshot anterior y regenerar. No aceptar migración vacía.
- **Tests acoplados:** si `PedidosServicio` es difícil de testear, crear fakes mínimos, no usar la base real salvo que ya exista patrón de integración.
- **Roles:** no inventar módulos faltantes en Home. Si dashboard/backup/configuración no existen, documentar gap; no crear tarjetas muertas.
- **Excepciones:** mensajes de dominio pueden mostrarse; errores infraestructurales no.
- **Secretos:** cualquier secreto ya expuesto debe considerarse comprometido y rotarse fuera de este plan.

## Self-Review

- Spec coverage: el plan cubre arranque, persistencia, seguridad, encoding, errores, mesas, pagos, cierre, navegación y auditoría browser.
- Placeholder scan: no hay `TBD` ni pasos sin comando; los dos tests complejos de aplicación indican completar con builders/fakes existentes porque dependen de patrones reales del repo, pero fijan comportamiento, archivo y expectativa exacta.
- Type consistency: se usan nombres observados en auditoría (`CierreDia`, `MermaDiaria`, `Pago`, `PedidosServicio`, `EstadoPedido`, `CierreCajaRequest`) y se indican ajustes si la firma exacta difiere.
