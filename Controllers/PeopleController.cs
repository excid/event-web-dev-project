using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Data;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

[Authorize]
public class PeopleController : Controller
{
    private readonly AppDbContext _context;

    public PeopleController(AppDbContext context)
    {
        _context = context;
    }

    // GET /People/Index
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserCardModel
            {
                Id          = u.Id,
                ProfileSlug = u.ProfileSlug,
                DisplayName = u.DisplayName ?? u.UserName ?? "Unknown",
                About       = u.About,
                AvatarUrl   = u.AvatarUrl,
                Tags        = u.Tags,
                Interests   = u.Interests
            })
            .ToListAsync();

        return View(users);
    }

    // GET /People/Search?q=john
    [HttpGet]
    public async Task<IActionResult> Search(string? q)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u =>
                EF.Functions.ILike(u.DisplayName ?? "", $"%{q}%") ||
                EF.Functions.ILike(u.UserName    ?? "", $"%{q}%") ||
                EF.Functions.ILike(u.Tags        ?? "", $"%{q}%") ||
                EF.Functions.ILike(u.Interests   ?? "", $"%{q}%"));

        var results = await query
            .OrderBy(u => u.DisplayName)
            .Select(u => new {
                id          = u.Id,
                profileSlug = u.ProfileSlug,
                displayName = u.DisplayName ?? u.UserName ?? "Unknown",
                about       = u.About,
                avatarUrl   = u.AvatarUrl,
                tags        = u.Tags,
                interests   = u.Interests
            })
            .ToListAsync();

        return Json(results);
    }
}