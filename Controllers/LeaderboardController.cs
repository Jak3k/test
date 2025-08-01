using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using System.Linq;
using Gamified_Coding_Platform.Filters;

namespace Gamified_Coding_Platform.Controllers
{
    // Only logged in users can view the leaderboard rankings
    [RequireLogin]
    // Displays the top performing users ranked by points and achievements
    public class LeaderboardController : Controller
    {
        // Database connection for accessing user ranking data
        private readonly PlatformDbContext _context;
        public LeaderboardController(PlatformDbContext context)
        {
            _context = context;
        }

        // Shows the leaderboard with top users ranked by total points earned
        public IActionResult Index()
        {
            // Get the top 10 highest scoring users from the database
            var topUsers = _context.Users
                .OrderByDescending(u => u.Points)
                .Take(10)
                .ToList();
            return View(topUsers);
        }
    }
}
