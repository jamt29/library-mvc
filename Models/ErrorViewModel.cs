namespace Biblioteca.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    /// <summary>Código HTTP que originó la página de error (500, 404, etc.).</summary>
    public int? StatusCode { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
