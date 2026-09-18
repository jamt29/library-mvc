using Biblioteca.Models;

namespace Biblioteca.Services;

public interface IAutorService
{
    IEnumerable<Author> GetAll();
    void Add(Author author);
}
