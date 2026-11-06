using System.ComponentModel.DataAnnotations;
using Biblioteca.Models;
using Xunit;

namespace Biblioteca.Tests.Libros;

public class LibroValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void Libro_ConCamposValidos_SuperaValidacion()
    {
        // Arrange
        var libro = new Libro
        {
            Titulo = "El Principito",
            Autor = "Antoine de Saint-Exupéry",
            Categoria = "Literatura Infantil",
            Precio = 12.50m,
            Disponible = true
        };

        // Act
        var results = ValidateModel(libro);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Libro_SinTitulo_FallaValidacionPorCampoRequerido()
    {
        // Arrange
        var libro = new Libro
        {
            Titulo = "",
            Autor = "Autor Válido",
            Categoria = "Categoría Válida",
            Precio = 10.00m
        };

        // Act
        var results = ValidateModel(libro);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Libro.Titulo)));
        Assert.Equal("El título es obligatorio.", results.First(r => r.MemberNames.Contains(nameof(Libro.Titulo))).ErrorMessage);
    }

    [Fact]
    public void Libro_SinAutor_FallaValidacionPorCampoRequerido()
    {
        // Arrange
        var libro = new Libro
        {
            Titulo = "Título Válido",
            Autor = "",
            Categoria = "Categoría Válida",
            Precio = 10.00m
        };

        // Act
        var results = ValidateModel(libro);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Libro.Autor)));
    }

    [Fact]
    public void Libro_SinCategoria_FallaValidacionPorCampoRequerido()
    {
        // Arrange
        var libro = new Libro
        {
            Titulo = "Título Válido",
            Autor = "Autor Válido",
            Categoria = "",
            Precio = 10.00m
        };

        // Act
        var results = ValidateModel(libro);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Libro.Categoria)));
    }

    [Theory]
    [InlineData("0.00")]
    [InlineData("-5.00")]
    [InlineData("10000.00")]
    public void Libro_ConPrecioFueraDeRango_FallaValidacion(string precioString)
    {
        // Arrange
        var precio = decimal.Parse(precioString, System.Globalization.CultureInfo.InvariantCulture);
        var libro = new Libro
        {
            Titulo = "Título Válido",
            Autor = "Autor Válido",
            Categoria = "Categoría Válida",
            Precio = precio
        };

        // Act
        var results = ValidateModel(libro);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Libro.Precio)));
    }

    [Fact]
    public void Libro_ConTituloExcesivamenteLargo_FallaPorStringLength()
    {
        // Arrange
        var libro = new Libro
        {
            Titulo = new string('A', 151), // Max 150
            Autor = "Autor",
            Categoria = "Cat",
            Precio = 10m
        };

        // Act
        var results = ValidateModel(libro);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Libro.Titulo)));
    }
}
