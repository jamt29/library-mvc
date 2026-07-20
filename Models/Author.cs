namespace Biblioteca.Models;

public class Author
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public bool IsActive { get; set; }
}
