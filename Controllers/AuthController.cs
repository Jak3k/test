using Microsoft.AspNetCore.Mvc;
using Gamified_Coding_Platform.Models;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace Gamified_Coding_Platform.Controllers
{
    // Handles user authentication including login, registration and password security
    public class AuthController : Controller
    {
        // Database connection for user account management
        private readonly PlatformDbContext _context;
        public AuthController(PlatformDbContext context)
        {
            _context = context;
        }

        // Creates a secure hash of the password using salt for database storage
        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));
        }

        // Generates random salt bytes for password hashing security
        private static byte[] GenerateSalt()
        {
            // Create 16 byte random salt for each password
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Shows the user registration form
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Processes new user registration and creates account
        [HttpPost]
        public IActionResult Register(User user, string password)
        {
            // Check if all required fields are filled out
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(user);
            }
            // Check if username is already taken by another user
            if (_context.Users.Any(u => !string.IsNullOrEmpty(u.Username) && u.Username.ToLower() == user.Username!.ToLower()))
            {
                ModelState.AddModelError("", "Username already exists. Please choose a unique username.");
                return View(user);
            }
            // Check if email address is already registered to another user
            if (_context.Users.Any(u => !string.IsNullOrEmpty(u.Email) && u.Email.ToLower() == user.Email!.ToLower()))
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(user);
            }
            // Create secure password hash with salt and store in database
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);
            user.Badges = Convert.ToBase64String(salt) + ":" + hash; // Store salt:hash in Badges (not for production)
            // Initialize new user with zero points
            user.Points = 0;
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        // Shows the user login form
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Processes user login and creates session if credentials are valid
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Find user by username or email address
            var user = _context.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
            // Verify user exists and has stored password data
            if (user != null && !string.IsNullOrEmpty(user.Badges) && password != null)
            {
                // Extract salt and hash from stored password data
                var parts = user.Badges?.Split(':');
                if (parts != null && parts.Length == 2 && !string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
                {
                    // Compare provided password hash with stored hash
                    var salt = Convert.FromBase64String(parts[0]);
                    var hash = HashPassword(password, salt);
                    if (hash == parts[1])
                    {
                        // Create user session and redirect to home page
                        HttpContext.Session.SetString("UserId", user.UserId.ToString());
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            // Show error message if login credentials are invalid
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        // Logs out user by clearing their session data
        [HttpGet]
        public IActionResult Logout()
        {
            // Remove all session data to log user out
            HttpContext.Session.Clear();
            // Redirect back to login page after logout
            return RedirectToAction("Login");
        }
    }
}
