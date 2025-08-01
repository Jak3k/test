using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using Gamified_Coding_Platform.Filters;

namespace Gamified_Coding_Platform.Controllers;

// Only logged in users can access the home page and main site features
[RequireLogin]
// Handles the main home page and core navigation for logged in users
public class HomeController : Controller
{
    // Logger for recording any errors or important events
    private readonly ILogger<HomeController> _logger;
    // Database context for getting challenge and badge counts
    private readonly PlatformDbContext _context;

    public HomeController(ILogger<HomeController> logger, PlatformDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Shows the main dashboard page for logged in users
    public IActionResult Index()
    {
        // Get dynamic counts for the stats section
        ViewBag.ChallengeCount = _context.Challenges.Count();
        ViewBag.BadgeCount = GetAllBadges().Count();
        ViewBag.TopicsCount = _context.Challenges.Select(c => c.Category).Distinct().Count();
        return View();
    }

    // Gets all available badges that can be earned
    private List<Badge> GetAllBadges()
    {
        return new List<Badge>
        {
            new Badge { Key = "FirstChallenge", Name = "First Challenge", Description = "Complete your first challenge." },
            new Badge { Key = "100Points", Name = "100 Points", Description = "Earn 100 points." },
            new Badge { Key = "5Challenges", Name = "5 Challenges Completed", Description = "Complete 5 challenges." },
            new Badge { Key = "10Challenges", Name = "10 Challenges Completed", Description = "Complete 10 challenges." },
            new Badge { Key = "HelloWorld", Name = "Hello World", Description = "Solve the 'Hello World' challenge." },
            new Badge { Key = "SumofTwoNumbers", Name = "Sum of Two Numbers", Description = "Solve the 'Sum of Two Numbers' challenge." },
            new Badge { Key = "ReverseaString", Name = "Reverse a String", Description = "Solve the 'Reverse a String' challenge." }
        };
    }

    // Shows the privacy policy page
    public IActionResult Privacy()
    {
        return View();
    }

    // Shows error page when something goes wrong in the application
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Create error view with request ID for debugging purposes
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
