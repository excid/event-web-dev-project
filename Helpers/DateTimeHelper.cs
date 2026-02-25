namespace event_web_dev_project.Helpers
{
    public static class DateTimeHelper
    {
        public static string ToTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} days ago";

            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hours ago";

            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";

            return "just now";
        }
    }
}