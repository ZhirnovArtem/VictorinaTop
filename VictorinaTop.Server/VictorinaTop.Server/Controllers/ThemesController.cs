using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using VictorinaTop.Server.Data;
using VictorinaTop.Server.Models;

namespace VictorinaTop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ThemesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ThemesController(AppDbContext db) => _db = db;

    public class CreateThemeRequest { public string Name { get; set; } = ""; }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var themes = await _db.Themes
            .Select(t => new { t.Id, t.Name, t.Author, t.QuestionCount })
            .ToListAsync();
        return Ok(themes);
    }

    [HttpGet("{id}/questions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQuestions(int id)
    {
        var questions = await _db.Questions
            .Where(q => q.ThemeId == id)
            .Select(q => new { q.Id, q.Text, q.CorrectAnswer, q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.Status })
            .ToListAsync();
        return Ok(questions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateThemeRequest request)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login)) return Unauthorized();

        var theme = new Theme { Name = request.Name, Author = login, CreatedAt = DateTime.UtcNow };
        _db.Themes.Add(theme);
        await _db.SaveChangesAsync();
        return Ok(new { id = theme.Id, name = theme.Name });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var login = User.Identity?.Name;
        var theme = await _db.Themes.FindAsync(id);
        if (theme == null) return NotFound();
        if (theme.Author != login) return Forbid();
        _db.Themes.Remove(theme);
        await _db.SaveChangesAsync();
        return Ok();
    }
}