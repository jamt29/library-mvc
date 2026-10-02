using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

// Datos que envía el formulario de inicio de sesión.
public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu usuario o correo electrónico.")]
    [Display(Name = "Usuario o correo electrónico")]
    public string UsuarioOCorreo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }
}
