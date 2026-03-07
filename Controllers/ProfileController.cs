using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Data;
using event_web_dev_project.Models;
using System.Security.Claims;

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
    // GET /Profile/Index?userId=<id>  — view another user's profile (read-only)
    [Authorize]
    public async Task<IActionResult> Index(string? userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        ApplicationUser? user;
        bool isOwner;

        if (string.IsNullOrEmpty(userId) || userId == currentUserId)
        {
            // Viewing own profile
            user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync();
                if (user == null) return RedirectToAction("Index", "Home");
            }
            isOwner = true;
        }
        else
        {
            // Viewing someone else's profile
            user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            isOwner = false;
        }

        var profileUserId = user.Id;

        var postHistory = await _context.ActivityPosts
            .Where(p => p.OwnerId == profileUserId && p.Status == "Closed")
            .OrderByDescending(p => p.PostedAt)
            .ToListAsync();

        var upcomingActivities = await _context.ActivityPosts
            .Where(p => p.OwnerId == profileUserId && !p.IsDeleted && p.Status == "Open")
            .OrderBy(p => p.ExpiresAt)
            .ToListAsync();

        var viewModel = new ProfileViewModel
        {
            UserId      = profileUserId,
            DisplayName = user.DisplayName ?? user.UserName ?? "Unknown User",
            Email       = user.Email ?? "",
            About       = user.About,
            AvatarUrl   = user.AvatarUrl,
            Tags        = user.Tags,
            Interests   = user.Interests,
            IsOwner     = isOwner,

            post_history        = postHistory,
            upcoming_activities = upcomingActivities,

            // Reviews received by this user
            reviews = await _context.Reviews
                .Where(r => r.RevieweeId == profileUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(),

            // Compute stats from already-loaded collections
            OrganizedCount = postHistory.Count + upcomingActivities.Count,

            JoinedCount = await _context.PostApplications
                .CountAsync(a => a.ApplicantId == profileUserId && a.Status == "Accepted"),
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
