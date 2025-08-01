using Gamified_Coding_Platform.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession();

var app = builder.Build();
// Seed preset challenges
using (var scope = app.Services.CreateScope())
{
    ChallengeSeeder.Seed(scope.ServiceProvider);

    // --- Award missing achievements to users based on their progress and points ---
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    var allBadges = new[]
    {
        new { Key = "FirstChallenge", Condition = new Func<User, int, bool>((u, completed) => completed >= 1) },
        new { Key = "100Points", Condition = new Func<User, int, bool>((u, completed) => u.Points >= 100) },
        new { Key = "5Challenges", Condition = new Func<User, int, bool>((u, completed) => completed >= 5) },
        new { Key = "10Challenges", Condition = new Func<User, int, bool>((u, completed) => completed >= 10) },
        new { Key = "HelloWorld", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "HelloWorld"))) },
        new { Key = "SumofTwoNumbers", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "SumofTwoNumbers"))) },
        new { Key = "ReverseaString", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "ReverseaString"))) },
        new { Key = "FindMaximum", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "FindMaximum"))) },
        new { Key = "Factorial", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "Factorial"))) },
        new { Key = "PalindromeCheck", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "PalindromeCheck"))) },
        new { Key = "FizzBuzz", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "FizzBuzz"))) },
        new { Key = "CountVowels", Condition = new Func<User, int, bool>((u, completed) => db.ProgressRecords.Any(p => p.UserId == u.UserId && p.Completed && db.Challenges.Any(c => c.ChallengeId == p.ChallengeId && (c.Title ?? "").Replace(" ", "") == "CountVowels"))) }
    };

    foreach (var user in db.Users.ToList())
    {
        int completed = db.ProgressRecords.Count(p => p.UserId == user.UserId && p.Completed);
        foreach (var badge in allBadges)
        {
            bool hasBadge = db.UserBadges.Any(b => b.UserId == user.UserId && b.BadgeKey == badge.Key);
            if (!hasBadge && badge.Condition(user, completed))
            {
                db.UserBadges.Add(new UserBadge
                {
                    UserId = user.UserId,
                    BadgeKey = badge.Key,
                    UnlockedAt = DateTime.Now
                });
            }
        }
    }
    db.SaveChanges();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
