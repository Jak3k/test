using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using System.Linq;
using Gamified_Coding_Platform.Filters;

namespace Gamified_Coding_Platform.Controllers
{
    // Only logged in users can access challenge features
    [RequireLogin]
    public class ChallengeController : Controller
    {
        // Database connection for accessing challenges and progress
        private readonly PlatformDbContext _context;
        public ChallengeController(PlatformDbContext context)
        {
            _context = context;
        }

        // Shows all available challenges to the user
        public IActionResult Index()
        {
            var challenges = _context.Challenges.ToList();
            return View(challenges);
        }

        // Shows the challenge attempt page where users write code
        public IActionResult Attempt(int id)
        {
            var challenge = _context.Challenges.Find(id);
            if (challenge == null) return NotFound();
            return View(challenge);
        }

        // Processes the submitted code and checks if its correct
        [HttpPost]
        public IActionResult Attempt(int id, string userCode)
        {
            var challenge = _context.Challenges.Find(id);
            if (challenge == null)
            {
                return NotFound();
            }
            // Checks if user code contains expected solution
            string result = "Code submitted! (Evaluation not implemented)";
            bool isCorrect = false;
            if (!string.IsNullOrEmpty(challenge.SolutionTemplate) && !string.IsNullOrEmpty(userCode))
            {
                if (userCode.Trim().Replace(" ","").Contains(challenge.SolutionTemplate.Trim().Replace(" ","")))
                {
                    result = "Success! Your code matches the expected solution pattern.";
                    isCorrect = true;
                }
                else
                {
                    result = "Incorrect solution. Please try again.";
                }
            }
            // If correct and user is logged in award points and badges
            if (isCorrect && HttpContext.Session.GetString("UserId") != null)
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (int.TryParse(userIdStr, out int userId))
                {
                    var progress = _context.ProgressRecords.FirstOrDefault(p => p.UserId == userId && p.ChallengeId == id);
                    bool firstCompletion = progress == null || !progress.Completed;
                    // Create new progress if user hasnt attempted this challenge
                    if (progress == null)
                    {
                        // New progress tracking for this user and challenge
                        progress = new Gamified_Coding_Platform.Models.Progress
                        {
                            UserId = userId,
                            ChallengeId = id,
                            Completed = true,
                            DateCompleted = DateTime.Now
                        };
                        _context.ProgressRecords.Add(progress);
                    }
                    else if (!progress.Completed)
                    {
                        progress.Completed = true;
                        progress.DateCompleted = DateTime.Now;
                        _context.ProgressRecords.Update(progress);
                    }
                    // Award points and badges for first completion
                    var user = _context.Users.Find(userId);
                    if (user != null && firstCompletion)
                    {
                        // Award points for completing this challenge
                        user.Points += challenge.Points;
                        int completedCount = _context.ProgressRecords.Count(p => p.UserId == userId && p.Completed);
                        user.ChallengesCompleted = completedCount + 1;

                        var now = DateTime.Now;
                        void AwardBadge(string key)
                        {
                            if (!_context.UserBadges.Any(b => b.UserId == userId && b.BadgeKey == key))
                            {
                                _context.UserBadges.Add(new UserBadge
                                {
                                    UserId = userId,
                                    BadgeKey = key,
                                    UnlockedAt = now
                                });
                            }
                        }
                        // First challenge badge (award if this is the first completed challenge)
                        if (user.ChallengesCompleted == 1)
                            AwardBadge("FirstChallenge");
                        // 100 points badge - award when user reaches 100 total points
                        if (user.Points >= 100)
                            AwardBadge("100Points");
                        // 5 challenges badge - award when user completes 5 challenges
                        if (user.ChallengesCompleted >= 5)
                            AwardBadge("5Challenges");
                        // 10 challenges badge - award when user completes 10 challenges
                        if (user.ChallengesCompleted >= 10)
                            AwardBadge("10Challenges");

                        // Award specific badge for completing this particular challenge
                        if (!string.IsNullOrEmpty(challenge.Title))
                        {
                            // Create badge key by removing spaces from challenge title
                            var key = challenge.Title.Replace(" ", "");
                            AwardBadge(key);
                        }

                        // Save all progress and badge changes to the database
                        _context.SaveChanges();
                    }
                }
            }
            
            // Show result to user and return to challenge page
            ViewBag.Result = result;
            return View(challenge);
        }

        // Shows the tutorial page to help new users understand how to use the platform
        public IActionResult Tutorial()
        {
            return View();
        }
    }
}
