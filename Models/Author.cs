using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Author
{
    public int ID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
    [Display(Name = "Apellido")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nacionalidad es obligatoria.")]
    [StringLength(60, ErrorMessage = "La nacionalidad no puede superar los 60 caracteres.")]
    [Display(Name = "Nacionalidad")]
    public string Nationality { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Nacimiento")]
    public DateOnly BirthDate { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;
}
