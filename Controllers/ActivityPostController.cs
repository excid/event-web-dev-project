using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Data;
using event_web_dev_project.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace event_web_dev_project.Controllers;

[Authorize]
public class ActivityPostController : Controller
{
    private readonly AppDbContext _db;

    public ActivityPostController(AppDbContext db)
    {
        _db = db;
    }

    // GET /ActivityPost/Index
    public async Task<IActionResult> Index(int? id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var allPosts = await _db.ActivityPosts
            .Where(p => !p.IsDeleted && p.OwnerId == currentUserId)
            .OrderByDescending(p => p.PostedAt)
            .ToListAsync();

        if (!allPosts.Any())
            return View(null);

        var selectedPost = id.HasValue
            ? allPosts.FirstOrDefault(p => p.Id == id.Value)
            : allPosts.First();

        if (selectedPost == null)
            return NotFound();

        selectedPost.Applications = await _db.PostApplications
            .Where(a => a.PostId == selectedPost.Id)
            .OrderBy(a => a.AppliedAt)
            .ToListAsync();

        // Pass owner's username slug for profile link
        var ownerUsername = await _db.Users
            .Where(u => u.Id == currentUserId)
            .Select(u => u.ProfileSlug)
            .FirstOrDefaultAsync();
        ViewBag.OwnerUsername = ownerUsername;
        ViewBag.AllPosts = allPosts;
        return View(selectedPost);
    }

    // GET /ActivityPost/Archive
    public async Task<IActionResult> Archive()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Only show the logged-in user's own archived posts
        var archivedPosts = await _db.ActivityPosts
            .Where(p => p.IsDeleted && p.OwnerId == currentUserId)
            .Include(p => p.Applications)
            .OrderByDescending(p => p.DeletedAt)
            .ToListAsync();

        return View(archivedPosts);
    }

    // POST /ActivityPost/ClosePost
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClosePost(int id)
    {
        var post = await _db.ActivityPosts.FindAsync(id);
        if (post == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (post.OwnerId != currentUserId)
            return Json(new { success = false, error = "Unauthorized" });

        post.Status = "Closed";
        post.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/RestorePost
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestorePost(int id)
    {
        var post = await _db.ActivityPosts.FindAsync(id);
        if (post == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (post.OwnerId != currentUserId)
            return Json(new { success = false, error = "Unauthorized" });
        
        if (post.ExpiresAt <= DateTime.Now)
            return Json(new { success = false, error = "Cannot restore an expired post" });

        if (post.CurrentMembers >= post.MaxMembers)
            return Json(new { success = false, error = "Cannot restore a post that is already full" });


        post.Status = "Open";
        post.IsDeleted = false;
        post.DeletedAt = null;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/HardDeletePost
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDeletePost(int id)
    {
        var post = await _db.ActivityPosts
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (post.OwnerId != currentUserId)
            return Json(new { success = false, error = "Unauthorized" });

        _db.PostApplications.RemoveRange(post.Applications);
        _db.ActivityPosts.Remove(post);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/AcceptApplication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptApplication(int applicationId, int postId)
    {
        var post = await _db.ActivityPosts.FindAsync(postId);
        if (post == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (post.OwnerId != currentUserId)
            return Json(new { success = false, error = "Unauthorized" });

        var application = await _db.PostApplications.FindAsync(applicationId);
        if (application == null) return NotFound();

        application.Status = "Accepted";

        if (post.CurrentMembers < post.MaxMembers)
            post.CurrentMembers++;

        if (application.ApplicantId != null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId    = application.ApplicantId,
                Type      = "ApplicationAccepted",
                Title     = "Application Accepted",
                Message   = $"Your application to join \"{post.Title}\" was accepted!",
                ActionUrl = "/MyBoard/Index",
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();

        return Json(new {
            success = true,
            currentMembers = post.CurrentMembers,
            maxMembers = post.MaxMembers
        });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(int applicationId, int postId)
    {
        var post = await _db.ActivityPosts.FindAsync(postId);
        if (post == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (post.OwnerId != currentUserId)
            return Json(new { success = false, error = "Unauthorized" });

        var application = await _db.PostApplications.FindAsync(applicationId);
        if (application == null) return NotFound();

        application.Status = "Rejected";

        if (application.ApplicantId != null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId    = application.ApplicantId,
                Type      = "ApplicationRejected",
                Title     = "Application Declined",
                Message   = $"Your application to join \"{post.Title}\" was not accepted.",
                ActionUrl = "/MyBoard/Index",
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int postId, string applicantName, string message)
    {
        var post = await _db.ActivityPosts.FindAsync(postId);
        if (post == null || post.Status == "Closed" || post.IsDeleted)
            return Json(new { success = false, error = "Post is not available" });

        var application = new PostApplication
        {
            PostId        = postId,
            ApplicantName = applicantName,
            Message       = message,
            Status        = "Pending",
            AppliedAt     = DateTime.Now,
            ApplicantId   = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        _db.PostApplications.Add(application);

        // Notify the post owner that a new application has arrived
        if (post.OwnerId != null && post.OwnerId != application.ApplicantId)
        {
            _db.Notifications.Add(new Notification
            {
                UserId    = post.OwnerId,
                Type      = "ApplicationReceived",
                Title     = "New Application",
                Message   = $"{applicantName} applied to join \"{post.Title}\".",
                ActionUrl = $"/ActivityPost/Index/{post.Id}",
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();

        return Json(new { success = true, applicationId = application.Id });
    }


    public IActionResult Create()
    {
        return View("~/Views/ActivityPost/Create.cshtml");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid data" });

        var userId      = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

        var modeLabel = model.Mode == "fifo"
            ? "First-Come, First-Served"
            : "Overflow allowed - Owner selects";

        var post = new ActivityPost
        {
            Title           = model.Title,
            Category        = model.Category,
            Description     = model.Description,
            Location        = model.Location,
            MaxMembers      = model.MaxMembers,
            CurrentMembers  = 0,
            ExpiresAt       = DateTime.Parse(model.Deadline),  // stored as Bangkok time
            ActivityDate    = DateTime.Parse(model.ActivityDate),  // stored as Bangkok time
            ApplicationMode = modeLabel,
            Status          = "Open",
            PostedBy        = displayName,
            OwnerId         = userId,
            PostedAt        = DateTime.Now,
            IsDeleted       = false
        };

        _db.ActivityPosts.Add(post);
        await _db.SaveChangesAsync();

        return Json(new { success = true, postId = post.Id });
    }
}