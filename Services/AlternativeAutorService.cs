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

    public Author? GetById(int id)
    {
        return _authors.FirstOrDefault(a => a.ID == id);
    }

    public bool Update(int id, Author author)
    {
        var existing = GetById(id);
        if (existing is null) return false;

        existing.Name        = author.Name;
        existing.Surname     = author.Surname;
        existing.Nationality = author.Nationality;
        existing.BirthDate   = author.BirthDate;
        existing.IsActive    = author.IsActive;

        return true;
    }

    public bool Delete(int id)
    {
        var author = GetById(id);
        if (author is null) return false;

        return _authors.Remove(author);
    }
}
