using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class BooksController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 2 * 1024 * 1024;

    private static List<Book> _books = new()
    {
        new Book { ID = 1, Name = "Cien años de soledad",       Author = "Gabriel García Márquez", Category = "Realismo mágico",    Price = 25.50m, IsAvailable = true  },
        new Book { ID = 2, Name = "Rayuela",                    Author = "Julio Cortázar",         Category = "Novela",            Price = 18.00m, IsAvailable = true  },
        new Book { ID = 3, Name = "El túnel",                   Author = "Ernesto Sabato",         Category = "Novela psicológica", Price = 12.99m, IsAvailable = false },
        new Book { ID = 4, Name = "Ficciones",                  Author = "Jorge Luis Borges",      Category = "Cuentos",           Price = 15.75m, IsAvailable = true  },
    };

    private readonly IWebHostEnvironment _env;

    public BooksController(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IActionResult Index()
    {
        return View(_books);
    }

    public IActionResult Details(int id)
    {
        var book = _books.FirstOrDefault(b => b.ID == id);
        if (book is null) return NotFound();
        return View(book);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Book book, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(book);

        var savedFile = await TrySaveImageAsync(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            ModelState.AddModelError(nameof(Book.ImagePath), failure.Message);
            return View(book);
        }

        book.ImagePath = (savedFile as ImageSaveResult.Success)?.FileName;
        book.ID = _books.Count == 0 ? 1 : _books.Max(b => b.ID) + 1;
        _books.Add(book);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var book = _books.FirstOrDefault(b => b.ID == id);
        if (book is null) return NotFound();
        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Book book, IFormFile? image)
    {
        if (id != book.ID) return BadRequest();
        if (!ModelState.IsValid) return View(book);

        var existing = _books.FirstOrDefault(b => b.ID == id);
        if (existing is null) return NotFound();

        var savedFile = await TrySaveImageAsync(image);
        if (savedFile is ImageSaveResult.Failure failure)
        {
            ModelState.AddModelError(nameof(Book.ImagePath), failure.Message);
            return View(book);
        }

        if (savedFile is ImageSaveResult.Success success)
        {
            DeleteImageFile(existing.ImagePath);
            existing.ImagePath = success.FileName;
        }

        existing.Name        = book.Name;
        existing.Author      = book.Author;
        existing.Category    = book.Category;
        existing.Price       = book.Price;
        existing.IsAvailable = book.IsAvailable;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var book = _books.FirstOrDefault(b => b.ID == id);
        if (book is null) return NotFound();
        return View(book);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var book = _books.FirstOrDefault(b => b.ID == id);
        if (book is null) return NotFound();

        DeleteImageFile(book.ImagePath);
        _books.Remove(book);
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

    private void DeleteImageFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;
        var path = Path.Combine(_env.WebRootPath, "images", fileName);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private abstract record ImageSaveResult
    {
        public sealed record Success(string FileName) : ImageSaveResult;
        public sealed record Failure(string Message) : ImageSaveResult;
    }
}
