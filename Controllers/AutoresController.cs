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
    public IActionResult Edit(int id)
    {
        var author = _autorService.GetById(id);
        if (author is null) return NotFound();
        return View(author);
    }

    [HttpPost]
    public IActionResult Edit(int id, Author author)
    {
        if (id != author.ID) return BadRequest();
        if (!ModelState.IsValid) return View(author);

        var updated = _autorService.Update(id, author);
        if (!updated) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var author = _autorService.GetById(id);
        if (author is null) return NotFound();
        return View(author);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var deleted = _autorService.Delete(id);
        if (!deleted) return NotFound();

        return RedirectToAction(nameof(Index));
    }
}
