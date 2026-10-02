using Biblioteca.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

// Controlador de autenticación: inicio de sesión, registro y cierre de sesión.
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si ya hay una sesión activa no tiene sentido mostrar el formulario.
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // Se permite iniciar sesión con el nombre de usuario o con el correo electrónico.
        var user = await _userManager.FindByNameAsync(model.UsuarioOCorreo)
            ?? await _userManager.FindByEmailAsync(model.UsuarioOCorreo);

        if (user is null)
        {
            // Mensaje genérico: no se revela si el usuario existe o no.
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Contrasena, model.Recordarme, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Bienvenido, {user.UserName}.";

            // Solo se redirige a URLs locales para evitar redirecciones abiertas.
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Usuario,
            Email = model.Correo,
            // No se implementa confirmación por correo electrónico.
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Contrasena);

        if (result.Succeeded)
        {
            // No se inicia sesión automáticamente: el usuario debe autenticarse.
            TempData["Success"] = "Tu cuenta se creó correctamente. Ya podés iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        // Los errores de Identity se traducen para que el mensaje sea determinista en español.
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Cerraste sesión correctamente.";
        return RedirectToAction("Index", "Home");
    }

    // Traduce los códigos de error de Identity a mensajes en español.
    private static string TranslateIdentityError(IdentityError error) => error.Code switch
    {
        nameof(IdentityErrorDescriber.DuplicateUserName) => "Ese nombre de usuario ya está registrado.",
        nameof(IdentityErrorDescriber.DuplicateEmail) => "Ese correo electrónico ya está registrado.",
        nameof(IdentityErrorDescriber.InvalidUserName) => "El nombre de usuario contiene caracteres no permitidos.",
        nameof(IdentityErrorDescriber.InvalidEmail) => "El correo electrónico no es válido.",
        nameof(IdentityErrorDescriber.PasswordTooShort) => "La contraseña es demasiado corta.",
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) => "La contraseña debe incluir al menos un número.",
        nameof(IdentityErrorDescriber.PasswordRequiresLower) => "La contraseña debe incluir al menos una letra minúscula.",
        nameof(IdentityErrorDescriber.PasswordRequiresUpper) => "La contraseña debe incluir al menos una letra mayúscula.",
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => "La contraseña debe incluir al menos un carácter especial.",
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => "La contraseña debe incluir caracteres variados.",
        _ => "No fue posible completar el registro. Verificá los datos ingresados."
    };
}
