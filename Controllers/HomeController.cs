using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Models;
using event_web_dev_project.Data;

namespace event_web_dev_project.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Load only open, non-deleted posts
        // Include applications so we can show applicant counts
        var posts = await _db.ActivityPosts
            .Where(p => !p.IsDeleted && p.Status == "Open")
            .Include(p => p.Applications)
            .OrderByDescending(p => p.PostedAt)
            .ToListAsync();

        return View(posts);
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
