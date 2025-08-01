using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

// Handles friend connections and social features between users
public class FriendsController : Controller
{
    // Database connection for managing friendship relationships
    private readonly PlatformDbContext _context;

    public FriendsController(PlatformDbContext context)
    {
        _context = context;
    }

    // Shows the friends list and pending friend requests for current user
    public IActionResult Index()
    {
        // Get current user ID from session data
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        // Get all accepted friendships for this user
        var friends = _context.Friendships
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.IsAccepted)
            .ToList();

        // Get all pending friend requests sent to this user
        var pending = _context.Friendships
            .Where(f => f.FriendId == userId && !f.IsAccepted)
            .ToList();

        // Pass friend data to the view for display
        ViewBag.Friends = friends;
        ViewBag.Pending = pending;
        return View();
    }

    // Sends a friend request to another user by username
    [HttpPost]
    public IActionResult SendRequest(string friendUsername)
    {
        // Get current user ID from session data
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        // Find the user they want to be friends with
        var friend = _context.Users.FirstOrDefault(u => !string.IsNullOrEmpty(u.Username) && u.Username.ToLower() == friendUsername.ToLower());
        if (friend == null || friend.UserId == userId)
        {
            TempData["FriendError"] = "User not found or invalid.";
            return RedirectToAction("Index");
        }

        // Check if friendship already exists between these users
        if (!_context.Friendships.Any(f =>
            (f.UserId == userId && f.FriendId == friend.UserId) ||
            (f.UserId == friend.UserId && f.FriendId == userId)))
        {
            // Create new pending friendship request
            _context.Friendships.Add(new Friendship
            {
                UserId = userId,
                FriendId = friend.UserId,
                IsAccepted = false
            });
            _context.SaveChanges();
        }
        else
        {
            TempData["FriendError"] = "Friend request already sent or you are already friends.";
        }
        return RedirectToAction("Index");
    }

    // POST: /Friends/Accept
    [HttpPost]
    public IActionResult Accept(int friendshipId)
    {
        var friendship = _context.Friendships.Find(friendshipId);
        if (friendship != null)
        {
            friendship.IsAccepted = true;
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // POST: /Friends/Remove
    [HttpPost]
    public IActionResult Remove(int friendshipId)
    {
        var friendship = _context.Friendships.Find(friendshipId);
        if (friendship != null)
        {
            _context.Friendships.Remove(friendship);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // GET: /Friends/FriendProfile/5
    public IActionResult FriendProfile(int id)
    {
        var friend = _context.Users.FirstOrDefault(u => u.UserId == id);
        if (friend == null)
            return NotFound();

        // Get friend's unlocked badges
        var unlockedBadges = _context.UserBadges.Where(ub => ub.UserId == id).ToList();

        // Define all possible badges (should match GamificationController)
        var allBadges = new List<Badge>
        {
            new Badge { Key = "FirstChallenge", Name = "First Challenge", Description = "Complete your first challenge." },
            new Badge { Key = "100Points", Name = "100 Points", Description = "Earn 100 points." },
            new Badge { Key = "5Challenges", Name = "5 Challenges Completed", Description = "Complete 5 challenges." },
            new Badge { Key = "10Challenges", Name = "10 Challenges Completed", Description = "Complete 10 challenges." }
        };

        ViewBag.AllBadges = allBadges;
        ViewBag.UnlockedBadges = unlockedBadges;
        return View(friend);
    }
}
