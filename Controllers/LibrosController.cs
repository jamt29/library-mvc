using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

public class LibrosController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 2 * 1024 * 1024;

    private readonly BibliotecaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public LibrosController(BibliotecaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index()
    {
        var libros = _context.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToList();

        return View(libros);
    }

    public IActionResult Details(int id)
    {
        var libro = _context.Libros
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == id);

        if (libro is null) return NotFound();
        return View(libro);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Libro libro, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(libro);

        var savedFile = TrySaveImage(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            ModelState.AddModelError(nameof(Libro.ImagenPath), failure.Message);
            return View(libro);
        }

        libro.ImagenPath = savedFile is ImageSaveResult.Success success && !string.IsNullOrEmpty(success.FileName)
            ? success.FileName
            : null;

        _context.Libros.Add(libro);
        _context.SaveChanges();

        TempData["Success"] = $"El libro \"{libro.Titulo}\" se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var libro = _context.Libros.Find(id);

        if (libro is null) return NotFound();
        return View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Libro libro, IFormFile? image)
    {
        if (id != libro.Id) return BadRequest();
        if (!ModelState.IsValid) return View(libro);

        var current = _context.Libros.Find(id);
        if (current is null) return NotFound();

        var previousImage = current.ImagenPath;

        _context.Entry(current).State = EntityState.Detached;

        var savedFile = TrySaveImage(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            ModelState.AddModelError(nameof(Libro.ImagenPath), failure.Message);
            return View(libro);
        }

        var newImageFileName = (savedFile as ImageSaveResult.Success)?.FileName;
        var hasNewImage = !string.IsNullOrEmpty(newImageFileName);

        libro.ImagenPath = hasNewImage ? newImageFileName : previousImage;

        _context.Libros.Update(libro);
        _context.SaveChanges();

        if (hasNewImage)
        {
            DeleteImageFile(previousImage);
        }

        TempData["Success"] = $"El libro \"{libro.Titulo}\" se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var libro = _context.Libros
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == id);

        if (libro is null) return NotFound();
        return View(libro);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var libro = _context.Libros.Find(id);
        if (libro is null) return NotFound();

        DeleteImageFile(libro.ImagenPath);
        _context.Libros.Remove(libro);
        _context.SaveChanges();

        TempData["Success"] = $"El libro \"{libro.Titulo}\" se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private ImageSaveResult TrySaveImage(IFormFile? image)
    {
        if (image is null || image.Length == 0) return new ImageSaveResult.Success(string.Empty);

        if (image.Length > MaxFileSize)
            return new ImageSaveResult.Failure("La imagen no puede superar los 2 MB.");

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return new ImageSaveResult.Failure("Formato no permitido. Use jpg, jpeg, png, gif o webp.");

        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(_env.WebRootPath, "images", fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.CopyTo(stream);
        }

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
