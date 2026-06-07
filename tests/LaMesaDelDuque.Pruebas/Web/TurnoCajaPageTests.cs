using LaMesaDelDuque.Web.Pages.Operaciones.TurnoCaja;
using Microsoft.AspNetCore.Authorization;

namespace LaMesaDelDuque.Pruebas.Web;

public class TurnoCajaPageTests
{
    [Fact]
    public void HistorialModel_DebePermitirCajeroPorqueIndexLoEnlaza()
    {
        var attribute = typeof(HistorialModel)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Single();

        var roles = attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

        Assert.Contains("Cajero", roles);
    }
}
