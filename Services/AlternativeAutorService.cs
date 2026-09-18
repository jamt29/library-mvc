using Biblioteca.Models;

namespace Biblioteca.Services;

public class AlternativeAutorService : IAutorService
{
    private static readonly List<Author> _authors = new()
    {
        new Author { ID = 101, Name = "Adolfo",   Surname = "Bioy Casares",   Nationality = "Argentina", BirthDate = new DateOnly(1914, 9, 15), IsActive = true },
        new Author { ID = 102, Name = "Silvina",  Surname = "Ocampo",         Nationality = "Argentina", BirthDate = new DateOnly(1903, 7, 28), IsActive = true },
        new Author { ID = 103, Name = "Clarice",  Surname = "Lispector",      Nationality = "Brasileña", BirthDate = new DateOnly(1920, 12, 10), IsActive = true }
    };

    public IEnumerable<Author> GetAll()
    {
        return _authors;
    }

    public void Add(Author author)
    {
        author.ID = _authors.Count == 0 ? 1 : _authors.Max(a => a.ID) + 1;
        _authors.Add(author);
    }
}
