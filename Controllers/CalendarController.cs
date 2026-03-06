using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers
{
    // สมมติว่าคลาส Controller ของคุณถูกประกาศไว้แบบนี้
    public partial class MyBoardController : Controller 
    {
        [HttpGet]
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // ใช้เดือน/ปีปัจจุบันหากไม่มีการส่งค่าพารามิเตอร์มา
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;
            var selectedDate = DateTime.Now.Date;

            // ป้องกันกรณีเดือนเกิน (เช่น กด Next จากเดือน 12 หรือ Prev จากเดือน 1)
            if (targetMonth > 12) { targetMonth = 1; targetYear++; }
            if (targetMonth < 1) { targetMonth = 12; targetYear--; }

            var model = new CalendarViewModel
            {
                CurrentYear = targetYear,
                CurrentMonth = targetMonth,
                SelectedDate = selectedDate
            };

            // 1. ดึงข้อมูลกิจกรรมที่คุณเป็นเจ้าของ (My Posts)
            var myPosts = await _db.ActivityPosts
                .Where(p => p.OwnerId == userId && 
                            p.ExpiresAt.Month == targetMonth && 
                            p.ExpiresAt.Year == targetYear)
                .ToListAsync();

            foreach (var post in myPosts)
            {
                model.AllEventsThisMonth.Add(new CalendarEventItem
                {
                    Title = post.Title,
                    EventDate = post.ExpiresAt, // แสดงบนปฏิทินตามวันหมดอายุ
                    Category = post.Category,
                    Status = post.Status,
                    SourceType = "Post",
                    Author = post.PostedBy ?? "You"
                });
            }

            // 2. ดึงข้อมูลกิจกรรมที่คุณไปเข้าร่วม (My Applications)
            var myApps = await _db.PostApplications
                .Include(a => a.ActivityPost)
                .Where(a => a.ApplicantId == userId && 
                            a.ActivityPost.ExpiresAt.Month == targetMonth && 
                            a.ActivityPost.ExpiresAt.Year == targetYear)
                .ToListAsync();

            foreach (var app in myApps)
            {
                model.AllEventsThisMonth.Add(new CalendarEventItem
                {
                    Title = app.ActivityPost.Title,
                    EventDate = app.ActivityPost.ExpiresAt,
                    Category = app.ActivityPost.Category,
                    Status = app.Status,
                    SourceType = "Application",
                    Author = app.ActivityPost.PostedBy ?? "Unknown"
                });
            }

            // 3. คำนวณวันในปฏิทิน (สร้างตาราง 42 ช่อง เพื่อให้ครอบคลุมทุกวันในเดือนและรอยต่อเดือน)
            var firstDayOfMonth = new DateTime(targetYear, targetMonth, 1);
            var startDay = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek); // หาวันอาทิตย์แรก

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
}