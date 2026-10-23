using System.Security.Claims;

namespace Biblioteca.Models;

// Nombres de los roles de la aplicación. Se centralizan para no repetir cadenas literales.
public static class Roles
{
    public const string Admin = "Admin";

    public const string Bibliotecario = "Bibliotecario";

    public const string Usuario = "Usuario";

    public const string Gestion = Admin + "," + Bibliotecario;

    public static readonly string[] Todos = { Admin, Bibliotecario, Usuario };

    public static bool PuedeGestionar(ClaimsPrincipal? user) =>
        user is not null && (user.IsInRole(Admin) || user.IsInRole(Bibliotecario));
}
