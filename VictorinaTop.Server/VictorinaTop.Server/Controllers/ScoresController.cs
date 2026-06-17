using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;          // ← добавить
using VictorinaTop.Server.Data;
using VictorinaTop.Server.Models;

namespace VictorinaTop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScoresController : ControllerBase
{
    private readonly AppDbContext _db;

    public ScoresController(AppDbContext db)
    {
        _db = db;
    }

    public class SubmitScoreRequest
    {
        public int ThemeId { get; set; }
        public int Points { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        // Получаем userId из JWT-токена
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var theme = await _db.Themes.FindAsync(request.ThemeId);

        var score = new Score
        {
            UserId = userId,
            ThemeName = theme?.Name ?? "",
            Points = request.Points,
            AchievedAt = DateTime.UtcNow
        };

        _db.Scores.Add(score);

        if (request.Points > user.MaxScore)
            user.MaxScore = request.Points;

        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int limit = 10)
    {
        var leaderboard = await _db.Users
            .OrderByDescending(u => u.MaxScore)
            .Take(limit)
            .Select(u => new { u.Id, u.Login, u.MaxScore })
            .ToListAsync();
        return Ok(leaderboard);
    }
}