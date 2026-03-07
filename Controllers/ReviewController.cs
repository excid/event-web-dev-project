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

    // POST /Review/Submit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int postId, string revieweeId, string revieweeName, int rating, string? comment, bool isAnonymous = false)
    {
        if (rating < 1 || rating > 5)
            return Json(new { success = false, error = "Rating must be between 1 and 5" });

        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (reviewerId == null)
            return Json(new { success = false, error = "Not authenticated" });

        // Prevent self-reviews
        if (reviewerId == revieweeId)
            return Json(new { success = false, error = "You cannot review yourself" });

        // Check the post exists
        var post = await _db.ActivityPosts.FindAsync(postId);
        if (post == null)
            return Json(new { success = false, error = "Post not found" });

        // Prevent duplicate reviews for the same reviewee on the same post
        var existing = await _db.Reviews.AnyAsync(r =>
            r.PostId == postId &&
            r.ReviewerId == reviewerId &&
            r.RevieweeId == revieweeId);

        if (existing)
            return Json(new { success = false, error = "You have already reviewed this person for this post" });

        var reviewer = await _userManager.FindByIdAsync(reviewerId);
        var reviewerName = isAnonymous ? "Anonymous User" : (reviewer?.DisplayName ?? reviewer?.UserName ?? "Unknown");

        var review = new Review
        {
            PostId       = postId,
            ReviewerId   = reviewerId,
            ReviewerName = reviewerName,
            RevieweeId   = revieweeId,
            RevieweeName = revieweeName,
            Rating       = rating,
            Comment      = comment,
            IsAnonymous  = isAnonymous,
            CreatedAt    = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }
}
