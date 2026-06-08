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
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public QuestionsController(AppDbContext db) => _db = db;

    public class CreateQuestionRequest
    {
        public int ThemeId { get; set; }
        public string Text { get; set; } = "";
        public string Status { get; set; } = "Easy";
        public string CorrectAnswer { get; set; } = "";
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
    {
        var login = User.Identity?.Name;
        var theme = await _db.Themes.FindAsync(request.ThemeId);
        if (theme == null) return NotFound();
        if (theme.Author != login) return Forbid();

        var question = new Question
        {
            Text = request.Text,
            Category = theme.Name,
            Status = request.Status,
            CorrectAnswer = request.CorrectAnswer,
            OptionA = request.OptionA,
            OptionB = request.OptionB,
            OptionC = request.OptionC,
            OptionD = request.OptionD,
            ThemeId = request.ThemeId
        };

        _db.Questions.Add(question);
        theme.QuestionCount++;
        await _db.SaveChangesAsync();
        return Ok(new { id = question.Id });
    }
}