using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictorinaTop.Server.Data;
using VictorinaTop.Server.Models;

namespace VictorinaTop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ThemesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ThemesController(AppDbContext db)
    {
        _db = db;
    }

    public class CreateThemeRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ThemeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var themes = await _db.Themes
            .Select(t => new ThemeResponse
            {
                Id = t.Id,
                Name = t.Name,
                Author = t.Author,
                QuestionCount = t.QuestionCount,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
        return Ok(themes);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var theme = await _db.Themes
            .Where(t => t.Id == id)
            .Select(t => new ThemeResponse
            {
                Id = t.Id,
                Name = t.Name,
                Author = t.Author,
                QuestionCount = t.QuestionCount,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (theme == null)
            return NotFound(new { error = "Тема не найдена" });

        return Ok(theme);
    }

    [HttpGet("{id}/questions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQuestions(int id)
    {
        var theme = await _db.Themes.FindAsync(id);
        if (theme == null)
            return NotFound(new { error = "Тема не найдена" });

        var questions = await _db.Questions
            .Where(q => q.ThemeId == id)
            .Select(q => new
            {
                q.Id,
                q.Text,
                q.CorrectAnswer,
                q.OptionA,
                q.OptionB,
                q.OptionC,
                q.OptionD,
                q.Status
            })
            .ToListAsync();
        return Ok(questions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateThemeRequest request)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            return Unauthorized(new { error = "Пользователь не авторизован" });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Название темы обязательно" });

        // Проверяем, существует ли тема с таким названием
        var exists = await _db.Themes.AnyAsync(t => t.Name == request.Name);
        if (exists)
            return Conflict(new { error = "Тема с таким названием уже существует" });

        var theme = new Theme
        {
            Name = request.Name,
            Author = login,
            QuestionCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Themes.Add(theme);
        await _db.SaveChangesAsync();

        return Ok(new { id = theme.Id, name = theme.Name, author = theme.Author });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            return Unauthorized();

        var theme = await _db.Themes.FindAsync(id);
        if (theme == null)
            return NotFound(new { error = "Тема не найдена" });

        if (theme.Author != login)
            return Forbid();

        // Удаляем все вопросы этой темы
        var questions = await _db.Questions
            .Where(q => q.ThemeId == id)
            .ToListAsync();
        _db.Questions.RemoveRange(questions);

        _db.Themes.Remove(theme);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Тема удалена" });
    }
}