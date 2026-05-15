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
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

// Capa de aplicación (servicios)
builder.Services.AgregarAplicacion();
builder.Services.AddScoped<INotificadorPedidos, SignalRNotificadorPedidos>();

// Persistencia con fail-fast si no hay connection string
builder.Services.AgregarPersistencia(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
    await db.Database.EnsureCreatedAsync();
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

        var entradas = new CategoriaProducto("Entradas", "Aperitivos y entrantes", 1);
        var platos = new CategoriaProducto("Platos Fuertes", "Platos principales", 2);
        var bebidas = new CategoriaProducto("Bebidas", "Bebidas", 3);
        var postres = new CategoriaProducto("Postres", "Postres y dulces", 4);
        db.Set<CategoriaProducto>().AddRange(entradas, platos, bebidas, postres);
        await db.SaveChangesAsync();

        db.Set<Producto>().AddRange(
            new Producto("Bruschetta Clásica", 8.50m, entradas, null, 8),
            new Producto("Solomillo al Duque", 24.00m, platos, null, 25),
            new Producto("Agua Mineral", 2.50m, bebidas, null, 1),
            new Producto("Tiramisú", 9.00m, postres, null, 5)
        );
        await db.SaveChangesAsync();

        for (int i = 1; i <= 10; i++)
            db.Set<Mesa>().Add(new Mesa(i, i <= 8 ? 4 : 8));
        await db.SaveChangesAsync();
    }
}

app.Run();
