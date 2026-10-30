using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;

namespace Biblioteca.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Books()
    {
        return View();
    }

    public IActionResult Authors()
    {
        return View();
    }

    public IActionResult Categories()
    {
        return RedirectToAction("Index", "Categorias");
    }

    public IActionResult Users()
    {
        return View();
    }

    public IActionResult Loans()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    // Página de error general: se invoca desde UseExceptionHandler y desde
    // UseStatusCodePagesWithReExecute. Solo muestra información general al usuario.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code = null)
    {
        var statusCode = code ?? StatusCodes.Status500InternalServerError;
        Response.StatusCode = statusCode;

        var requestId = HttpContext.TraceIdentifier;

        _logger.LogWarning(
            "Se muestra la página de error personalizada. Código HTTP: {Codigo}. Identificador de petición: {RequestId}.",
            statusCode,
            requestId);

        return View(new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode
        });
    }
}
