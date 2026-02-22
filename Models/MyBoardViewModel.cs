namespace event_web_dev_project.Models
{
    public class MyBoardViewModel
    {
        public MyPostsTabModel PostsTab { get; set; } = new();
        public MyApplicationsTabModel ApplicationsTab { get; set; } = new();
        public MyInvitationsTabModel InvitationsTab { get; set; } = new();
    }
}
