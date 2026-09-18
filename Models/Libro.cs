using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Libro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El autor no puede superar los 100 caracteres.")]
    [Display(Name = "Autor")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(80, ErrorMessage = "La categoría no puede superar los 80 caracteres.")]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99", ErrorMessage = "El precio debe estar entre 0.01 y 9999.99.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Display(Name = "Disponible")]
    public bool Disponible { get; set; } = true;

    [Display(Name = "Imagen")]
    public string? ImagenPath { get; set; }
}
