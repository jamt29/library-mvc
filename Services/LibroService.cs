using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services;

public class LibroService : ILibroService
{
    private readonly BibliotecaDbContext _context;

    public LibroService(BibliotecaDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Libro> GetAll()
    {
        return _context.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToList();
    }

    public Libro? GetById(int id)
    {
        return _context.Libros
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == id);
    }

    public void Create(Libro libro)
    {
        _context.Libros.Add(libro);
        _context.SaveChanges();
    }

    public bool Update(int id, Libro libro)
    {
        var existing = _context.Libros.Find(id);
        if (existing is null)
        {
            return false;
        }

        _context.Entry(existing).State = EntityState.Detached;
        libro.Id = id;
        _context.Libros.Update(libro);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _context.Libros.Find(id);
        if (existing is null)
        {
            return false;
        }

        _context.Libros.Remove(existing);
        _context.SaveChanges();
        return true;
    }
}
