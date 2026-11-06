using System.ComponentModel.DataAnnotations;
using Biblioteca.Controllers;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Biblioteca.Tests.Libros;

public class LibrosControllerTests
{
    private readonly Mock<ILibroService> _mockLibroService;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<ILogger<LibrosController>> _mockLogger;
    private readonly Mock<ITempDataDictionary> _mockTempData;

    public LibrosControllerTests()
    {
        _mockLibroService = new Mock<ILibroService>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<LibrosController>>();
        _mockTempData = new Mock<ITempDataDictionary>();
    }

    private LibrosController CreateController()
    {
        var controller = new LibrosController(_mockLibroService.Object, _mockEnv.Object, _mockLogger.Object)
        {
            TempData = _mockTempData.Object
        };
        return controller;
    }

    #region Actividad 2 & 3: Pruebas de Consulta (Index & Details)

    [Fact]
    public void Index_CuandoExistenLibros_DevuelveVistaConColeccionDeLibros()
    {
        // Arrange
        var librosEsperados = new List<Libro>
        {
            new() { Id = 1, Titulo = "Cien Años de Soledad", Autor = "Gabriel García Márquez", Categoria = "Novela", Precio = 25.50m, Disponible = true },
            new() { Id = 2, Titulo = "Don Quijote de la Mancha", Autor = "Miguel de Cervantes", Categoria = "Clásico", Precio = 19.99m, Disponible = true }
        };

        _mockLibroService.Setup(s => s.GetAll()).Returns(librosEsperados);
        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Libro>>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(2, model.Count());
        Assert.Equal(librosEsperados, model);
        _mockLibroService.Verify(s => s.GetAll(), Times.Once);
    }

    [Fact]
    public void Index_CuandoNoExistenLibros_DevuelveVistaConColeccionVacia()
    {
        // Arrange
        _mockLibroService.Setup(s => s.GetAll()).Returns(new List<Libro>());
        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Libro>>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Empty(model);
        _mockLibroService.Verify(s => s.GetAll(), Times.Once);
    }

    [Fact]
    public void Details_CuandoLibroExiste_DevuelveVistaConLibroCorrespondiente()
    {
        // Arrange
        var libroExistente = new Libro
        {
            Id = 10,
            Titulo = "El Aleph",
            Autor = "Jorge Luis Borges",
            Categoria = "Ficción",
            Precio = 15.00m,
            Disponible = true
        };

        _mockLibroService.Setup(s => s.GetById(10)).Returns(libroExistente);
        var controller = CreateController();

        // Act
        var result = controller.Details(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Libro>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(10, model.Id);
        Assert.Equal("El Aleph", model.Titulo);
        Assert.Equal("Jorge Luis Borges", model.Autor);
        Assert.True(model.Disponible);
        _mockLibroService.Verify(s => s.GetById(10), Times.Once);
    }

    [Fact]
    public void Details_CuandoLibroNoExiste_DevuelveNotFound()
    {
        // Arrange
        _mockLibroService.Setup(s => s.GetById(999)).Returns((Libro?)null);
        var controller = CreateController();

        // Act
        var result = controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockLibroService.Verify(s => s.GetById(999), Times.Once);
    }

    #endregion

    #region Actividad 2 & 3: Pruebas de Registro (Create)

    [Fact]
    public void Create_Get_DevuelveVistaPorDefecto()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Create();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Create_Post_ConDatosValidos_InvocaServicioYRedirigeAIndex()
    {
        // Arrange
        var nuevoLibro = new Libro
        {
            Titulo = "Rayuela",
            Autor = "Julio Cortázar",
            Categoria = "Novela",
            Precio = 22.00m,
            Disponible = true
        };

        var controller = CreateController();

        // Act
        var result = controller.Create(nuevoLibro, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibrosController.Index), redirectResult.ActionName);
        _mockLibroService.Verify(s => s.Create(It.Is<Libro>(l => l.Titulo == "Rayuela" && l.Autor == "Julio Cortázar")), Times.Once);
    }

