using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class CategoriasController : Controller
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    public IActionResult Index()
    {
        return View(_categoriaService.GetAll());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        _categoriaService.Create(categoria);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var categoria = _categoriaService.GetById(id);
        if (categoria is null) return NotFound();
        return View(categoria);
    }

    [HttpPost]
    public IActionResult Edit(int id, Categoria categoria)
    {
        if (id != categoria.Id) return BadRequest();
        if (!ModelState.IsValid) return View(categoria);

        var updated = _categoriaService.Update(id, categoria);
        if (!updated) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var categoria = _categoriaService.GetById(id);
        if (categoria is null) return NotFound();
        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var deleted = _categoriaService.Delete(id);
        if (!deleted) return NotFound();

        return RedirectToAction(nameof(Index));
    }
}
