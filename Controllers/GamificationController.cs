using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using Microsoft.AspNetCore.Http;
using Gamified_Coding_Platform.Filters;

namespace Gamified_Coding_Platform.Controllers
{
    // Only logged in users can view badges and achievements
    [RequireLogin]
    // Shows user badges, achievements and gamification progress
    public class GamificationController : Controller
    {
        // Database connection for accessing badge and achievement data
        private readonly PlatformDbContext _context;
        public GamificationController(PlatformDbContext context)
        {
            _context = context;
        }

        // Shows all available badges and which ones the user has unlocked
        public IActionResult Index()
        {
            // Get current user ID from session data
            int? userId = null;
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var parsedId))
            {
                userId = parsedId;
            }
            User? user = null;
            List<UserBadge> unlocked = new();
            if (userId.HasValue)
            {
                user = _context.Users.Find(userId.Value);
                unlocked = _context.UserBadges.Where(ub => ub.UserId == userId.Value).ToList();
            }

            var allBadges = new List<Badge>
            {
                new Badge { Key = "FirstChallenge", Name = "First Challenge", Description = "Complete your first challenge." },
                new Badge { Key = "100Points", Name = "100 Points", Description = "Earn 100 points." },
                new Badge { Key = "5Challenges", Name = "5 Challenges Completed", Description = "Complete 5 challenges." },
                new Badge { Key = "10Challenges", Name = "10 Challenges Completed", Description = "Complete 10 challenges." },
                new Badge { Key = "HelloWorld", Name = "Hello World", Description = "Solve the 'Hello World' challenge." },
                new Badge { Key = "SumofTwoNumbers", Name = "Sum of Two Numbers", Description = "Solve the 'Sum of Two Numbers' challenge." },
                new Badge { Key = "ReverseaString", Name = "Reverse a String", Description = "Solve the 'Reverse a String' challenge." },
                
                new Badge { Key = "FirstTutorial", Name = "First Tutorial", Description = "Complete your first tutorial." },
                new Badge { Key = "TutorialExplorer", Name = "Tutorial Explorer", Description = "Complete 3 tutorials." },
                new Badge { Key = "TutorialMaster", Name = "Tutorial Master", Description = "Complete all available tutorials." },
                new Badge { Key = "HelloWorldTutorial", Name = "Hello World Tutorial", Description = "Complete the 'Hello World' tutorial." },
                new Badge { Key = "SumofTwoNumbersTutorial", Name = "Sum of Two Numbers Tutorial", Description = "Complete the 'Sum of Two Numbers' tutorial." },
                new Badge { Key = "ReverseaStringTutorial", Name = "Reverse a String Tutorial", Description = "Complete the 'Reverse a String' tutorial." }
            };

            // Pass badge data to the view for display
            ViewBag.AllBadges = allBadges;
            ViewBag.UnlockedBadges = unlocked;
            return View(user);
        }
    }
}
