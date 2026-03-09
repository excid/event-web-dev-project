using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Models;
using event_web_dev_project.Data;

namespace event_web_dev_project.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _context.ActivityPosts
            .Where(p => !p.IsDeleted && p.Status == "Open")
            .Include(p => p.Applications)
            .OrderByDescending(p => p.PostedAt)
            .ToListAsync();

        var locations = await _context.ActivityPosts
        .Where(p => !p.IsDeleted && p.Status == "Open")
        .Select(p => p.Location)
        .Distinct()
        .OrderBy(l => l)
        .ToListAsync();

        ViewBag.Locations = locations;

        return View(posts);
    }
    [HttpGet]
    public async Task<IActionResult> Search(string? q, string? categories, string? location,
        string? sortBy, string? dateRange, string? activityDateRange, string? statusFilter)
    {
        var query = _context.ActivityPosts
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p =>
                EF.Functions.ILike(p.Title, $"%{q}%") ||
                EF.Functions.ILike(p.Description, $"%{q}%"));

        if (!string.IsNullOrWhiteSpace(categories))
        {
            var catList = categories.Split(',').Select(c => c.Trim()).ToList();
            query = query.Where(p => catList.Contains(p.Category));
        }

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => p.Location == location);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            var statusList = statusFilter.Split(',').Select(s => s.Trim()).ToList();
            query = query.Where(p => statusList.Contains(p.Status));
        }

        var now = DateTime.Now;

        query = dateRange switch
        {
            "today" => query.Where(p => p.ExpiresAt >= now && p.ExpiresAt < now.AddDays(1)),
            "week"  => query.Where(p => p.ExpiresAt >= now && p.ExpiresAt < now.AddDays(7)),
            "month" => query.Where(p => p.ExpiresAt >= now && p.ExpiresAt < now.AddDays(30)),
            _       => query
        };

        query = activityDateRange switch
        {
            "today" => query.Where(p => p.ActivityDate >= now && p.ActivityDate < now.AddDays(1)),
            "week"  => query.Where(p => p.ActivityDate >= now && p.ActivityDate < now.AddDays(7)),
            "month" => query.Where(p => p.ActivityDate >= now && p.ActivityDate < now.AddDays(30)),
            _       => query
        };

        query = sortBy switch
        {
            "expiring" => query.OrderBy(p => p.ExpiresAt),
            "activity" => query.OrderBy(p => p.ActivityDate),
            "members"  => query.OrderByDescending(p => p.CurrentMembers),
            _          => query.OrderByDescending(p => p.PostedAt)  // newest
        };

        var results = await query
            .Select(p => new {
                id             = p.Id,
                title          = p.Title,
                category       = p.Category,
                description    = p.Description,
                location       = p.Location,
                postedBy       = p.PostedBy,
                postedAt       = p.PostedAt.ToString("MMM d, yyyy"),
                expiresAt      = p.ExpiresAt.ToString("MMM d, yyyy"),
                activityDate   = p.ActivityDate.ToString("MMM d, yyyy"),
                maxMembers     = p.MaxMembers,
                currentMembers = p.CurrentMembers,
                spotsLeft      = p.SpotsLeft,
                status         = p.Status
            })
            .ToListAsync();

        return Json(results);
    }

    

    public IActionResult Privacy()
    {
        return View();
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


