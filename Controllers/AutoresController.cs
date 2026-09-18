using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class AutoresController : Controller
{
    private readonly IAutorService _autorService;

    public AutoresController(IAutorService autorService)
    {
        _autorService = autorService;
    }

    public IActionResult Index()
    {
        return View(_autorService.GetAll());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Author author)
    {
        if (!ModelState.IsValid) return View(author);

        _autorService.Add(author);

        TempData["Success"] = $"El autor {author.Name} {author.Surname} se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
