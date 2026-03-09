using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using event_web_dev_project.Data;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

[Authorize]
public class CalendarController : Controller
{
    // ประกาศตัวแปรเพื่อเชื่อมต่อ Database
    private readonly AppDbContext _db;

    // รับค่า Database Context ผ่าน Constructor
    public CalendarController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var targetYear = year ?? DateTime.Now.Year;
        var targetMonth = month ?? DateTime.Now.Month;
        var selectedDate = DateTime.Now.Date;

        if (targetMonth > 12) { targetMonth = 1; targetYear++; }
        if (targetMonth < 1) { targetMonth = 12; targetYear--; }

        var model = new CalendarViewModel
        {
            CurrentYear = targetYear,
            CurrentMonth = targetMonth,
            SelectedDate = selectedDate
        };

        // 1. ดึงข้อมูล My Posts
        var myPosts = await _db.ActivityPosts
            .Where(p => p.OwnerId == userId && 
                        p.ActivityDate.Month == targetMonth && 
                        p.ActivityDate.Year == targetYear)
            .ToListAsync();

        foreach (var post in myPosts)
        {
            model.AllEventsThisMonth.Add(new CalendarEventItem
            {
                Title = post.Title,
                EventDate = post.ActivityDate,
                Category = post.Category,
                Status = post.Status,
                SourceType = "Post",
                Author = post.PostedBy ?? "You"
            });
        }

        // 2. ดึงข้อมูล My Applications
        var myApps = await _db.PostApplications
            .Include(a => a.ActivityPost)
            .Where(a => a.ApplicantId == userId && 
                        a.ActivityPost.ActivityDate.Month == targetMonth && 
                        a.ActivityPost.ActivityDate.Year == targetYear)
            .ToListAsync();

        foreach (var app in myApps)
        {
            model.AllEventsThisMonth.Add(new CalendarEventItem
            {
                Title = app.ActivityPost?.Title,
                EventDate = app.ActivityPost.ActivityDate,
                Category = app.ActivityPost?.Category,
                Status = app.Status,
                SourceType = "Application",
                Author = app.ActivityPost?.PostedBy ?? "Unknown"
            });
        }

        // 3. คำนวณตารางปฏิทิน
        var firstDayOfMonth = new DateTime(targetYear, targetMonth, 1);
        var startDay = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek); 

        for (int i = 0; i < 42; i++) 
        {
            var currentDate = startDay.AddDays(i);
            model.Days.Add(new CalendarDayModel
            {
                Date = currentDate,
                IsCurrentMonth = currentDate.Month == targetMonth,
                Events = model.AllEventsThisMonth
                              .Where(e => e.EventDate.Date == currentDate.Date)
                              .ToList()
            });
        }

        return View(model);
    }
}