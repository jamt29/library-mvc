using Biblioteca.Data;
using Biblioteca.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registro del DbContext de EF Core con Npgsql (PostgreSQL).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<BibliotecaDbContext>(options => options.UseNpgsql(connectionString));

// ASP.NET Core Identity: usuarios e inicio/cierre de sesión sobre EF Core.
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<BibliotecaDbContext>()
    .AddDefaultTokenProviders();

// Autorización: las cookies de Identity redirigen al login y a la página de acceso denegado.
builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Registro de dependencia IAutorService con su implementación AutorService en ciclo de vida Scoped.
builder.Services.AddScoped<IAutorService, AutorService>();

// Registro de ICategoriaService con CategoriaService (ADO.NET con PostgreSQL)
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// Para intercambiar la implementación sin tocar AutoresController, simplemente se cambia el registro a:
// builder.Services.AddScoped<IAutorService, AlternativeAutorService>();

// ---------------------------------------------------------------------------
// Logging: consola con marca de tiempo (salida de Visual Studio / consola)
// y depuración. Los niveles por categoría se ajustan en appsettings.json.
// ---------------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.FormatterName = ConsoleFormatterNames.Simple);
builder.Services.Configure<ConsoleFormatterOptions>(options =>
{
    options.TimestampFormat = "HH:mm:ss ";
});
builder.Logging.AddDebug();

// El núcleo de ASP.NET Core es muy verboso: se deja en Warning para que la
// salida del proyecto muestre sobre todo los eventos de la aplicación.
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var app = builder.Build();

// Aplica las migraciones pendientes y siembra los datos iniciales.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
    DbInitializer.Initialize(context);

    // Usuarios de prueba idempotentes para poder iniciar sesión sin registrarse.
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


    await DbInitializer.MigrateLegacyAdminRoleAsync(roleManager, userManager);

    await DbInitializer.SeedRolesAsync(roleManager);

    await DbInitializer.SeedAdminUserAsync(userManager, app.Environment);
    await DbInitializer.SeedBibliotecarioUserAsync(userManager, app.Environment);

    await DbInitializer.EnsureAdminRoleAsync(userManager);
    await DbInitializer.EnsureBibliotecarioRoleAsync(userManager);

    await DbInitializer.AssignDefaultRoleAsync(userManager);
    await DbInitializer.VerifyUsersHaveRoleAsync(userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Página de excepciones detallada para depurar en local.
    // El resto de los entornos muestran la página de error personalizada.
    app.UseDeveloperExceptionPage();
}
else
{
    // Manejo centralizado de errores no controlados: se vuelve a ejecutar la
    // ruta /Home/Error, que muestra un mensaje general sin filtrar detalles
    // técnicos (trazas, rutas internas o cadenas de conexión).
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Errores de estado (por ejemplo, 404) vuelven a ejecutar la página de error
// personalizada en lugar de mostrar una respuesta vacía.
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
