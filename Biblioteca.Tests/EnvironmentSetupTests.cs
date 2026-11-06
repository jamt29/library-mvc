using Xunit;

namespace Biblioteca.Tests;

public class EnvironmentSetupTests
{
    [Fact]
    public void EntornoDePruebas_ConfiguradoCorrectamente_DeberiaPasar()
    {
        // Arrange
        var esperado = true;

        // Act
        var resultado = true;

        // Assert
        Assert.True(resultado);
        Assert.Equal(esperado, resultado);
    }
}
