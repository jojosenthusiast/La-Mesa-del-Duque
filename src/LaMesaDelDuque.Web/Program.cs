using LaMesaDelDuque.Aplicacion;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Web.Filtros;
using LaMesaDelDuque.Web.Hubs;
using LaMesaDelDuque.Web.Seguridad;
using LaMesaDelDuque.Web.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

// Requerido por Npgsql 6+ con Supabase: sin esto los DateTime fallan al leer/escribir
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// QuestPDF Community License
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ManejadorExcepcionesJsonFilter>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizeFolder("/Operaciones");
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.ConfigureFilter(new TypeFilterAttribute(typeof(ManejadorExcepcionesJsonFilter)));
});

// Autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Capa de aplicación (servicios)
builder.Services.AgregarAplicacion();
builder.Services.AddScoped<INotificadorPedidos, SignalRNotificadorPedidos>();
builder.Services.AddScoped<INotificadorSalon, SignalRNotificadorSalon>();
builder.Services.AddScoped<INotificadorDashboard, SignalRNotificadorDashboard>();
builder.Services.AddScoped<INotificadorProductos, SignalRNotificadorProductos>();

// Persistencia con fail-fast si no hay connection string
builder.Services.AgregarPersistencia(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseLaMesaSecurityHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<PedidosHub>("/hubs/pedidos").RequireAuthorization();

// Seed de desarrollo: crea roles, admin, categorías y datos operativos
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LaMesaDelDuqueDbContext>();
    Console.WriteLine($"DB PROVIDER: {db.Database.ProviderName}");
    if (db.Database.ProviderName!.Contains("Sqlite"))
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    if (!await db.Set<Rol>().AnyAsync())
    {
        var adminRol = new Rol("Administrador", "Acceso total al sistema");
        var meseroRol = new Rol("Mesero", "Captura de pedidos y consulta de salón");
        var encargadoRol = new Rol("Encargado", "Gestión de catálogo, mesas y reportes");
        var cocineroRol = new Rol("Cocinero", "Visualización de pedidos en preparación");
        var cajeroRol = new Rol("Cajero", "Cobro en caja, despacho y cierre de turno");
        var gerenteRol = new Rol("Gerente", "Acceso a reportes, dashboard y auditoría sin módulos operativos");
        db.Set<Rol>().AddRange(adminRol, meseroRol, encargadoRol, cocineroRol, cajeroRol, gerenteRol);
        await db.SaveChangesAsync();

        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12);
        var meseroHash = BCrypt.Net.BCrypt.HashPassword("Mesero789!", 12);
        var encargadoHash = BCrypt.Net.BCrypt.HashPassword("Encargado321!", 12);
        var cocineroHash = BCrypt.Net.BCrypt.HashPassword("Cocina456!", 12);
        var cajeroHash = BCrypt.Net.BCrypt.HashPassword("Cajero567!", 12);
        var gerenteHash = BCrypt.Net.BCrypt.HashPassword("Gerente890!", 12);
        db.Set<Usuario>().AddRange(
            new Usuario("admin", "admin@mesadelduque.com", adminHash, "Administrador", adminRol),
            new Usuario("maria", "maria@mesadelduque.com", meseroHash, "María Mesera", meseroRol),
            new Usuario("carlos", "carlos@mesadelduque.com", encargadoHash, "Carlos Encargado", encargadoRol),
            new Usuario("pedro", "pedro@mesadelduque.com", cocineroHash, "Pedro Cocinero", cocineroRol),
            new Usuario("sofia", "sofia@mesadelduque.com", cajeroHash, "Sofía Cajera", cajeroRol),
            new Usuario("luciana", "luciana@mesadelduque.com", gerenteHash, "Luciana Gerente", gerenteRol)
        );
        await db.SaveChangesAsync();

        var entradas = new CategoriaProducto("Entradas", "Aperitivos y entrantes", 1, LaMesaDelDuque.Dominio.Enumeraciones.EstacionCocina.Caliente);
        var platos = new CategoriaProducto("Platos Fuertes", "Platos principales", 2, LaMesaDelDuque.Dominio.Enumeraciones.EstacionCocina.Parrilla);
        var bebidas = new CategoriaProducto("Bebidas", "Bebidas", 3, LaMesaDelDuque.Dominio.Enumeraciones.EstacionCocina.Bar);
        var postres = new CategoriaProducto("Postres", "Postres y dulces", 4, LaMesaDelDuque.Dominio.Enumeraciones.EstacionCocina.Fria);
        db.Set<CategoriaProducto>().AddRange(entradas, platos, bebidas, postres);
        await db.SaveChangesAsync();

        db.Set<Producto>().AddRange(
            new Producto("Bruschetta Clásica", 8.50m, entradas, "/images/productos/bruschetta.jpg", 8),
            new Producto("Solomillo al Duque", 24.00m, platos, "/images/productos/solomillo.jpg", 25),
            new Producto("Agua Mineral", 2.50m, bebidas, "/images/productos/agua-mineral.jpg", 1),
            new Producto("Tiramisú", 9.00m, postres, "/images/productos/tiramisu.jpg", 5)
        );
        await db.SaveChangesAsync();

        for (int i = 1; i <= 10; i++)
            db.Set<Mesa>().Add(new Mesa(i, i <= 8 ? 4 : 8));
        await db.SaveChangesAsync();

        db.Set<RestauranteConfig>().Add(new RestauranteConfig(
            "La Mesa del Duque",
            "Av. Principal #1, San Salvador",
            new TimeOnly(11, 0),
            new TimeOnly(23, 0),
            cantidadMesas: 10,
            periodoGraciaMinutos: 5));
        await db.SaveChangesAsync();

        // ── Seed: Alérgenos ──────────────────────────────────
        if (!await db.Set<Alergeno>().AnyAsync())
        {
            var alergenos = new[]
            {
                new Alergeno("Gluten"),
                new Alergeno("Lácteos"),
                new Alergeno("Mariscos"),
                new Alergeno("Maní"),
                new Alergeno("Huevo"),
                new Alergeno("Soja"),
                new Alergeno("Sulfitos"),
                new Alergeno("Pescado")
            };
            db.Set<Alergeno>().AddRange(alergenos);
            await db.SaveChangesAsync();
        }

        // ── Seed: Proveedores ───────────────────────────────
        if (!await db.Set<Proveedor>().AnyAsync())
        {
            var p1 = new Proveedor("Distribuidora La Plaza", "0614-010101-101-1", "Juan Mercado", "2288-1111", "ventas@laplaza.com.sv", "Av. Central #45, San Salvador");
            var p2 = new Proveedor("Carnes Selectas S.A.", "0614-020202-202-2", "Roberto Parrilla", "2288-2222", "pedidos@carnesselectas.com.sv", "Calle La Reforma #120, Santa Tecla");
            var p3 = new Proveedor("Bebidas Premium", "0614-030303-303-3", "Ana Licores", "2288-3333", "info@bebidaspremium.com.sv", "Blvd. Los Héroes #300, San Salvador");
            db.Set<Proveedor>().AddRange(p1, p2, p3);
            await db.SaveChangesAsync();
        }

        // ── Seed: Ingredientes ──────────────────────────────
        if (!await db.Set<Ingrediente>().AnyAsync())
        {
            var provs = await db.Set<Proveedor>().ToListAsync();
            var laPlaza = provs.FirstOrDefault(pr => pr.Nombre.Contains("Plaza"));
            var carnes = provs.FirstOrDefault(pr => pr.Nombre.Contains("Carnes"));
            var bebidasP = provs.FirstOrDefault(pr => pr.Nombre.Contains("Bebidas"));

            db.Set<Ingrediente>().AddRange(
                new Ingrediente("Pan baguette", "unidad", 30, 5, 0.80m, laPlaza),
                new Ingrediente("Tomate", "kg", 10, 2, 3.50m, laPlaza),
                new Ingrediente("Albahaca fresca", "manojo", 5, 1, 2.00m, laPlaza),
                new Ingrediente("Ajo", "kg", 2, 0.5m, 4.00m, laPlaza),
                new Ingrediente("Aceite de oliva", "litro", 5, 1, 12.00m, laPlaza),
                new Ingrediente("Solomillo de res", "kg", 8, 2, 18.00m, carnes),
                new Ingrediente("Papas", "kg", 20, 5, 2.50m, laPlaza),
                new Ingrediente("Sal y pimienta", "kg", 3, 0.5m, 5.00m, laPlaza),
                new Ingrediente("Mantequilla", "kg", 4, 1, 8.00m, laPlaza),
                new Ingrediente("Agua mineral", "litro", 50, 10, 0.60m, bebidasP),
                new Ingrediente("Mascarpone", "kg", 4, 1, 15.00m, laPlaza),
                new Ingrediente("Café espresso", "kg", 5, 1, 20.00m, laPlaza),
                new Ingrediente("Cacao en polvo", "kg", 2, 0.5m, 10.00m, laPlaza),
                new Ingrediente("Huevos", "unidad", 60, 12, 0.30m, laPlaza),
                new Ingrediente("Azúcar", "kg", 10, 2, 1.50m, laPlaza)
            );
            await db.SaveChangesAsync();
        }

        // ── Seed: Recetas ───────────────────────────────────
        var productos = await db.Set<Producto>().ToListAsync();
        var bruschetta = productos.FirstOrDefault(p => p.Nombre.Contains("Bruschetta"));
        var solomillo = productos.FirstOrDefault(p => p.Nombre.Contains("Solomillo"));
        var tiramisu = productos.FirstOrDefault(p => p.Nombre.Contains("Tiramisú"));
        var ings = await db.Set<Ingrediente>().ToListAsync();

        if (bruschetta is not null && !await db.Set<RecetaProducto>().AnyAsync(r => r.ProductoId == bruschetta.Id))
        {
            var riB = new[]
            {
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Pan baguette")), 1m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Tomate")), 0.2m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Albahaca")), 0.05m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Ajo")), 0.01m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Aceite de oliva")), 0.03m),
            };
            db.Set<RecetaProducto>().Add(new RecetaProducto(bruschetta,
                "1. Tostar el pan baguette en rebanadas.\n2. Picar tomate y albahaca, mezclar con ajo picado y aceite de oliva.\n3. Colocar la mezcla sobre cada rebanada tostada.\n4. Servir inmediatamente.",
                riB));
            await db.SaveChangesAsync();
        }

        if (solomillo is not null && !await db.Set<RecetaProducto>().AnyAsync(r => r.ProductoId == solomillo.Id))
        {
            var riS = new[]
            {
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Solomillo de res")), 0.3m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Papas")), 0.25m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Sal y pimienta")), 0.005m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Mantequilla")), 0.02m),
            };
            db.Set<RecetaProducto>().Add(new RecetaProducto(solomillo,
                "1. Sellar el solomillo en sartén caliente con mantequilla.\n2. Llevar al horno a 180°C por 12-15 min (término medio).\n3. Cortar papas en bastones y freír hasta dorar.\n4. Salpimentar al gusto. Servir con las papas.",
                riS));
            await db.SaveChangesAsync();
        }

        if (tiramisu is not null && !await db.Set<RecetaProducto>().AnyAsync(r => r.ProductoId == tiramisu.Id))
        {
            var riT = new[]
            {
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Mascarpone")), 0.25m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Café espresso")), 0.05m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Cacao en polvo")), 0.02m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Huevos")), 3m),
                new RecetaIngrediente(ings.First(i => i.Nombre.Contains("Azúcar")), 0.1m),
            };
            db.Set<RecetaProducto>().Add(new RecetaProducto(tiramisu,
                "1. Preparar café espresso fuerte y dejar enfriar.\n2. Batir yemas con azúcar, agregar mascarpone.\n3. Montar claras a nieve e incorporar.\n4. Remojar bizcochos en café y armar capas alternando crema y bizcochos.\n5. Espolvorear cacao y refrigerar 4 horas.",
                riT));
            await db.SaveChangesAsync();
        }

        // ── Seed: ProductoAlergeno ──────────────────────────
        if (!await db.Set<ProductoAlergeno>().AnyAsync())
        {
            var alergenos = await db.Set<Alergeno>().ToListAsync();
            var GetAle = (string name) => alergenos.First(a => a.Nombre == name);

            if (bruschetta is not null)
            {
                db.Set<ProductoAlergeno>().AddRange(
                    new ProductoAlergeno(bruschetta, GetAle("Gluten"), "Pan baguette de trigo"),
                    new ProductoAlergeno(bruschetta, GetAle("Lácteos"), "Queso parmesano en la mezcla")
                );
            }
            if (solomillo is not null)
            {
                db.Set<ProductoAlergeno>().AddRange(
                    new ProductoAlergeno(solomillo, GetAle("Lácteos"), "Mantequilla para sellar la carne")
                );
            }
            if (tiramisu is not null)
            {
                db.Set<ProductoAlergeno>().AddRange(
                    new ProductoAlergeno(tiramisu, GetAle("Gluten"), "Bizcochos de soletilla"),
                    new ProductoAlergeno(tiramisu, GetAle("Lácteos"), "Queso mascarpone"),
                    new ProductoAlergeno(tiramisu, GetAle("Huevo"), "Huevos en la crema")
                );
            }
            await db.SaveChangesAsync();
        }
    }

    if (!await db.Set<RestauranteConfig>().AnyAsync())
    {
        db.Set<RestauranteConfig>().Add(new RestauranteConfig(
            "La Mesa del Duque",
            "Av. Principal #1, San Salvador",
            new TimeOnly(11, 0),
            new TimeOnly(23, 0),
            cantidadMesas: 10,
            periodoGraciaMinutos: 5));
        await db.SaveChangesAsync();
    }

    // ── Seed: Motivos de descuento ──────────────────────────
    if (!await db.Set<MotivoDescuento>().AnyAsync())
    {
        db.Set<MotivoDescuento>().AddRange(
            new MotivoDescuento("Error de cocina", "Producto llegó frío, incorrecto o tarde."),
            new MotivoDescuento("Cliente VIP", "Cliente frecuente o de alto valor."),
            new MotivoDescuento("Aniversario o celebración", "Descuento por ocasión especial del cliente."),
            new MotivoDescuento("Cortesía de la casa", "Obsequio discrecional del establecimiento."),
            new MotivoDescuento("Inconveniencia al cliente", "Compensación por demora o problema de servicio.")
        );
        await db.SaveChangesAsync();
    }

    // Repair stale seed hashes — no-op when already correct
    var seedCredentials = new Dictionary<string, string>
    {
        ["admin"]   = "Admin123!",
        ["maria"]   = "Mesero789!",
        ["carlos"]  = "Encargado321!",
        ["pedro"]   = "Cocina456!",
        ["sofia"]   = "Cajero567!"
    };
    var seedUsernames = seedCredentials.Keys.ToList();
    var seedUsers = await db.Set<Usuario>().Where(u => seedUsernames.Contains(u.Username)).ToListAsync();
    bool anyRepaired = false;
    foreach (var user in seedUsers)
    {
        var expected = seedCredentials[user.Username];
        if (!BCrypt.Net.BCrypt.Verify(expected, user.PasswordHash))
        {
            user.CambiarPasswordHash(BCrypt.Net.BCrypt.HashPassword(expected, 12));
            anyRepaired = true;
            Console.WriteLine($"[DEV] {user.Username} password hash repaired.");
        }
    }
    if (anyRepaired) await db.SaveChangesAsync();
}

app.Run();
