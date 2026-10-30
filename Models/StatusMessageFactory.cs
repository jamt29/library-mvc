using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Models;

/// <summary>
/// Fábrica de mensajes de estado reutilizables.
///
/// Centraliza el texto y las acciones de las páginas de error y de acceso
/// denegado, de modo que ambas vistas compartan la misma lógica de presentación
/// sin duplicar contenido ni exponer detalles técnicos al usuario.
/// </summary>
public static class StatusMessageFactory
{
    /// <summary>
    /// Construye el mensaje para una página de error general (500, 404, etc.).
    /// </summary>
    public static StatusMessageViewModel ForHttpStatus(int? statusCode, string? requestId, IUrlHelper url)
    {
        var code = statusCode ?? 500;

        var model = code switch
        {
            404 => new StatusMessageViewModel
            {
                Title = "No encontramos esa página",
                Message = "La dirección que intentaste abrir no existe, cambió de lugar o el recurso ya no está disponible.",
                Detail = "Revisá el enlace o volvé al inicio para seguir explorando el catálogo de la biblioteca.",
                Icon = "search",
                Tone = "warning",
                Badge = "Error 404"
            },
            403 => new StatusMessageViewModel
            {
                Title = "Acceso restringido",
                Message = "Tu sesión actual no tiene permisos para consultar este recurso.",
                Detail = "Podés seguir usando las secciones disponibles para tu rol o iniciar sesión con otra cuenta.",
                Icon = "lock",
                Tone = "danger",
                Badge = "Error 403"
            },
            400 => new StatusMessageViewModel
            {
                Title = "Solicitud no válida",
                Message = "No pudimos interpretar los datos enviados en la petición.",
                Detail = "Volvé atrás, revisá el formulario y enviá la información nuevamente.",
                Icon = "alert",
                Tone = "warning",
                Badge = "Error 400"
            },
            _ => new StatusMessageViewModel
            {
                Title = "Ocurrió un problema inesperado",
                Message = "Se produjo una falla al procesar tu solicitud. Ya estamos revisando el incidente para resolverlo.",
                Detail = "Intentá nuevamente en unos instantes. Si el problema continúa, comunicate con el personal de la biblioteca.",
                Icon = "alert",
                Tone = "danger",
                Badge = "Error 500"
            }
        };

        model.StatusCode = code;
        model.RequestId = requestId;

        model.Actions.Add(new StatusActionViewModel
        {
            Label = "Ir al inicio",
            Url = url.Action("Index", "Home") ?? "/",
            Style = "solid"
        });

        model.Actions.Add(new StatusActionViewModel
        {
            Label = "Ver el catálogo",
            Url = url.Action("Index", "Libros") ?? "/Libros",
            Style = "outline"
        });

        model.Actions.Add(new StatusActionViewModel
        {
            Label = "Volver a intentar",
            Url = url.Action("Index", "Home") ?? "/",
            Style = "ghost"
        });

        return model;
    }

    /// <summary>
    /// Construye el mensaje de la página de acceso denegado.
    /// </summary>
    public static StatusMessageViewModel ForAccessDenied(bool autenticado, string? rol, IUrlHelper url, string? recurso = null)
    {
        var detalle = autenticado
            ? $"Iniciaste sesión como \"{rol ?? "Usuario"}\". Las operaciones de gestión están reservadas para los roles Admin y Bibliotecario; mientras tanto podés seguir consultando el catálogo."
            : "Si ya tenés una cuenta, iniciá sesión. Si todavía no la tenés, podés registrarte en pocos segundos.";

        if (!string.IsNullOrEmpty(recurso))
        {
            detalle = $"La sección que intentaste abrir ({recurso}) está reservada para otros roles. {detalle}";
        }

        var model = new StatusMessageViewModel
        {
            Title = "Acceso denegado",
            Message = autenticado
                ? "Tu cuenta no tiene los permisos necesarios para realizar esta operación."
                : "Necesitás iniciar sesión con una cuenta que tenga permisos para realizar esta operación.",
            Detail = detalle,
            Icon = "shield",
            Tone = "danger",
            Badge = "Permisos insuficientes"
        };

        model.Actions.Add(new StatusActionViewModel
        {
            Label = "Ir al catálogo",
            Url = url.Action("Index", "Libros") ?? "/Libros",
            Style = "solid"
        });

        model.Actions.Add(new StatusActionViewModel
        {
            Label = "Volver al inicio",
            Url = url.Action("Index", "Home") ?? "/",
            Style = "outline"
        });

        if (!autenticado)
        {
            model.Actions.Add(new StatusActionViewModel
            {
                Label = "Iniciar sesión",
                Url = url.Action("Login", "Account") ?? "/Account/Login",
                Style = "ghost"
            });
        }

        return model;
    }
}
