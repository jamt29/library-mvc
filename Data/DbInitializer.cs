using Biblioteca.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public static class DbInitializer
{
    public const string AdminUserName = "admin";
    public const string AdminEmail = "admin@biblioteca.local";
    public const string AdminPassword = "Admin2026!";

    public const string BibliotecarioUserName = "bibliotecario";
    public const string BibliotecarioEmail = "bibliotecario@biblioteca.local";
    public const string BibliotecarioPassword = "Bibliotecario2026!";

    public static void Initialize(BibliotecaDbContext context)
    {
        context.Database.Migrate();
        SeedLibros(context);
        SeedAutores(context);
    }

    public static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, IHostEnvironment environment)
    {
        var existing = await userManager.FindByNameAsync(AdminUserName);

        if (existing is not null)
        {
            if (environment.IsDevelopment())
            {
                await EnsureKnownPasswordAsync(userManager, existing, AdminPassword);
            }

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

    // Crea el usuario de pruebas con rol Bibliotecario, de forma idempotente.
    public static async Task SeedBibliotecarioUserAsync(UserManager<IdentityUser> userManager, IHostEnvironment environment)
    {
        var existing = await userManager.FindByNameAsync(BibliotecarioUserName);

        if (existing is not null)
        {
            if (environment.IsDevelopment())
            {
                await EnsureKnownPasswordAsync(userManager, existing, BibliotecarioPassword);
            }

            return;
        }

        // Si el correo ya pertenece a otra cuenta se omite, para no romper el arranque.
        if (await userManager.FindByEmailAsync(BibliotecarioEmail) is not null)
        {
            return;
        }

        var bibliotecario = new IdentityUser
        {
            UserName = BibliotecarioUserName,
            Email = BibliotecarioEmail,
            // No se implementa confirmación por correo electrónico.
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(bibliotecario, BibliotecarioPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"No se pudo crear el usuario de pruebas '{BibliotecarioUserName}': {errors}");
        }
    }

    private static async Task EnsureKnownPasswordAsync(UserManager<IdentityUser> userManager, IdentityUser user, string password)
    {
        if (await userManager.CheckPasswordAsync(user, password))
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"No se pudo restablecer la contraseña del usuario de pruebas '{user.UserName}': {errors}");
        }
    }

    // Migra el rol heredado "Administrador" al nuevo rol Admin, de forma idempotente.
    public static async Task MigrateLegacyAdminRoleAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        const string legacyRoleName = "Administrador";

        var legacyRole = await roleManager.FindByNameAsync(legacyRoleName);
        if (legacyRole is null)
        {
            return;
        }

        // Si el rol nuevo no existe, se renombra el heredado para conservar sus asignaciones.
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            legacyRole.Name = Roles.Admin;
            var renameResult = await roleManager.UpdateAsync(legacyRole);

            if (!renameResult.Succeeded)
            {
                var errors = string.Join("; ", renameResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"No se pudo renombrar el rol '{legacyRoleName}' a '{Roles.Admin}': {errors}");
            }

            return;
        }

        // Si ambos roles existen, se pasan los usuarios al rol nuevo y se elimina el heredado.
        var legacyUsers = await userManager.GetUsersInRoleAsync(legacyRoleName);
        foreach (var user in legacyUsers)
        {
            if (await userManager.IsInRoleAsync(user, Roles.Admin))
            {
                continue;
            }

            var addResult = await userManager.AddToRoleAsync(user, Roles.Admin);
            if (!addResult.Succeeded)
            {
                var errors = string.Join("; ", addResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"No se pudo asignar el rol '{Roles.Admin}' al usuario '{user.UserName}': {errors}");
            }
        }

        var deleteResult = await roleManager.DeleteAsync(legacyRole);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join("; ", deleteResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"No se pudo eliminar el rol heredado '{legacyRoleName}': {errors}");
        }
    }

    // Crea los roles de la aplicación de forma idempotente.
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in Roles.Todos)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"No se pudo crear el rol '{roleName}': {errors}");
            }
        }
    }

    // Garantiza que el usuario administrador tenga el rol Admin, aunque ya existiera antes de los roles.
    public static async Task EnsureAdminRoleAsync(UserManager<IdentityUser> userManager)
    {
        var admin = await userManager.FindByNameAsync(AdminUserName);
        if (admin is null)
        {
            return;
        }

        if (await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(admin, Roles.Admin);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"No se pudo asignar el rol '{Roles.Admin}' al usuario '{AdminUserName}': {errors}");
        }
    }

    // Garantiza que el usuario bibliotecario tenga el rol Bibliotecario.
    public static async Task EnsureBibliotecarioRoleAsync(UserManager<IdentityUser> userManager)
    {
        var bibliotecario = await userManager.FindByNameAsync(BibliotecarioUserName);
        if (bibliotecario is null)
        {
            return;
        }

        if (await userManager.IsInRoleAsync(bibliotecario, Roles.Bibliotecario))
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(bibliotecario, Roles.Bibliotecario);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"No se pudo asignar el rol '{Roles.Bibliotecario}' al usuario '{BibliotecarioUserName}': {errors}");
        }
    }

    // Asigna el rol Usuario a los usuarios existentes que todavía no tienen ningún rol.
    public static async Task AssignDefaultRoleAsync(UserManager<IdentityUser> userManager)
    {
        foreach (var user in userManager.Users.ToList())
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                continue;
            }

            var result = await userManager.AddToRoleAsync(user, Roles.Usuario);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"No se pudo asignar el rol '{Roles.Usuario}' al usuario '{user.UserName}': {errors}");
            }
        }
    }

    // Comprobación final de la siembra: todos los usuarios deben tener al menos un rol.
    public static async Task VerifyUsersHaveRoleAsync(UserManager<IdentityUser> userManager)
    {
        foreach (var user in userManager.Users.ToList())
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                throw new InvalidOperationException($"El usuario '{user.UserName}' quedó sin roles asignados.");
            }
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
