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

    public async Task<IActionResult> Index()
    {
        var libros = await _context.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToListAsync();

        return View(libros);
    }

    public async Task<IActionResult> Details(int id)
    {
        var libro = await _context.Libros
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

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
    public async Task<IActionResult> Create(Libro libro, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(libro);

        var savedFile = await TrySaveImageAsync(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            ModelState.AddModelError(nameof(Libro.ImagenPath), failure.Message);
            return View(libro);
        }

        // Store null instead of an empty string when no image was uploaded.
        libro.ImagenPath = savedFile is ImageSaveResult.Success success && !string.IsNullOrEmpty(success.FileName)
            ? success.FileName
            : null;

        _context.Libros.Add(libro);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"El libro \"{libro.Titulo}\" se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ImageSaveResult> TrySaveImageAsync(IFormFile? image)
    {
        if (image is null || image.Length == 0) return new ImageSaveResult.Success(string.Empty);

        if (image.Length > MaxFileSize)
            return new ImageSaveResult.Failure("La imagen no puede superar los 2 MB.");

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return new ImageSaveResult.Failure("Formato no permitido. Use jpg, jpeg, png, gif o webp.");

        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(_env.WebRootPath, "images", fileName);

        await using (var stream = new FileStream(path, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return new ImageSaveResult.Success(fileName);
    }

    private abstract record ImageSaveResult
    {
        public sealed record Success(string FileName) : ImageSaveResult;
        public sealed record Failure(string Message) : ImageSaveResult;
    }
}
