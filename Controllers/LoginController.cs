using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using event_web_dev_project.Models;
using event_web_dev_project.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace event_web_dev_project.Controllers;

public class LoginController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public LoginController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
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
            ProfileSlug = await GenerateUniqueProfileSlugAsync(model.DisplayName),
            CreatedAt   = DateTime.Now
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

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a display name to a URL-safe slug and ensures it is unique
    /// across all existing profile slugs.
    /// </summary>
    private async Task<string> GenerateUniqueProfileSlugAsync(string displayName)
    {
        // Lowercase, replace spaces/special chars with underscores, collapse duplicates
        var baseSlug = Regex.Replace(displayName.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "user";

        var slug = baseSlug;
        int suffix = 1;
        while (await _context.Users.AnyAsync(u => u.ProfileSlug == slug))
            slug = baseSlug + suffix++;

        return slug;
    }
}
