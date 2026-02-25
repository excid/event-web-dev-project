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
