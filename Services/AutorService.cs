using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services;

public class AutorService : IAutorService
{
    private readonly BibliotecaDbContext _context;

    public AutorService(BibliotecaDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Author> GetAll()
    {
        return _context.Autores
            .AsNoTracking()
            .OrderBy(a => a.Surname)
            .ThenBy(a => a.Name)
            .ToList();
    }

    public void Add(Author author)
    {
        _context.Autores.Add(author);
        _context.SaveChanges();
    }
}