    [Fact]
    public void Create_Post_ConModeloInvalido_NoInvocaServicioYDevuelveVista()
    {
        // Arrange
        var libroInvalido = new Libro
        {
            Titulo = "", // Inválido: Required
            Autor = "Autor Desconocido",
            Categoria = "General",
            Precio = 10.00m
        };

        var controller = CreateController();
        controller.ModelState.AddModelError("Titulo", "El título es obligatorio.");

        // Act
        var result = controller.Create(libroInvalido, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Same(libroInvalido, viewResult.Model);
        _mockLibroService.Verify(s => s.Create(It.IsAny<Libro>()), Times.Never);
    }

    #endregion

    #region Actividad 2 & 3: Pruebas de Edición (Edit)

    [Fact]
    public void Edit_Get_CuandoLibroExiste_DevuelveVistaConElLibro()
    {
        // Arrange
        var libroExistente = new Libro { Id = 5, Titulo = "Ficciones", Autor = "Jorge Luis Borges", Categoria = "Cuentos", Precio = 18.00m };
        _mockLibroService.Setup(s => s.GetById(5)).Returns(libroExistente);
        var controller = CreateController();

        // Act
        var result = controller.Edit(5);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Libro>(viewResult.Model);
        Assert.Equal(5, model.Id);
        _mockLibroService.Verify(s => s.GetById(5), Times.Once);
    }

    [Fact]
    public void Edit_Get_CuandoLibroNoExiste_DevuelveNotFound()
    {
        // Arrange
        _mockLibroService.Setup(s => s.GetById(99)).Returns((Libro?)null);
        var controller = CreateController();

        // Act
        var result = controller.Edit(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockLibroService.Verify(s => s.GetById(99), Times.Once);
    }

    [Fact]
    public void Edit_Post_CuandoIdNoCoincideConModelo_DevuelveBadRequest()
    {
        // Arrange
        var libro = new Libro { Id = 10, Titulo = "Pedro Páramo", Autor = "Juan Rulfo", Categoria = "Novela" };
        var controller = CreateController();

        // Act
        var result = controller.Edit(20, libro, null); // 20 != 10

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _mockLibroService.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public void Edit_Post_ConModeloInvalido_NoActualizaYDevuelveVista()
    {
        // Arrange
        var libro = new Libro { Id = 3, Titulo = "", Autor = "Autor", Categoria = "Cat" };
        var controller = CreateController();
        controller.ModelState.AddModelError("Titulo", "El título es obligatorio.");

        // Act
        var result = controller.Edit(3, libro, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Same(libro, viewResult.Model);
        _mockLibroService.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public void Edit_Post_CuandoLibroNoExisteEnServicio_DevuelveNotFound()
    {
        // Arrange
        var libro = new Libro { Id = 77, Titulo = "Libro Inexistente", Autor = "Autor", Categoria = "Cat", Precio = 10m };
        _mockLibroService.Setup(s => s.GetById(77)).Returns((Libro?)null);
        var controller = CreateController();

        // Act
        var result = controller.Edit(77, libro, null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockLibroService.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public void Edit_Post_ConDatosValidos_ActualizaYRedirigeAIndex()
    {
        // Arrange
        var libroExistente = new Libro { Id = 4, Titulo = "Título Original", Autor = "Autor", Categoria = "Cat", Precio = 10m };
        var libroActualizado = new Libro { Id = 4, Titulo = "Título Modificado", Autor = "Autor Modificado", Categoria = "Cat", Precio = 15m };

        _mockLibroService.Setup(s => s.GetById(4)).Returns(libroExistente);
        _mockLibroService.Setup(s => s.Update(4, It.IsAny<Libro>())).Returns(true);
        var controller = CreateController();

        // Act
        var result = controller.Edit(4, libroActualizado, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibrosController.Index), redirectResult.ActionName);
        _mockLibroService.Verify(s => s.Update(4, It.Is<Libro>(l => l.Titulo == "Título Modificado")), Times.Once);
    }

    #endregion

    #region Actividad 2 & 3: Pruebas de Eliminación (Delete)

    [Fact]
    public void Delete_Get_CuandoLibroExiste_DevuelveVistaConLibro()
    {
        // Arrange
        var libro = new Libro { Id = 8, Titulo = "Libro Para Borrar", Autor = "Autor", Categoria = "Cat" };
        _mockLibroService.Setup(s => s.GetById(8)).Returns(libro);
        var controller = CreateController();

        // Act
        var result = controller.Delete(8);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Libro>(viewResult.Model);
        Assert.Equal(8, model.Id);
        _mockLibroService.Verify(s => s.GetById(8), Times.Once);
    }

    [Fact]
    public void Delete_Get_CuandoLibroNoExiste_DevuelveNotFound()
    {
        // Arrange
        _mockLibroService.Setup(s => s.GetById(99)).Returns((Libro?)null);
        var controller = CreateController();

        // Act
        var result = controller.Delete(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockLibroService.Verify(s => s.GetById(99), Times.Once);
    }

    [Fact]
    public void DeleteConfirmed_CuandoLibroExiste_EliminaYRedirigeAIndex()
    {
        // Arrange
        var libro = new Libro { Id = 12, Titulo = "Libro a eliminar", Autor = "Autor", Categoria = "Cat" };
        _mockLibroService.Setup(s => s.GetById(12)).Returns(libro);
        _mockLibroService.Setup(s => s.Delete(12)).Returns(true);
        var controller = CreateController();

        // Act
        var result = controller.DeleteConfirmed(12);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibrosController.Index), redirectResult.ActionName);
        _mockLibroService.Verify(s => s.Delete(12), Times.Once);
    }

    [Fact]
    public void DeleteConfirmed_CuandoLibroNoExiste_DevuelveNotFound()
    {
        // Arrange
        _mockLibroService.Setup(s => s.GetById(99)).Returns((Libro?)null);
        var controller = CreateController();

        // Act
        var result = controller.DeleteConfirmed(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockLibroService.Verify(s => s.Delete(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Actividad 5 (Reto): Casos adicionales con [Theory] y validaciones

    [Theory]
    [InlineData(1, "Libro A", "Autor A", "Cat A", 10.50, true)]
    [InlineData(2, "Libro B", "Autor B", "Cat B", 99.99, false)]
    [InlineData(3, "Libro C", "Autor C", "Cat C", 0.01, true)]
    public void Details_ConVariosEscenariosDeDatos_DevuelveVistaCorrecta(int id, string titulo, string autor, string categoria, decimal precio, bool disponible)
    {
        // Arrange
        var libro = new Libro
        {
            Id = id,
            Titulo = titulo,
            Autor = autor,
            Categoria = categoria,
            Precio = precio,
            Disponible = disponible
        };

        _mockLibroService.Setup(s => s.GetById(id)).Returns(libro);
        var controller = CreateController();

        // Act
        var result = controller.Details(id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Libro>(viewResult.Model);
        Assert.Equal(id, model.Id);
        Assert.Equal(titulo, model.Titulo);
        Assert.Equal(autor, model.Autor);
        Assert.Equal(categoria, model.Categoria);
        Assert.Equal(precio, model.Precio);
        Assert.Equal(disponible, model.Disponible);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(99999)]
    public void Details_ConIdsInexistentes_SiempreDevuelveNotFound(int idInexistente)
    {
        // Arrange
        _mockLibroService.Setup(s => s.GetById(idInexistente)).Returns((Libro?)null);
        var controller = CreateController();

        // Act
        var result = controller.Details(idInexistente);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockLibroService.Verify(s => s.GetById(idInexistente), Times.Once);
    }

    #endregion
}
