using Microsoft.AspNetCore.Mvc;

namespace event_web_dev_project.Controllers;

public class EventController : Controller
{
    // GET /Event/Join?state=apply|pending|accepted|rejected|owner
    public IActionResult Join()
    {
        return View();
    }
}
