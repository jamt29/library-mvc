using Biblioteca.Controllers;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Biblioteca.Tests.Libros;

public class FailingIntentionalTestExample
{
    /// <summary>
    /// Actividad 5 (Reto): Demostración de prueba que falla intencionalmente al esperar una condición errónea,
    /// analizando el comportamiento de aserción y restaurándola a su condición correcta.
    /// </summary>
    [Fact]
    public void Reto_PruebaConFalloIntencional_AnalizadaYRestaurada()
    {
        // Arrange
        var mockService = new Mock<ILibroService>();
        var libros = new List<Libro>
        {
            new() { Id = 1, Titulo = "Libro 1", Autor = "Autor 1", Categoria = "Cat 1" },
            new() { Id = 2, Titulo = "Libro 2", Autor = "Autor 2", Categoria = "Cat 2" }
        };
        mockService.Setup(s => s.GetAll()).Returns(libros);

        // CONDICIÓN INTENCIONAL ERRÓNEA (para provocar fallo si se descomenta):
        // int conteoEsperadoErroneo = 5; // Fallaría porque la lista tiene 2 elementos:
        // Assert.Equal(conteoEsperadoErroneo, libros.Count);

        // CONDICIÓN CORRECTA RESTAURADA:
        int conteoEsperadoCorrecto = 2;
        Assert.Equal(conteoEsperadoCorrecto, libros.Count);
        Assert.NotEmpty(libros);
    }
}
