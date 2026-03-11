using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using event_web_dev_project.Data;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

public class EventController : Controller
{
    private readonly AppDbContext _db;

    public EventController(AppDbContext db)
    {
        _db = db;
    }
    public async Task<IActionResult> Join(int id)
    {
        var post = await _db.ActivityPosts
            .Include(p => p.Applications)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (post.OwnerId == currentUserId)
            return RedirectToAction("Index", "ActivityPost", new { id = post.Id });

        // Pass owner's username slug for profile link
        if (post.OwnerId != null)
        {
            var ownerUsername = await _db.Users
                .Where(u => u.Id == post.OwnerId)
                .Select(u => u.ProfileSlug)
                .FirstOrDefaultAsync();
            ViewBag.OwnerUsername = ownerUsername;
        }

        return View(post);
    }

    // POST /Event/SubmitApplication
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitApplication([FromBody] Join model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid data" });

        var post = await _db.ActivityPosts.FindAsync(model.PostId);
        if (post == null)
            return Json(new { success = false, error = "Post not found" });

        if (post.Status != "Open")
            return Json(new { success = false, error = "Post is no longer open" });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? model.ApplicantName;

        // Prevent duplicate applications
        var alreadyApplied = await _db.PostApplications
            .AnyAsync(a => a.PostId == model.PostId && a.ApplicantId == userId);

        if (alreadyApplied)
            return Json(new { success = false, error = "You have already applied to this post" });

        bool isFifo = post.ApplicationMode == "First-Come, First-Served";

        if (isFifo && post.CurrentMembers >= post.MaxMembers)
            return Json(new { success = false, error = "This activity is already full" });

        bool instantlyAccepted = isFifo;

        var application = new PostApplication
        {
            PostId        = model.PostId,
            ApplicantId   = userId,
            ApplicantName = displayName,
            Message       = model.Message,
            Status        = instantlyAccepted ? "Accepted" : "Pending",
            AppliedAt     = DateTime.Now
        };

        _db.PostApplications.Add(application);

    if (instantlyAccepted)
    {
        post.CurrentMembers++;

        if (post.CurrentMembers >= post.MaxMembers)
        {
            post.Status = "Closed";
            post.IsDeleted = true; 

            if (post.OwnerId != null)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId    = post.OwnerId,
                    Type      = "PostFull",
                    Title     = "Activity is Now Full",
                    Message   = $"Your post \"{post.Title}\" has reached its member limit and has been automatically closed.",
                    ActionUrl = $"/ActivityPost/Index/{post.Id}",
                    CreatedAt = DateTime.Now
                });
            }

            var acceptedApplicantIds = await _db.PostApplications
                .Where(a => a.PostId == post.Id && a.Status == "Accepted" && a.ApplicantId != null)
                .Select(a => a.ApplicantId!)
                .ToListAsync();

            foreach (var memberId in acceptedApplicantIds)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId    = memberId,
                    Type      = "PostFull",
                    Title     = "Activity is Now Full",
                    Message   = $"\"{post.Title}\" has reached its member limit. See you there!",
                    ActionUrl = $"/Event/Join/{post.Id}",
                    CreatedAt = DateTime.Now
                });
            }
        }
    }

        if (post.OwnerId != null && post.OwnerId != userId)
        {
            _db.Notifications.Add(new Notification
            {
                UserId    = post.OwnerId,
                Type      = "ApplicationReceived",
                Title     = instantlyAccepted ? "New Member Joined" : "New Application",
                Message   = instantlyAccepted
                    ? $"{displayName} automatically joined \"{post.Title}\" (First-Come, First-Served)."
                    : $"{displayName} applied to join \"{post.Title}\".",
                ActionUrl = $"/ActivityPost/Index/{post.Id}",
                CreatedAt = DateTime.Now
            });
        }

        if (instantlyAccepted && userId != null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId    = userId,
                Type      = "ApplicationAccepted",
                Title     = "Application Accepted",
                Message   = $"You have been automatically accepted to join \"{post.Title}\"!",
                ActionUrl = "/MyBoard/Index",
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();

        return Json(new { success = true, accepted = instantlyAccepted });
    }
}