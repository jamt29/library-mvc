using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;

namespace Biblioteca.Controllers;

public class HomeController : Controller
{
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
