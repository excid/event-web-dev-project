using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

public class LoginController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // ─── Login ───────────────────────────────────────────────────────────────

    // GET /Login/Index
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View("~/Views/Login/Login.cshtml");
    }

    // POST /Login/Index
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Login/Login.cshtml", model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View("~/Views/Login/Login.cshtml", model);
    }

    // ─── Register ────────────────────────────────────────────────────────────

    // GET /Login/Register
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View("~/Views/Login/Register.cshtml");
    }

    // POST /Login/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Login/Register.cshtml", model);

        var user = new ApplicationUser
        {
            UserName    = model.Email,
            Email       = model.Email,
            DisplayName = model.DisplayName,
            CreatedAt   = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Auto-login after registration
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        // Show Identity errors (e.g. "Email already taken", "Password too weak")
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View("~/Views/Login/Register.cshtml", model);
    }

    // ─── Logout ──────────────────────────────────────────────────────────────

    // POST /Login/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Login");
    }
}
