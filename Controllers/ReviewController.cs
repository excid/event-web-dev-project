using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Data;
using event_web_dev_project.Models;
using System.Security.Claims;

namespace event_web_dev_project.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET /Review/Create?revieweeId=<id>
    public async Task<IActionResult> Create(string revieweeId)
    {
        if (string.IsNullOrEmpty(revieweeId))
            return BadRequest();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (revieweeId == currentUserId)
            return BadRequest("You cannot review yourself.");

        var reviewee = await _userManager.FindByIdAsync(revieweeId);
        if (reviewee == null)
            return NotFound();

        ViewBag.RevieweeId   = revieweeId;
        ViewBag.RevieweeUsername = reviewee.ProfileSlug;
        ViewBag.RevieweeName = reviewee.DisplayName ?? reviewee.UserName ?? "Unknown User";
        ViewBag.RevieweeAvatarUrl = reviewee.AvatarUrl;
        return View();
    }

    // POST /Review/Submit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int? postId, string revieweeId, string revieweeName, int rating, string? comment, bool isAnonymous = false)
    {
        if (rating < 1 || rating > 5)
            return Json(new { success = false, error = "Rating must be between 1 and 5" });

        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (reviewerId == null)
            return Json(new { success = false, error = "Not authenticated" });

        // Prevent self-reviews
        if (reviewerId == revieweeId)
            return Json(new { success = false, error = "You cannot review yourself" });

        // postId = 0 is a sentinel meaning this is a direct profile review (not tied to a specific post)
        int resolvedPostId = postId ?? 0;

        if (resolvedPostId != 0)
        {
            // Check the post exists
            var post = await _db.ActivityPosts.FindAsync(resolvedPostId);
            if (post == null)
                return Json(new { success = false, error = "Post not found" });

            // Prevent duplicate reviews for the same reviewee on the same post
            var existing = await _db.Reviews.AnyAsync(r =>
                r.PostId == resolvedPostId &&
                r.ReviewerId == reviewerId &&
                r.RevieweeId == revieweeId);

            if (existing)
                return Json(new { success = false, error = "You have already reviewed this person for this post" });
        }
        else
        {
            // For direct profile reviews (postId = 0), prevent duplicate reviews per reviewer/reviewee pair
            var existing = await _db.Reviews.AnyAsync(r =>
                r.PostId == 0 &&
                r.ReviewerId == reviewerId &&
                r.RevieweeId == revieweeId);

            if (existing)
                return Json(new { success = false, error = "You have already reviewed this user" });
        }

        var reviewer = await _userManager.FindByIdAsync(reviewerId);
        var reviewerName = isAnonymous ? "Anonymous User" : (reviewer?.DisplayName ?? reviewer?.UserName ?? "Unknown");

        var review = new Review
        {
            PostId       = resolvedPostId,
            ReviewerId   = reviewerId,
            ReviewerName = reviewerName,
            RevieweeId   = revieweeId,
            RevieweeName = revieweeName,
            Rating       = rating,
            Comment      = comment,
            IsAnonymous  = isAnonymous,
            CreatedAt    = DateTime.Now
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var displayName = isAnonymous ? "Someone" : (reviewer?.DisplayName ?? reviewer?.UserName ?? "Someone");
        _db.Notifications.Add(new Notification
        {
            UserId    = revieweeId,
            Type      = "ReviewReceived",
            Title     = "New Review",
            Message   = $"{displayName} left you a {rating}-star review.",
            ActionUrl = "/Profile/Index",
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /Review/Update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string revieweeId, int rating, string? comment, bool isAnonymous = false)
    {
        if (rating < 1 || rating > 5)
            return Json(new { success = false, error = "Rating must be between 1 and 5" });

        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (reviewerId == null)
            return Json(new { success = false, error = "Not authenticated" });

        if (reviewerId == revieweeId)
            return Json(new { success = false, error = "You cannot review yourself" });

        // Find the existing direct-profile review
        var existing = await _db.Reviews.FirstOrDefaultAsync(r =>
            r.PostId == 0 &&
            r.ReviewerId == reviewerId &&
            r.RevieweeId == revieweeId);

        if (existing == null)
            return Json(new { success = false, error = "No existing review found to update" });

        var reviewer = await _userManager.FindByIdAsync(reviewerId);
        existing.ReviewerName = isAnonymous ? "Anonymous User" : (reviewer?.DisplayName ?? reviewer?.UserName ?? "Unknown");
        existing.Rating       = rating;
        existing.Comment      = comment;
        existing.IsAnonymous  = isAnonymous;

        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }
}
