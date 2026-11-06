using Biblioteca.Models;

namespace Biblioteca.Services;

public interface ILibroService
{
    IEnumerable<Libro> GetAll();
    Libro? GetById(int id);
    void Create(Libro libro);
    bool Update(int id, Libro libro);
    bool Delete(int id);
}
