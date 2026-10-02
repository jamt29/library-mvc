using Biblioteca.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public static class DbInitializer
{
    public const string AdminUserName = "admin";
    public const string AdminEmail = "admin@biblioteca.local";
    public const string AdminPassword = "Admin2026";

    public static void Initialize(BibliotecaDbContext context)
    {
        context.Database.Migrate();
        SeedLibros(context);
        SeedAutores(context);
    }

    // Crea el usuario de pruebas de forma idempotente: si ya existe, no hace nada.
    public static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
    {
        if (await userManager.FindByNameAsync(AdminUserName) is not null)
        {
            return;
        }

        // Si el correo ya pertenece a otra cuenta se omite, para no romper el arranque.
        if (await userManager.FindByEmailAsync(AdminEmail) is not null)
        {
            return;
        }

        var admin = new IdentityUser
        {
            UserName = AdminUserName,
            Email = AdminEmail,
            // No se implementa confirmación por correo electrónico.
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, AdminPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"No se pudo crear el usuario de pruebas '{AdminUserName}': {errors}");
        }
    }

    private static void SeedLibros(BibliotecaDbContext context)
    {
        if (context.Libros.Any())
        {
            return;
        }

        var libros = new List<Libro>
        {
            new() { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Realismo mágico", Precio = 25.50m, Disponible = true },
            new() { Titulo = "Rayuela", Autor = "Julio Cortázar", Categoria = "Novela", Precio = 18.00m, Disponible = true },
            new() { Titulo = "El túnel", Autor = "Ernesto Sabato", Categoria = "Novela psicológica", Precio = 12.99m, Disponible = false },
            new() { Titulo = "Ficciones", Autor = "Jorge Luis Borges", Categoria = "Cuentos", Precio = 15.75m, Disponible = true },
            new() { Titulo = "La casa de los espíritus", Autor = "Isabel Allende", Categoria = "Realismo mágico", Precio = 21.40m, Disponible = true },
            new() { Titulo = "Pedro Páramo", Autor = "Juan Rulfo", Categoria = "Novela", Precio = 14.25m, Disponible = false },
        };

        context.Libros.AddRange(libros);
        context.SaveChanges();
    }

    private static void SeedAutores(BibliotecaDbContext context)
    {
        if (context.Autores.Any())
        {
            return;
        }

        var autores = new List<Author>
        {
            new() { Name = "Gabriel",    Surname = "García Márquez", Nationality = "Colombiana",      BirthDate = new DateOnly(1927, 3, 6),  IsActive = false },
            new() { Name = "Isabel",     Surname = "Allende",        Nationality = "Chilena",         BirthDate = new DateOnly(1942, 8, 2),  IsActive = true  },
            new() { Name = "Jorge Luis", Surname = "Borges",         Nationality = "Argentina",       BirthDate = new DateOnly(1899, 8, 24), IsActive = false },
            new() { Name = "Julio",      Surname = "Cortázar",       Nationality = "Belga-Argentino", BirthDate = new DateOnly(1914, 8, 26), IsActive = false },
            new() { Name = "Mario",      Surname = "Vargas Llosa",   Nationality = "Peruana",         BirthDate = new DateOnly(1936, 3, 28), IsActive = true  },
            new() { Name = "Eduardo",    Surname = "Galeano",        Nationality = "Uruguaya",        BirthDate = new DateOnly(1940, 9, 3),  IsActive = false },
        };

        context.Autores.AddRange(autores);
        context.SaveChanges();
    }
}
