using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

// Handles user profile viewing and editing functionality
public class ProfileController : Controller
{
    // Database connection for accessing user profile data
    private readonly PlatformDbContext _context;

    public ProfileController(PlatformDbContext context)
    {
        _context = context;
    }

    // Shows the user profile page with current statistics and information
    public IActionResult Index()
    {
        // Get current user ID from session data
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        // Fetch the most current user data from database
        var user = _context.Users
            .Where(u => u.UserId == userId)
            .FirstOrDefault();

        // Update user statistics from progress records if user exists
        if (user != null)
        {
            // Recalculate completed challenges from progress records
            user.ChallengesCompleted = _context.ProgressRecords.Count(p => p.UserId == userId && p.Completed);
            // Additional point recalculation could be added here if needed
            // user.Points = ...;
        }

        // Redirect to login if user not found
        if (user == null) return RedirectToAction("Login", "Auth");
        return View(user);
    }

    // Shows the profile edit form for updating user information
    public IActionResult Edit()
    {
        // Get current user ID from session data
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        // Find user record to edit
        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return RedirectToAction("Login", "Auth");
        return View(user);
    }

    // Processes the submitted profile changes and saves them
    [HttpPost]
    public IActionResult Edit(User model, IFormFile? avatarFile)
    {
        // Check if all form data is valid
        if (ModelState.IsValid)
        {
            // Find the user record to update
            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user != null)
            {
                user.Username = model.Username;
                user.Email = model.Email;
                
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    var fileName = $"avatar_{user.UserId}{Path.GetExtension(avatarFile.FileName)}";
                    var filePath = Path.Combine("wwwroot", "avatars", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        avatarFile.CopyTo(stream);
                    }
                    user.AvatarUrl = $"/avatars/{fileName}";
                }
                // Save all changes to the database
                _context.Users.Update(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        // If validation fails show form again with error messages
        return View(model);
    }
}
