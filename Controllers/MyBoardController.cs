using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using event_web_dev_project.Models;

namespace event_web_dev_project.Controllers;

public class MyBoardController : Controller
{
    public IActionResult Index()
    {
        var model = new MyBoardViewModel
        {
            PostsTab = new MyPostsTabModel
            {
                Posts = new List<PostCardModel>
                {
                    new PostCardModel
                    {
                        Category = "Sports",
                        Status = MyPostsTabModel.StatusOpen,
                        Title = "Looking for Football Teammates",
                        Description = "Join our Sunday afternoon football match at Central Park. All skill levels welcome — just bring your boots and good energy!",
                        Location = "Central Park, NY",
                        CurrentMember = 8,
                        MaxMember = 11,
                        PublishDate = new DateTime(2026, 2, 10),
                        ExpirationDate = new DateTime(2026, 3, 10),
                        Author = "John Doe",
                        NumApplication = 5
                    }
                }
            },
            ApplicationsTab = new MyApplicationsTabModel
            {
                Applications = new List<ApplicationCardModel>
                {
                    new ApplicationCardModel
                    {
                        Title = "Looking for Football Teammates",
                        Status = "Pending",
                        ApplicationDate = DateTime.Now.AddDays(-2),
                        Message = "Hi! I’ve been playing for 3 years and would love to join."
                    },
                    new ApplicationCardModel
                    {
                        Title = "Weekend Brunch Crew",
                        Status = "Accepted",
                        ApplicationDate = DateTime.Now.AddDays(-7),
                        Message = "Sounds fun! I enjoy discovering new cafes."
                    },
                    new ApplicationCardModel
                    {
                        Title = "Network Study Group",
                        Status = "Rejected",
                        ApplicationDate = DateTime.Now.AddDays(-1),
                        Message = "I want to improve my networking skills."
                    }
                }
            },
            InvitationsTab = new MyInvitationsTabModel
            {
                ReceivedInvitations = new List<ReceivedInvitationCardModel>
                    {
                        new ReceivedInvitationCardModel
                        {
                            Title = "Sunday Football Match",
                            Status = "Pending",
                            Sender = "Michael Lee",
                            ReceivedDate = DateTime.Now.AddDays(-1),
                            EventDate = DateTime.Now.AddDays(5),
                            Message = "Hey! We’re short one player this Sunday. Would you like to join?"
                        },
                        new ReceivedInvitationCardModel
                        {
                            Title = "Networking Study Group",
                            Status = "Accepted",
                            Sender = "Alice Wong",
                            ReceivedDate = DateTime.Now.AddDays(-3),
                            EventDate = DateTime.Now.AddDays(7),
                            Message = "We think you'd be a great fit for our IPv4 lab prep session."
                        },
                        new ReceivedInvitationCardModel
                        {
                            Title = "Weekend Brunch Meetup",
                            Status = "Rejected",
                            Sender = "Chris Tan",
                            ReceivedDate = DateTime.Now.AddDays(-6),
                            EventDate = DateTime.Now.AddDays(2),
                            Message = "Join us for brunch this Saturday at our favorite café!"
                        }
                    },

                    SentInvitations = new List<SentInvitationCardModel>
                    {
                        new SentInvitationCardModel
                        {
                            Title = "Basketball Friendly Game",
                            Status = "Pending",
                            Receiver = "Daniel Smith",
                            SentDate = DateTime.Now.AddDays(-2),
                            EventDate = DateTime.Now.AddDays(4),
                            Message = "Would love to have you join our 3v3 basketball session!"
                        },
                        new SentInvitationCardModel
                        {
                            Title = "UI/UX Design Workshop",
                            Status = "Accepted",
                            Receiver = "Sophia Chen",
                            SentDate = DateTime.Now.AddDays(-5),
                            EventDate = DateTime.Now.AddDays(10),
                            Message = "We think you'd really enjoy this design workshop."
                        },
                        new SentInvitationCardModel
                        {
                            Title = "Music Jam Session",
                            Status = "Rejected",
                            Receiver = "Ryan Park",
                            SentDate = DateTime.Now.AddDays(-4),
                            EventDate = DateTime.Now.AddDays(6),
                            Message = "We’re gathering musicians for a casual jam night."
                        }
                    }
            }
        };
        return View(model);
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
