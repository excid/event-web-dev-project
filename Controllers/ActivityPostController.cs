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
        var allPosts = await _db.ActivityPosts
            .Where(p => !p.IsDeleted)
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

        ViewBag.AllPosts = allPosts;
        return View(selectedPost);
    }

    // GET /ActivityPost/Archive
    public async Task<IActionResult> Archive()
    {
        var archivedPosts = await _db.ActivityPosts
            .Where(p => p.IsDeleted)
            .OrderByDescending(p => p.DeletedAt)
            .ToListAsync();

        return View(archivedPosts);
    }

    // POST /ActivityPost/ClosePost  → returns JSON
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClosePost(int id)
    {
        var post = await _db.ActivityPosts.FindAsync(id);
        if (post == null) return NotFound();

        post.Status = "Closed";
        post.IsDeleted = true;
        post.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/RestorePost  → returns JSON
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestorePost(int id)
    {
        var post = await _db.ActivityPosts.FindAsync(id);
        if (post == null) return NotFound();

        post.Status = "Open";
        post.IsDeleted = false;
        post.DeletedAt = null;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/HardDeletePost  → returns JSON
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDeletePost(int id)
    {
        var post = await _db.ActivityPosts
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        _db.PostApplications.RemoveRange(post.Applications);
        _db.ActivityPosts.Remove(post);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/AcceptApplication  → returns JSON
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptApplication(int applicationId, int postId)
    {
        var application = await _db.PostApplications.FindAsync(applicationId);
        if (application == null) return NotFound();

        application.Status = "Accepted";

        var post = await _db.ActivityPosts.FindAsync(postId);
        if (post != null && post.CurrentMembers < post.MaxMembers)
            post.CurrentMembers++;

        await _db.SaveChangesAsync();

        // Return updated counts so JS can update the UI
        return Json(new {
            success = true,
            currentMembers = post?.CurrentMembers ?? 0,
            maxMembers = post?.MaxMembers ?? 0
        });
    }

    // POST /ActivityPost/RejectApplication  → returns JSON
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(int applicationId, int postId)
    {
        var application = await _db.PostApplications.FindAsync(applicationId);
        if (application == null) return NotFound();

        application.Status = "Rejected";
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /ActivityPost/Apply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int postId, string applicantName, string message)
    {
        var post = await _db.ActivityPosts.FindAsync(postId);
        if (post == null || post.Status == "Closed" || post.IsDeleted)
            return Json(new { success = false, error = "Post is not available" });

        var application = new PostApplication
        {
            PostId = postId,
            ApplicantName = applicantName,
            Message = message,
            Status = "Pending",
            AppliedAt = DateTime.Now
        };

        _db.PostApplications.Add(application);
        await _db.SaveChangesAsync();

        return Json(new { success = true, applicationId = application.Id });
    }

    [Authorize]  // must be logged in
public IActionResult Create()
{
    return View("~/Views/ActivityPost/Create.cshtml");
}

// POST /ActivityPost/Create  →  saves the new post to DB
[HttpPost]
[Authorize]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([FromBody] CreatePostViewModel model)
{
    if (!ModelState.IsValid)
        return Json(new { success = false, error = "Invalid data" });

    // Get the logged-in user's ID and display name
    var userId      = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var displayName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

    // Map application mode label
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
        ExpiresAt       = DateTime.Parse(model.Deadline).ToUniversalTime(),
        ApplicationMode = modeLabel,
        Status          = "Open",
        PostedBy        = displayName,
        OwnerId         = userId,           // ← links post to logged-in user
        PostedAt        = DateTime.UtcNow,
        IsDeleted       = false
    };

    _db.ActivityPosts.Add(post);
    await _db.SaveChangesAsync();

    return Json(new { success = true, postId = post.Id });
}

}
