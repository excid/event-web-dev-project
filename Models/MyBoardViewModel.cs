namespace event_web_dev_project.Models
{
    public class MyBoardViewModel
    {
        public MyPostsTabModel PostsTab { get; set; } = new();
        public MyApplicationsTabModel ApplicationsTab { get; set; } = new();
        public MyInvitationsTabModel InvitationsTab { get; set; } = new();
    }
    public class ApplicationCardModel
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? Message { get; set; }

        public string StatusMessage =>
            Status switch
            {
                "Accepted" => "🎉 Congratulations! Your application has been accepted. The organizer will contact you with more details.",
                "Pending"  => "⏳ Your application is currently under review. Please wait for the organizer's response.",
                "Rejected" => "❌ Unfortunately, your application was not approved. You may explore other events.",
                _ => ""
            };
    }
    public class ReceivedInvitationCardModel
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Sender { get; set; }
        public DateTime ReceivedDate {get; set;}
        public DateTime EventDate {get; set;}
        public string? Message { get; set; }
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
                "Accepted" => "🎉 Great news! The recipient has accepted your invitation.",
                "Pending"  => "⏳ Your invitation is awaiting the recipient’s response.",
                "Rejected" => "❌ The recipient has declined your invitation.",
                _ => ""
            };
    }
    public class MyApplicationsTabModel
    {
        public const string StatusPending = "Pending";
        public const string StatusAccepted = "Accepted";
        public const string StatusRejected = "Rejected";

        public List<ApplicationCardModel> Applications { get; set; } = new();

        public int TotalApplications => Applications.Count;
        public int PendingApplications => Applications.Count(a => a.Status == StatusPending);
        public int AcceptedApplications => Applications.Count(a => a.Status == StatusAccepted);
        public int RejectedApplications => Applications.Count(a => a.Status == StatusRejected);
    }
    public class MyInvitationsTabModel
    {
        public const string StatusPending = "Pending";
        public const string StatusAccepted = "Accepted";
        public const string StatusRejected = "Rejected";

        public List<SentInvitationCardModel> SentInvitations { get; set; } = new();

        public int SentTotalInvitations => SentInvitations.Count;
        public int SentPendingInvitations => SentInvitations.Count(i => i.Status == StatusPending);
        public int SentAcceptedInvitations => SentInvitations.Count(i => i.Status == StatusAccepted);
        public int SentRejectedInvitations => SentInvitations.Count(i => i.Status == StatusRejected);

        public List<ReceivedInvitationCardModel> ReceivedInvitations { get; set; } = new();

        public int ReceivedTotalInvitations => ReceivedInvitations.Count;
        public int ReceivedPendingInvitations => ReceivedInvitations.Count(i => i.Status == StatusPending);
        public int ReceivedAcceptedInvitations => ReceivedInvitations.Count(i => i.Status == StatusAccepted);
        public int ReceivedRejectedInvitations => ReceivedInvitations.Count(i => i.Status == StatusRejected);
    }

    public class MyPostsTabModel
    {
        public const string StatusOpen = "Open";
        public const string StatusClosed = "Closed";
        public const string StatusExpired = "Expired";

        public List<PostCardModel> Posts { get; set; } = new();

        public int TotalPosts => Posts.Count;
        public int OpenPosts => Posts.Count(p => p.Status == StatusOpen);
        public int ClosedPosts => Posts.Count(p => p.Status == StatusClosed);
        public int ExpiredPosts => Posts.Count(p => p.Status == StatusExpired);
        public int TotalApplications => Posts.Sum(p => p.NumApplication);
    }
    public class PostCardModel
    {
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int CurrentMember { get; set; }
        public int MaxMember { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? Author { get; set; }
        public int NumApplication { get; set; }
    }
}
