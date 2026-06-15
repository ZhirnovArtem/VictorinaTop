using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictorinaTop.Server.Data;
using VictorinaTop.Server.Models;

namespace VictorinaTop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public QuestionsController(AppDbContext db)
    {
        _db = db;
    }

    public class CreateQuestionRequest
    {
        public string ThemeName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Status { get; set; } = "Easy";
        public string CorrectAnswer { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            return Unauthorized();

        var theme = await _db.Themes
            .FirstOrDefaultAsync(t => t.Name == request.ThemeName);

        if (theme == null)
            return NotFound(new { error = "Тема не найдена" });

        if (theme.Author != login)
            return Forbid();

        var question = new Question
        {
            Text = request.Text,
            Category = request.ThemeName,
            Status = request.Status,
            CorrectAnswer = request.CorrectAnswer,
            OptionA = request.OptionA,
            OptionB = request.OptionB,
            OptionC = request.OptionC,
            OptionD = request.OptionD
        };

        _db.Questions.Add(question);
        theme.QuestionCount++;
        await _db.SaveChangesAsync();

        return Ok(new { id = question.Id });
    }
}