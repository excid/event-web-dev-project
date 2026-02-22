namespace event_web_dev_project.Models
{
    public class ReceivedInvitationCardModel
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Sender { get; set; }
        public DateTime ReceivedDate {get; set;}
        public DateTime EventDate {get; set;}
        public string? Message { get; set; }

        public string StatusMessage =>
            Status switch
            {
                "Accepted" => "🎉 ACCEPT XD",
                "Pending"  => "⏳ PENDING BRUH",
                "Rejected" => "❌ REJECTED",
                _ => ""
            };
    }
    public class SentInvitationCardModel
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Receiver { get; set; }
        public DateTime SentDate {get; set;}
        public DateTime EventDate {get; set;}
        public string? Message { get; set; }

        public string StatusMessage =>
            Status switch
            {
                "Accepted" => "🎉 ACCEPT XD",
                "Pending"  => "⏳ PENDING BRUH",
                "Rejected" => "❌ REJECTED",
                _ => ""
            };
    }
}