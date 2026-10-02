using Biblioteca.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

// El contexto hereda de IdentityDbContext para incluir las tablas de usuarios y roles de Identity.
public class BibliotecaDbContext : IdentityDbContext<IdentityUser>
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : base(options) { }

    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Author> Autores => Set<Author>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primero se configura el modelo de Identity; si no se llama a base, sus tablas no se crean.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.ToTable("libros");
            entity.Property(l => l.Id).HasColumnName("id");
            entity.Property(l => l.Titulo).HasColumnName("titulo").HasMaxLength(150).IsRequired();
            entity.Property(l => l.Autor).HasColumnName("autor").HasMaxLength(100).IsRequired();
            entity.Property(l => l.Categoria).HasColumnName("categoria").HasMaxLength(80).IsRequired();
            entity.Property(l => l.Precio).HasColumnName("precio").HasPrecision(10, 2);
            entity.Property(l => l.Disponible).HasColumnName("disponible");
            entity.Property(l => l.ImagenPath).HasColumnName("imagen_path").HasMaxLength(255);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("autores");
            entity.Property(a => a.ID).HasColumnName("id");
            entity.Property(a => a.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
            entity.Property(a => a.Surname).HasColumnName("surname").HasMaxLength(80).IsRequired();
            entity.Property(a => a.Nationality).HasColumnName("nationality").HasMaxLength(60).IsRequired();
            entity.Property(a => a.BirthDate).HasColumnName("birth_date");
            entity.Property(a => a.IsActive).HasColumnName("is_active");
        });
    }
}
