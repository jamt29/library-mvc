namespace Biblioteca.Models;

/// <summary>
/// Datos que alimentan el componente reutilizable de mensajes de estado
/// (páginas de error y de acceso denegado).
/// </summary>
public class StatusMessageViewModel
{
    /// <summary>Título principal del mensaje (por ejemplo: "Ocurrió un problema").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Explicación general, sin detalles técnicos internos.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Texto complementario opcional.</summary>
    public string? Detail { get; set; }

    /// <summary>Nombre del icono SVG a mostrar (partial _Icon).</summary>
    public string Icon { get; set; } = "alert";

    /// <summary>Tono visual: "danger", "warning" o "info".</summary>
    public string Tone { get; set; } = "danger";

    /// <summary>Etiqueta corta del encabezado (por ejemplo: "Error 500").</summary>
    public string? Badge { get; set; }

    /// <summary>Código HTTP asociado al mensaje, cuando corresponde.</summary>
    public int? StatusCode { get; set; }

    /// <summary>Identificador de la petición, útil para el soporte técnico.</summary>
    public string? RequestId { get; set; }

    /// <summary>Botones de navegación que se ofrecen al usuario.</summary>
    public List<StatusActionViewModel> Actions { get; set; } = new();
}

/// <summary>
/// Botón de navegación del mensaje de estado.
/// </summary>
public class StatusActionViewModel
{
    /// <summary>Texto visible del botón.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>URL de destino.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Estilo visual: "solid", "outline" o "ghost".</summary>
    public string Style { get; set; } = "outline";
}
