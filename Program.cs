using Biblioteca.Data;
using Biblioteca.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registro del DbContext de EF Core con Npgsql (PostgreSQL).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<BibliotecaDbContext>(options => options.UseNpgsql(connectionString));

//Registro de dependencia IAutorService con su implementación AutorService en ciclo de vida Scoped.
builder.Services.AddScoped<IAutorService, AutorService>();

// Registro de ICategoriaService con CategoriaService (ADO.NET con PostgreSQL)
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// Para intercambiar la implementación sin tocar AutoresController, simplemente se cambia el registro a:
// builder.Services.AddScoped<IAutorService, AlternativeAutorService>();

var app = builder.Build();

// Aplica las migraciones pendientes y siembra los datos iniciales de libros.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
    DbInitializer.Initialize(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
