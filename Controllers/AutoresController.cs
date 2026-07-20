using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class AutoresController : Controller
{
    private static List<Author> _authors = new()
    {
        new Author { ID = 1, Name = "Gabriel",    Surname = "García Márquez", Nationality = "Colombiana",     BirthDate = new DateOnly(1927, 3, 6),  IsActive = false },
        new Author { ID = 2, Name = "Isabel",     Surname = "Allende",        Nationality = "Chilena",        BirthDate = new DateOnly(1942, 8, 2),  IsActive = true  },
        new Author { ID = 3, Name = "Jorge Luis", Surname = "Borges",         Nationality = "Argentina",      BirthDate = new DateOnly(1899, 8, 24), IsActive = false },
        new Author { ID = 4, Name = "Julio",      Surname = "Cortázar",       Nationality = "Belga-Argentino",BirthDate = new DateOnly(1914, 8, 26), IsActive = false },
        new Author { ID = 5, Name = "Mario",      Surname = "Vargas Llosa",   Nationality = "Peruana",        BirthDate = new DateOnly(1936, 3, 28), IsActive = true  },
        new Author { ID = 6, Name = "Eduardo",    Surname = "Galeano",        Nationality = "Uruguaya",       BirthDate = new DateOnly(1940, 9, 3),  IsActive = false },
    };

    public IActionResult Index()
    {
        return View(_authors);
    }
}
