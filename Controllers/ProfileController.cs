using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Data;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET /Profile/Index
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            user = await _context.Users.FirstOrDefaultAsync();
            if (user == null) return RedirectToAction("Index", "Home");
        }

        var viewModel = new ProfileViewModel
        {
            UserId      = user.Id,
            DisplayName = user.DisplayName ?? user.UserName ?? "Unknown User",
            Email       = user.Email ?? "",
            About       = user.About,
            AvatarUrl   = user.AvatarUrl,
            Tags        = user.Tags,
            Interests   = user.Interests,
            post_history = await _context.ActivityPosts
                .Where(p => p.OwnerId == user.Id && p.Status =="Closed")
                .OrderByDescending(p => p.PostedAt)
                .ToListAsync()
        };

        return View(viewModel);
    }

    // POST /Profile/UpdateProfile
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        user.DisplayName = model.DisplayName;
        user.About       = model.About;
        user.Tags        = model.Tags;
        user.Interests   = model.Interests;
        user.AvatarUrl   = model.AvatarUrl;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View("Index", model);
    }
}