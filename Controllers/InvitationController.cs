using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using event_web_dev_project.Data;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

[Authorize]
public class InvitationController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvitationController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // POST /Invitation/Send
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string receiverId, int postId, string? message)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (senderId == null) return Unauthorized();

        // Prevent inviting yourself
        if (senderId == receiverId)
            return Json(new { success = false, error = "You cannot invite yourself." });

        // Verify the post belongs to the sender and is open
        var post = await _db.ActivityPosts
            .FirstOrDefaultAsync(p => p.Id == postId && p.OwnerId == senderId && p.Status == "Open" && !p.IsDeleted);
        if (post == null)
            return Json(new { success = false, error = "Post not found or not available." });

        // Verify the receiver exists
        var receiver = await _userManager.FindByIdAsync(receiverId);
        if (receiver == null)
            return Json(new { success = false, error = "User not found." });

        // Prevent duplicate pending invitations to the same post
        var existing = await _db.Invitations
            .AnyAsync(i => i.SenderId == senderId
                        && i.ReceiverId == receiverId
                        && i.PostId == postId
                        && i.Status == "Pending");
        if (existing)
            return Json(new { success = false, error = "You have already sent a pending invitation for this event." });

        var invitation = new Invitation
        {
            SenderId   = senderId,
            ReceiverId = receiverId,
            PostId     = postId,
            Message    = message?.Trim(),
            SentAt     = DateTime.Now,
            Status     = "Pending",
        };

        _db.Invitations.Add(invitation);

        var sender = await _userManager.FindByIdAsync(senderId);
        var senderName = sender?.DisplayName ?? sender?.UserName ?? "Someone";
        _db.Notifications.Add(new Notification
        {
            UserId    = receiverId,
            Type      = "InvitationReceived",
            Title     = "New Invitation",
            Message   = $"{senderName} invited you to join \"{post.Title}\".",
            ActionUrl = "/MyBoard/Index",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST /Invitation/Respond
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(int invitationId, string action)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null) return Unauthorized();

        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.ReceiverId == currentUserId);

        if (invitation == null) return NotFound();

        if (invitation.Status != "Pending")
            return Json(new { success = false, error = "Invitation is no longer pending." });

        invitation.Status = action == "accept" ? "Accepted" : "Rejected";

        var responder = await _userManager.FindByIdAsync(currentUserId);
        var responderName = responder?.DisplayName ?? responder?.UserName ?? "Someone";
        var post = await _db.ActivityPosts.FindAsync(invitation.PostId);
        var postTitle = post?.Title ?? "an event";

        var notifTitle   = action == "accept" ? "Invitation Accepted" : "Invitation Declined";
        var notifMessage = action == "accept"
            ? $"{responderName} accepted your invitation to \"{postTitle}\"."
            : $"{responderName} declined your invitation to \"{postTitle}\".";

        _db.Notifications.Add(new Notification
        {
            UserId    = invitation.SenderId,
            Type      = action == "accept" ? "InvitationAccepted" : "InvitationRejected",
            Title     = notifTitle,
            Message   = notifMessage,
            ActionUrl = "/MyBoard/Index",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();

        return Json(new { success = true, status = invitation.Status });
    }
}
