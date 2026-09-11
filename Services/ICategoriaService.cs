using Biblioteca.Models;

namespace Biblioteca.Services;

public interface ICategoriaService
{
    IEnumerable<Categoria> GetAll();
    Categoria? GetById(int id);
    void Create(Categoria categoria);
    bool Update(int id, Categoria categoria);
    bool Delete(int id);
}
