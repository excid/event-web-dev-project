using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

public class LoginController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    // GET /Login/Index  →  shows the login form
    public IActionResult Index()
    {
        // If already logged in, skip the login page
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View("~/Views/Login/Login.cshtml");
    }

    // POST /Login/Index  →  processes the form submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        // Checks all [Required] and [EmailAddress] rules first
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

        // Wrong email or password
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View("~/Views/Login/Login.cshtml", model);
    }

    // POST /Login/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Login");
    }
}
