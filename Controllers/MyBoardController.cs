using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using event_web_dev_project.Data;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

[Authorize]  
public class MyBoardController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyBoardController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var myPosts = await _db.ActivityPosts
            .Where(p => p.OwnerId == userId && !p.IsDeleted)
            .Include(p => p.Applications)
            .OrderByDescending(p => p.PostedAt)
            .ToListAsync();

        var postsTab = new MyPostsTabModel
        {
            Posts = myPosts.Select(p => new PostCardModel
            {
                Category        = p.Category,
                Status          = p.Status,
                Title           = p.Title,
                Description     = p.Description,
                Location        = p.Location,
                CurrentMember   = p.CurrentMembers,
                MaxMember       = p.MaxMembers,
                PublishDate     = p.PostedAt,
                ExpirationDate  = p.ExpiresAt,
                Author          = p.PostedBy,
                NumApplication  = p.Applications.Count,
                PostId          = p.Id   
            }).ToList()
        };

        var myApplications = await _db.PostApplications
            .Where(a => a.ApplicantId == userId)
            .Include(a => a.ActivityPost)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        var applicationsTab = new MyApplicationsTabModel
        {
            Applications = myApplications.Select(a => new ApplicationCardModel
            {
                Title           = a.ActivityPost?.Title ?? "Unknown Post",
                Status          = a.Status,
                Description     = a.ActivityPost?.Description ?? "",
                ApplicationDate = a.AppliedAt,
                Message         = a.Message
            }).ToList()
        };

        var sentInvitations = await _db.Invitations
            .Where(i => i.SenderId == userId)
            .Include(i => i.Receiver)
            .Include(i => i.Post)
            .OrderByDescending(i => i.SentAt)
            .ToListAsync();

        var receivedInvitations = await _db.Invitations
            .Where(i => i.ReceiverId == userId)
            .Include(i => i.Sender)
            .Include(i => i.Post)
            .OrderByDescending(i => i.SentAt)
            .ToListAsync();

        var invitationsTab = new MyInvitationsTabModel
        {
            SentInvitations = sentInvitations.Select(i => new SentInvitationCardModel
            {
                InvitationId     = i.Id,
                Title            = i.Post?.Title ?? "Unknown Event",
                Status           = i.Status,
                Receiver         = i.Receiver?.DisplayName ?? i.Receiver?.UserName ?? "Unknown",
                ReceiverUserId   = i.ReceiverId,
                ReceiverUsername = i.Receiver?.ProfileSlug,
                SentDate         = i.SentAt,
                EventDate        = (i.Post != null && i.Post.ActivityDate != default) ? i.Post.ActivityDate : (i.Post?.ExpiresAt ?? i.SentAt),
                Message          = i.Message,
            }).ToList(),

            ReceivedInvitations = receivedInvitations.Select(i => new ReceivedInvitationCardModel
            {
                InvitationId   = i.Id,
                Title          = i.Post?.Title ?? "Unknown Event",
                Status         = i.Status,
                Sender         = i.Sender?.DisplayName ?? i.Sender?.UserName ?? "Unknown",
                SenderUserId   = i.SenderId,
                SenderUsername = i.Sender?.ProfileSlug,
                ReceivedDate   = i.SentAt,
                EventDate      = (i.Post != null && i.Post.ActivityDate != default) ? i.Post.ActivityDate : (i.Post?.ExpiresAt ?? i.SentAt),
                Message        = i.Message,
            }).ToList(),
        };

        var model = new MyBoardViewModel
        {
            PostsTab        = postsTab,
            ApplicationsTab = applicationsTab,
            InvitationsTab  = invitationsTab
        };

        return View(model);
    }
}
