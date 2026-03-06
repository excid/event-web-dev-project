using System;
using System.Collections.Generic;

namespace event_web_dev_project.Models
{
    public class CalendarViewModel
    {
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public DateTime SelectedDate { get; set; }
        
        // ข้อมูลสำหรับวาดช่องตารางปฏิทินทั้งหมด
        public List<CalendarDayModel> Days { get; set; } = new();
        
        // ข้อมูลกิจกรรมทั้งหมดในเดือนที่เลือก (ใช้ส่งไปให้ JavaScript)
        public List<CalendarEventItem> AllEventsThisMonth { get; set; } = new();
    }

    public class CalendarDayModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<CalendarEventItem> Events { get; set; } = new();
    }

    public class CalendarEventItem
    {
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public string Category { get; set; } 
        public string SourceType { get; set; } // "Post" (กิจกรรมที่คุณสร้าง) หรือ "Application" (กิจกรรมที่ไปเข้าร่วม)
        public string Status { get; set; }
        public string Author { get; set; }
        
        // แปลง Category เป็นคลาส CSS เพื่อแสดงสีให้ตรงหมวดหมู่
        public string CssColorClass => Category?.ToLower() switch
        {
            "sports" => "cat-sports",
            "dining" => "cat-dining",
            "social" => "cat-social",
            "shopping" => "cat-shopping",
            "study" => "cat-study",
            _ => "cat-others"
        };
    }
}