using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

// El módulo completo requiere una sesión activa; la gestión fina se restringe por rol en cada acción.
[Authorize]
public class LibrosController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 2 * 1024 * 1024;

    private readonly BibliotecaDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LibrosController> _logger;

    public LibrosController(BibliotecaDbContext context, IWebHostEnvironment env, ILogger<LibrosController> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    // Consulta: cualquier usuario autenticado (en cualquier rol) puede ver el catálogo.
    public IActionResult Index()
    {
        _logger.LogInformation("Consulta del catálogo de libros iniciada.");

        var libros = _context.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToList();

        _logger.LogInformation("Catálogo de libros consultado. Total de libros obtenidos: {Cantidad}.", libros.Count);

        return View(libros);
    }

    // Consulta: cualquier usuario autenticado puede ver el detalle de un libro.
    public IActionResult Details(int id)
    {
        _logger.LogInformation("Consulta del detalle del libro con identificador {LibroId}.", id);

        var libro = _context.Libros
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == id);

        if (libro is null)
        {
            _logger.LogWarning("No se encontró el libro con identificador {LibroId}.", id);
            return NotFound();
        }

        _logger.LogInformation("Detalle del libro {LibroId} ({Titulo}) consultado.", libro.Id, libro.Titulo);

        return View(libro);
    }

    // Gestión: solo los roles Admin y Bibliotecario pueden agregar libros.
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult Create()
    {
        _logger.LogInformation("Se abrió el formulario de alta de libros.");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult Create(Libro libro, IFormFile? image)
    {
        _logger.LogInformation("Alta de libro solicitada. Título: {Titulo}.", libro.Titulo);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Alta de libro rechazada: el modelo no superó la validación. Título: {Titulo}.", libro.Titulo);
            return View(libro);
        }

        var savedFile = TrySaveImage(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            _logger.LogWarning("Alta de libro rechazada: la imagen no pudo guardarse. Motivo: {Motivo}.", failure.Message);
            ModelState.AddModelError(nameof(Libro.ImagenPath), failure.Message);
            return View(libro);
        }

        libro.ImagenPath = savedFile is ImageSaveResult.Success success && !string.IsNullOrEmpty(success.FileName)
            ? success.FileName
            : null;

        try
        {
            _context.Libros.Add(libro);
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al guardar el libro '{Titulo}' en la base de datos.", libro.Titulo);
            throw;
        }

        _logger.LogInformation("Libro creado correctamente. Identificador: {LibroId}, título: {Titulo}.", libro.Id, libro.Titulo);

        TempData["Success"] = $"El libro \"{libro.Titulo}\" se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Gestión: solo los roles Admin y Bibliotecario pueden editar libros.
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult Edit(int id)
    {
        _logger.LogInformation("Se abrió el formulario de edición del libro {LibroId}.", id);

        var libro = _context.Libros.Find(id);

        if (libro is null)
        {
            _logger.LogWarning("No se encontró el libro con identificador {LibroId} para editar.", id);
            return NotFound();
        }

        return View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult Edit(int id, Libro libro, IFormFile? image)
    {
        _logger.LogInformation("Actualización del libro {LibroId} solicitada. Título: {Titulo}.", id, libro.Titulo);

        if (id != libro.Id)
        {
            _logger.LogWarning("Actualización rechazada: el identificador de la ruta ({RutaId}) no coincide con el del formulario ({FormularioId}).", id, libro.Id);
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Actualización del libro {LibroId} rechazada: el modelo no superó la validación.", id);
            return View(libro);
        }

        var current = _context.Libros.Find(id);
        if (current is null)
        {
            _logger.LogWarning("No se encontró el libro con identificador {LibroId} para actualizar.", id);
            return NotFound();
        }

        var previousImage = current.ImagenPath;

        _context.Entry(current).State = EntityState.Detached;

        var savedFile = TrySaveImage(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            _logger.LogWarning("Actualización del libro {LibroId} rechazada: la imagen no pudo guardarse. Motivo: {Motivo}.", id, failure.Message);
            ModelState.AddModelError(nameof(Libro.ImagenPath), failure.Message);
            return View(libro);
        }

        var newImageFileName = (savedFile as ImageSaveResult.Success)?.FileName;
        var hasNewImage = !string.IsNullOrEmpty(newImageFileName);

        libro.ImagenPath = hasNewImage ? newImageFileName : previousImage;

        try
        {
            _context.Libros.Update(libro);
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al actualizar el libro {LibroId} en la base de datos.", id);
            throw;
        }

        if (hasNewImage)
        {
            DeleteImageFile(previousImage);
        }

        _logger.LogInformation("Libro {LibroId} actualizado correctamente. Título: {Titulo}.", libro.Id, libro.Titulo);

        TempData["Success"] = $"El libro \"{libro.Titulo}\" se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Gestión: solo los roles Admin y Bibliotecario pueden eliminar libros.
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Se solicitó la eliminación del libro {LibroId}.", id);

        var libro = _context.Libros
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == id);

        if (libro is null)
        {
            _logger.LogWarning("No se encontró el libro con identificador {LibroId} para eliminar.", id);
            return NotFound();
        }

        return View(libro);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult DeleteConfirmed(int id)
    {
        _logger.LogInformation("Eliminación del libro {LibroId} solicitada.", id);

        var libro = _context.Libros.Find(id);
        if (libro is null)
        {
            _logger.LogWarning("No se encontró el libro con identificador {LibroId} para eliminar.", id);
            return NotFound();
        }

        var titulo = libro.Titulo;

        try
        {
            DeleteImageFile(libro.ImagenPath);
            _context.Libros.Remove(libro);
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al eliminar el libro {LibroId} de la base de datos.", id);
            throw;
        }

        _logger.LogInformation("Libro {LibroId} eliminado correctamente. Título: {Titulo}.", id, titulo);

        TempData["Success"] = $"El libro \"{titulo}\" se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private ImageSaveResult TrySaveImage(IFormFile? image)
    {
        if (image is null || image.Length == 0) return new ImageSaveResult.Success(string.Empty);

        if (image.Length > MaxFileSize)
        {
            _logger.LogWarning("Se rechazó una imagen por superar el tamaño máximo permitido ({TamanoMaximo} bytes).", MaxFileSize);
            return new ImageSaveResult.Failure("La imagen no puede superar los 2 MB.");
        }

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            _logger.LogWarning("Se rechazó una imagen con formato no permitido: {Extension}.", ext);
            return new ImageSaveResult.Failure("Formato no permitido. Use jpg, jpeg, png, gif o webp.");
        }

        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(_env.WebRootPath, "images", fileName);

        try
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.CopyTo(stream);
            }
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error al escribir el archivo de imagen de la portada en el servidor.");
            return new ImageSaveResult.Failure("No fue posible guardar la imagen. Intentá nuevamente.");
        }

        _logger.LogInformation("Portada del libro guardada correctamente. Archivo: {Archivo}.", fileName);

        return new ImageSaveResult.Success(fileName);
    }

    private void DeleteImageFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;

        fileName = Path.GetFileName(fileName);

        var path = Path.Combine(_env.WebRootPath, "images", fileName);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }

    private abstract record ImageSaveResult
    {
        public sealed record Success(string FileName) : ImageSaveResult;
        public sealed record Failure(string Message) : ImageSaveResult;
    }
}
