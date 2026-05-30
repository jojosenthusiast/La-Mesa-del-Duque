using LaMesaDelDuque.Web.Models.Operaciones;
using LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace LaMesaDelDuque.Pruebas.Web;

public sealed class TablesidePageTests
{
    [Fact]
    public async Task OnPostCrearJsonAsync_TipoServicioInvalido_DebeRechazarSinUsarDefault()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var page = new TablesideModel(
            new FakePedidosServicio(),
            catalogo,
            new FakePedidosMesasServicio(),
            NullLogger<TablesideModel>.Instance)
        {
            Vm = new PedidosPageVm
            {
                CrearPedido = new CrearPedidoFormVm
                {
                    TipoServicio = "ServicioRoto",
                    Lineas =
                    [
                        new LineaPedidoFormVm
                        {
                            ProductoId = catalogo.ProductoActivoId,
                            Cantidad = 1,
                            PrecioUnitario = 20m
                        }
                    ]
                }
            }
        };

        var result = await page.OnPostCrearJsonAsync();

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Tipo de servicio inválido.", badRequest.Value);
    }
}
