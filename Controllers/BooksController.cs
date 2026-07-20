using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class BooksController : Controller
{
    private static List<Book> _books = new()
    {
        new Book { ID = 1, Name = "Cien años de soledad",       Author = "Gabriel García Márquez", Category = "Realismo mágico",  Price = 25.50m, IsAvailable = true  },
        new Book { ID = 2, Name = "Rayuela",                    Author = "Julio Cortázar",         Category = "Novela",          Price = 18.00m, IsAvailable = true  },
        new Book { ID = 3, Name = "El túnel",                   Author = "Ernesto Sabato",         Category = "Novela psicológica", Price = 12.99m, IsAvailable = false },
        new Book { ID = 4, Name = "Ficciones",                  Author = "Jorge Luis Borges",      Category = "Cuentos",         Price = 15.75m, IsAvailable = true  },
    };

    public IActionResult Index()
    {
        return View(_books);
    }
}
