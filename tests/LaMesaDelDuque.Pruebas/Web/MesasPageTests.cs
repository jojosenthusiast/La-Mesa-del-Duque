using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Pages.Operaciones.Mesas;

namespace LaMesaDelDuque.Pruebas.Web;

public class MesasPageTests
{
    [Fact]
    public async Task OnGetAsync_carga_mesas_y_resumen_por_estado()
    {
        var servicio = new FakeMesasServicio
        {
            Mesas =
            [
                new MesaDto { Id = Guid.NewGuid(), Numero = 1, Capacidad = 4, Estado = "Disponible", Activa = true },
                new MesaDto { Id = Guid.NewGuid(), Numero = 2, Capacidad = 6, Estado = "Ocupada", Activa = true },
                new MesaDto { Id = Guid.NewGuid(), Numero = 3, Capacidad = 2, Estado = "Disponible", Activa = true }
            ]
        };

        var page = new IndexModel(servicio);

        await page.OnGetAsync();

        Assert.Equal(3, page.Vm.Mesas.Count);
        Assert.Equal(2, page.Vm.ResumenPorEstado["Disponible"]);
        Assert.Equal(1, page.Vm.ResumenPorEstado["Ocupada"]);
    }
}

internal sealed class FakeMesasServicio : IMesasServicio
{
    public List<MesaDto> Mesas { get; set; } = [];

    public Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default)
        => Task.FromResult(Mesas);

    public Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default)
        => Task.FromResult(Mesas.FirstOrDefault(m => m.Numero == numero));

    public Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default)
        => Task.FromResult(new MesaDto { Id = Guid.NewGuid(), Numero = numero, Capacidad = capacidad, Estado = "Disponible", Activa = true });

    public Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default)
        => Task.FromResult(new MesaDto { Id = mesaId, Numero = numero, Capacidad = capacidad, Estado = "Disponible", Activa = true });

    public Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default)
        => Task.CompletedTask;

    public Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default)
        => Task.CompletedTask;
}
