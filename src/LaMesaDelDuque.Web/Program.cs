using LaMesaDelDuque.Aplicacion;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Web.Hubs;
using LaMesaDelDuque.Web.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Requerido por Npgsql 6+ con Supabase: sin esto los DateTime fallan al leer/escribir
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizeFolder("/Operaciones");
    options.Conventions.AuthorizeFolder("/Admin");
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

// Persistencia con fail-fast si no hay connection string
builder.Services.AgregarPersistencia(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

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
        db.Set<Rol>().AddRange(adminRol, meseroRol, encargadoRol, cocineroRol);
        await db.SaveChangesAsync();

        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12);
        var meseroHash = BCrypt.Net.BCrypt.HashPassword("Mesero789!", 12);
        var encargadoHash = BCrypt.Net.BCrypt.HashPassword("Encargado321!", 12);
        var cocineroHash = BCrypt.Net.BCrypt.HashPassword("Cocina456!", 12);
        db.Set<Usuario>().AddRange(
            new Usuario("admin", "admin@mesadelduque.com", adminHash, "Administrador", adminRol),
            new Usuario("maria", "maria@mesadelduque.com", meseroHash, "María Mesera", meseroRol),
            new Usuario("carlos", "carlos@mesadelduque.com", encargadoHash, "Carlos Encargado", encargadoRol),
            new Usuario("pedro", "pedro@mesadelduque.com", cocineroHash, "Pedro Cocinero", cocineroRol)
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
    }

    // Repair stale seed hashes — no-op when already correct
    var seedCredentials = new Dictionary<string, string>
    {
        ["admin"]   = "Admin123!",
        ["maria"]   = "Mesero789!",
        ["carlos"]  = "Encargado321!",
        ["pedro"]   = "Cocina456!"
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
