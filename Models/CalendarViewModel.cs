using System;
using System.Collections.Generic;

namespace event_web_dev_project.Models
{
    public class CalendarViewModel
    {
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public DateTime SelectedDate { get; set; }
        
        public List<CalendarDayModel> Days { get; set; } = new();
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
        public string? Title { get; set; }
        public DateTime EventDate { get; set; }
        public string? Category { get; set; } 
        public string? SourceType { get; set; }
        public string? Status { get; set; }
        public string? Author { get; set; }
        
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