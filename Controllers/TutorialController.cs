using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using Gamified_Coding_Platform.Filters;

// Handles tutorial viewing and practice for learning C# programming concepts
[RequireLogin]
public class TutorialController : Controller
{
    // Database connection for tracking tutorial progress and awarding badges
    private readonly PlatformDbContext _context;
    
    public TutorialController(PlatformDbContext context)
    {
        _context = context;
    }
    // Static list of tutorials with examples - in production this would come from database
    private static readonly List<TutorialModel> Tutorials = new()
    {
        new TutorialModel { Id = 1, Title = "Hello World", Description = "Learn how to print 'Hello, World!' to the console.", Example = "Console.WriteLine(\"Hello, World!\");" },
        new TutorialModel { Id = 2, Title = "Sum of Two Numbers", Description = "Learn how to return the sum of two numbers.", Example = "return a + b;" },
        new TutorialModel { Id = 3, Title = "Reverse a String", Description = "Learn how to reverse a string.", Example = "return new string(input.Reverse().ToArray());" }
    };

    // Shows the main tutorial list page with all available programming lessons
    public IActionResult Index()
    {
        return View(Tutorials);
    }

    // Shows a specific tutorial with examples and practice exercises
    public IActionResult Attempt(int id)
    {
        // Find the requested tutorial by its ID number
        var tutorial = Tutorials.FirstOrDefault(t => t.Id == id);
        if (tutorial == null) return NotFound();
        return View(tutorial);
    }

    // Handles tutorial completion and awards appropriate badges
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Complete(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Json(new { success = false, message = "User not logged in" });
        }

        var tutorial = Tutorials.FirstOrDefault(t => t.Id == id);
        if (tutorial == null)
        {
            return Json(new { success = false, message = $"Tutorial not found. ID: {id}" });
        }

        var existingProgress = _context.TutorialProgressRecords
            .FirstOrDefault(tp => tp.UserId == userId && tp.TutorialId == id);

        bool firstCompletion = existingProgress == null;

        // Create or update tutorial progress
        if (existingProgress == null)
        {
            var tutorialProgress = new TutorialProgress
            {
                UserId = userId,
                TutorialId = id,
                TutorialTitle = tutorial.Title,
                CompletedAt = DateTime.Now,
                IsCompleted = true
            };
            _context.TutorialProgressRecords.Add(tutorialProgress);
        }
        else if (!existingProgress.IsCompleted)
        {
            existingProgress.IsCompleted = true;
            existingProgress.CompletedAt = DateTime.Now;
            _context.TutorialProgressRecords.Update(existingProgress);
        }

        // Award badges for first-time completion
        if (firstCompletion)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                var now = DateTime.Now;
                var newBadges = new List<string>();

                void AwardBadge(string key, string badgeName)
                {
                    if (!_context.UserBadges.Any(b => b.UserId == userId && b.BadgeKey == key))
                    {
                        _context.UserBadges.Add(new UserBadge
                        {
                            UserId = userId,
                            BadgeKey = key,
                            UnlockedAt = now
                        });
                        newBadges.Add(badgeName);
                    }
                }

                // Award specific tutorial badge
                var tutorialBadgeKey = tutorial.Title.Replace(" ", "") + "Tutorial";
                AwardBadge(tutorialBadgeKey, $"{tutorial.Title} Tutorial");

                // Count total completed tutorials BEFORE saving changes
                int totalCompleted = _context.TutorialProgressRecords
                    .Count(tp => tp.UserId == userId && tp.IsCompleted);

                // Award milestone badges
                if (totalCompleted == 0) // This will be the first tutorial
                {
                    AwardBadge("FirstTutorial", "First Tutorial");
                }
                
                if (totalCompleted + 1 >= 3)
                {
                    AwardBadge("TutorialExplorer", "Tutorial Explorer");
                }
                
                if (totalCompleted + 1 >= Tutorials.Count)
                {
                    AwardBadge("TutorialMaster", "Tutorial Master");
                }

                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Tutorial completed successfully!", 
                    newBadges = newBadges 
                });
            }
        }

        _context.SaveChanges();
        return Json(new { success = true, message = "Tutorial completed!" });
    }
}

// Model class that defines the structure of tutorial data
public class TutorialModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Example { get; set; } = "";
}
