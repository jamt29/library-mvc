using Biblioteca.Models;

namespace Biblioteca.Services;

public interface IAutorService
{
    IEnumerable<Author> GetAll();
    Author? GetById(int id);
    bool Update(int id, Author author);
    bool Delete(int id);
}
