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

        // IDs of activities the profile user has joined as an accepted participant
        var joinedActivityIds = await _context.PostApplications
            .Where(a => a.ApplicantId == profileUserId && a.Status == "Accepted")
            .Select(a => a.PostId)
            .ToListAsync();

        var now = DateTime.Now;

        // Recent Activity History: all activities (owned or joined) whose ActivityDate has passed
        var postHistory = await _context.ActivityPosts
            .Where(p => !p.IsDeleted && p.ActivityDate <= now &&
                        (p.OwnerId == profileUserId || joinedActivityIds.Contains(p.Id)))
            .OrderByDescending(p => p.ActivityDate)
            .ToListAsync();

        // Upcoming Activities: all activities (owned or joined) whose ActivityDate hasn't passed yet
        var upcomingActivities = await _context.ActivityPosts
            .Where(p => !p.IsDeleted && p.ActivityDate > now &&
                        (p.OwnerId == profileUserId || joinedActivityIds.Contains(p.Id)))
            .OrderBy(p => p.ActivityDate)
            .ToListAsync();

        // Collect display names for activity owners (needed for the per-activity review button)
        var ownerIds = postHistory
            .Select(p => p.OwnerId)
            .Where(id => id != null && id != profileUserId)
            .Distinct()
            .ToList();

        var activityOwnerNames = ownerIds.Count > 0
            ? await _context.Users
                .Where(u => ownerIds.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id,
                    u => u.DisplayName ?? u.UserName ?? "Unknown User")
            : new Dictionary<string, string>();

        var allReviews = await _context.Reviews
            .Where(r => r.RevieweeId == profileUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Find any direct-profile review (PostId == 0) written by the current user
        Review? existingReviewByCurrentUser = null;
        if (!isOwner && currentUserId != null)
        {
            existingReviewByCurrentUser = allReviews
                .FirstOrDefault(r => r.PostId == 0 && r.ReviewerId == currentUserId);
        }

        // Load current user's open posts (for invite dropdown) and the post IDs with pending invitations
        List<ActivityPost> ownedOpenPosts = new();
        HashSet<int> alreadyInvitedPostIds = new();
        if (!isOwner && currentUserId != null)
        {
            ownedOpenPosts = await _context.ActivityPosts
                .Where(p => p.OwnerId == currentUserId && p.Status == "Open" && !p.IsDeleted)
                .OrderByDescending(p => p.PostedAt)
                .ToListAsync();

            var pendingPostIds = await _context.Invitations
                .Where(i => i.SenderId == currentUserId
                         && i.ReceiverId == profileUserId
                         && i.Status == "Pending")
                .Select(i => i.PostId)
                .ToListAsync();
            alreadyInvitedPostIds = pendingPostIds.ToHashSet();
        }

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

            reviews = allReviews,

            HasReviewedByCurrentUser    = existingReviewByCurrentUser != null,
            ExistingReviewByCurrentUser = existingReviewByCurrentUser,

            // Compute stats from already-loaded collections
            OrganizedCount = postHistory.Count(p => p.OwnerId == profileUserId)
                           + upcomingActivities.Count(p => p.OwnerId == profileUserId),

            JoinedCount = await _context.PostApplications
                .CountAsync(a => a.ApplicantId == profileUserId && a.Status == "Accepted"),

            OwnedOpenPosts       = ownedOpenPosts,
            AlreadyInvitedPostIds = alreadyInvitedPostIds,

            ActivityOwnerNames   = activityOwnerNames,
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
