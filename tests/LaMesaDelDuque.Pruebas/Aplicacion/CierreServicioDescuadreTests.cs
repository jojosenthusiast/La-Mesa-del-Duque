using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public sealed class CierreServicioDescuadreTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly ICierreServicio _servicio;

    public CierreServicioDescuadreTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _uot = new UnidadDeTrabajo(_contexto,
            new CategoriaProductoRepositorio(_contexto),
            new ProductoRepositorio(_contexto),
            new IngredienteRepositorio(_contexto),
            new MesaRepositorio(_contexto),
            new PedidoRepositorio(_contexto),
            new RolRepositorio(_contexto),
            new UsuarioRepositorio(_contexto),
            new AuditoriaRepositorio(_contexto),
            new RecetaProductoRepositorio(_contexto),
            new OrdenCocinaRepositorio(_contexto),
            new CuentaRepositorio(_contexto),
            new PagoRepositorio(_contexto),
            null,
            new MermaRepositorio(_contexto),
            new CierreDiaRepositorio(_contexto));

        var mermaServicio = new MermaServicio(_uot);
        _servicio = new CierreServicio(_uot, mermaServicio);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<Usuario> CrearUsuarioAsync()
    {
        var rol = new Rol("Encargado");
        await _contexto.Set<Rol>().AddAsync(rol);
        var usuario = new Usuario("encargado", null, "$2a$12$hash", "Encargado Test", rol);
        await _contexto.Set<Usuario>().AddAsync(usuario);
        await _contexto.SaveChangesAsync();
        return usuario;
    }

    [Fact]
    public async Task CerrarDia_ConDescuadreYSinObservacion_DebeLanzarExcepcion()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CerrarDiaAsync(new CierreCajaRequest
            {
                EfectivoReal = 900m,
                TarjetaReal = 0m,
                Observacion = ""
            }, usuario.Id));

        Assert.Contains("observacion", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CerrarDia_ConDescuadreYConObservacion_DebePermitir()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);

        var dto = await _servicio.CerrarDiaAsync(new CierreCajaRequest
        {
            EfectivoReal = 900m,
            TarjetaReal = 0m,
            Observacion = "Faltante por error de cambio"
        }, usuario.Id);

        Assert.True(dto.EsCerrado);
    }

    [Fact]
    public async Task CerrarDia_SinDescuadreYSinObservacion_DebePermitir()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);

        var dto = await _servicio.CerrarDiaAsync(new CierreCajaRequest
        {
            EfectivoReal = 0m,
            TarjetaReal = 0m,
            Observacion = ""
        }, usuario.Id);

        Assert.True(dto.EsCerrado);
        Assert.Equal(0m, dto.DiferenciaEfectivo);
        Assert.Equal(0m, dto.DiferenciaTarjeta);
    }
}
