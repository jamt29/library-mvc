namespace Biblioteca.Models;

// Nombres de los roles de la aplicación. Se centralizan para no repetir cadenas literales.
public static class Roles
{
    // Acceso total: puede consultar el catálogo y también gestionarlo (agregar, editar y eliminar).
    public const string Administrador = "Administrador";

    // Acceso básico: puede consultar el catálogo, pero no gestionarlo.
    public const string Usuario = "Usuario";

    // Roles que la aplicación crea al iniciar.
    public static readonly string[] Todos = { Administrador, Usuario };
}
