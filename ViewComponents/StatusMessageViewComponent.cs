using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Biblioteca.ViewComponents;

/// <summary>
/// Componente de vista reutilizable que renderiza el panel visual de estado
/// (error general, acceso denegado, etc.).
///
/// La lógica de presentación vive en <see cref="StatusMessageFactory"/> y el
/// markup en Views/Shared/Components/StatusMessage/Default.cshtml, de modo que
/// ambas páginas comparten el mismo diseño sin duplicar código.
/// </summary>
public class StatusMessageViewComponent : ViewComponent
{
    public ViewViewComponentResult Invoke(StatusMessageViewModel model)
    {
        return View(model);
    }
}
