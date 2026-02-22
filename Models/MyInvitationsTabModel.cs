namespace event_web_dev_project.Models
{
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
}
