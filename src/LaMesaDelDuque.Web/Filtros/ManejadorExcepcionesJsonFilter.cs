using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LaMesaDelDuque.Web.Filtros;

public sealed class ManejadorExcepcionesJsonFilter : IExceptionFilter
{
    private readonly ILogger<ManejadorExcepcionesJsonFilter> _logger;

    public ManejadorExcepcionesJsonFilter(ILogger<ManejadorExcepcionesJsonFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (!EsRespuestaJson(context))
            return;

        IActionResult result = context.Exception switch
        {
            ReglaDominioException ex => new ObjectResult(new { ok = false, error = ex.Message })
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            },
            ArgumentException ex => new ObjectResult(new { ok = false, error = ex.Message })
            {
                StatusCode = StatusCodes.Status400BadRequest
            },
            _ => BuildInternalError(context.Exception)
        };

        context.Result = result;
        context.ExceptionHandled = true;
    }

    private ObjectResult BuildInternalError(Exception ex)
    {
        _logger.LogError(ex, "Error no controlado en handler JSON");
        return new ObjectResult(new { ok = false, error = "Ocurrió un error interno." })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }

    private static bool EsRespuestaJson(ExceptionContext context)
    {
        var request = context.HttpContext.Request;

        var accept = request.Headers.Accept.ToString();
        if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            return true;

        var contentType = request.ContentType ?? string.Empty;
        if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            return true;

        var actionDescriptor = context.ActionDescriptor;
        if (actionDescriptor?.DisplayName?.Contains("Json", StringComparison.OrdinalIgnoreCase) == true)
            return true;

        var path = request.Path.Value ?? string.Empty;
        if (path.Contains("Json", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
