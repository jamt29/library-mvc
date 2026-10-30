using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Biblioteca.Controllers;

public class CategoriasController : Controller
{
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaService categoriaService, ILogger<CategoriasController> logger)
    {
        _categoriaService = categoriaService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Consulta del listado de categorías iniciada.");

        try
        {
            var categorias = _categoriaService.GetAll();

            _logger.LogInformation("Listado de categorías consultado. Total de categorías obtenidas: {Cantidad}.", categorias.Count());

            return View(categorias);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Error al consultar las categorías en la base de datos.");
            throw;
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        _logger.LogInformation("Se abrió el formulario de alta de categorías.");

        return View();
    }

    [HttpPost]
    public IActionResult Create(Categoria categoria)
    {
        _logger.LogInformation("Alta de categoría solicitada. Nombre: {Nombre}.", categoria.Nombre);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Alta de categoría rechazada: el modelo no superó la validación. Nombre: {Nombre}.", categoria.Nombre);
            return View(categoria);
        }

        try
        {
            _categoriaService.Create(categoria);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Error al guardar la categoría '{Nombre}' en la base de datos.", categoria.Nombre);
            throw;
        }

        _logger.LogInformation("Categoría creada correctamente. Identificador: {CategoriaId}, nombre: {Nombre}.", categoria.Id, categoria.Nombre);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        _logger.LogInformation("Se abrió el formulario de edición de la categoría {CategoriaId}.", id);

        var categoria = _categoriaService.GetById(id);
        if (categoria is null)
        {
            _logger.LogWarning("No se encontró la categoría con identificador {CategoriaId}.", id);
            return NotFound();
        }

        return View(categoria);
    }

    [HttpPost]
    public IActionResult Edit(int id, Categoria categoria)
    {
        _logger.LogInformation("Actualización de la categoría {CategoriaId} solicitada. Nombre: {Nombre}.", id, categoria.Nombre);

        if (id != categoria.Id)
        {
            _logger.LogWarning("Actualización rechazada: el identificador de la ruta ({RutaId}) no coincide con el del formulario ({FormularioId}).", id, categoria.Id);
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Actualización de la categoría {CategoriaId} rechazada: el modelo no superó la validación.", id);
            return View(categoria);
        }

        bool updated;
        try
        {
            updated = _categoriaService.Update(id, categoria);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Error al actualizar la categoría {CategoriaId} en la base de datos.", id);
            throw;
        }

        if (!updated)
        {
            _logger.LogWarning("No se encontró la categoría con identificador {CategoriaId} para actualizar.", id);
            return NotFound();
        }

        _logger.LogInformation("Categoría {CategoriaId} actualizada correctamente. Nombre: {Nombre}.", id, categoria.Nombre);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Se solicitó la eliminación de la categoría {CategoriaId}.", id);

        var categoria = _categoriaService.GetById(id);
        if (categoria is null)
        {
            _logger.LogWarning("No se encontró la categoría con identificador {CategoriaId} para eliminar.", id);
            return NotFound();
        }

        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        _logger.LogInformation("Eliminación de la categoría {CategoriaId} solicitada.", id);

        bool deleted;
        try
        {
            deleted = _categoriaService.Delete(id);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Error al eliminar la categoría {CategoriaId} de la base de datos.", id);
            throw;
        }

        if (!deleted)
        {
            _logger.LogWarning("No se encontró la categoría con identificador {CategoriaId} para eliminar.", id);
            return NotFound();
        }

        _logger.LogInformation("Categoría {CategoriaId} eliminada correctamente.", id);

        return RedirectToAction(nameof(Index));
    }
}
