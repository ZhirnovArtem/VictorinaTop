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
        public int ThemeId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Status { get; set; } = "Easy";
        public string CorrectAnswer { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class UpdateQuestionRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Status { get; set; } = "Easy";
        public string CorrectAnswer { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class QuestionResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int ThemeId { get; set; }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var questions = await _db.Questions
            .Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Category = q.Category,
                Status = q.Status,
                CorrectAnswer = q.CorrectAnswer,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                ThemeId = q.ThemeId
            })
            .ToListAsync();
        return Ok(questions);
    }

    [HttpGet("theme/{themeId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByThemeId(int themeId)
    {
        var theme = await _db.Themes.FindAsync(themeId);
        if (theme == null)
            return NotFound(new { error = "Тема не найдена" });

        var questions = await _db.Questions
            .Where(q => q.ThemeId == themeId)
            .Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Category = q.Category,
                Status = q.Status,
                CorrectAnswer = q.CorrectAnswer,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                ThemeId = q.ThemeId
            })
            .ToListAsync();
        return Ok(questions);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var question = await _db.Questions
            .Where(q => q.Id == id)
            .Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Category = q.Category,
                Status = q.Status,
                CorrectAnswer = q.CorrectAnswer,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                ThemeId = q.ThemeId
            })
            .FirstOrDefaultAsync();

        if (question == null)
            return NotFound(new { error = "Вопрос не найден" });

        return Ok(question);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            return Unauthorized(new { error = "Пользователь не авторизован" });

        // Проверяем существование темы
        var theme = await _db.Themes.FindAsync(request.ThemeId);
        if (theme == null)
            return NotFound(new { error = "Тема не найдена" });

        // Проверяем права (только автор темы может добавлять вопросы)
        if (theme.Author != login)
            return Forbid();

        // Валидация
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { error = "Текст вопроса обязателен" });

        if (string.IsNullOrWhiteSpace(request.CorrectAnswer))
            return BadRequest(new { error = "Правильный ответ обязателен" });

        if (string.IsNullOrWhiteSpace(request.OptionA) || string.IsNullOrWhiteSpace(request.OptionB) ||
            string.IsNullOrWhiteSpace(request.OptionC) || string.IsNullOrWhiteSpace(request.OptionD))
            return BadRequest(new { error = "Все варианты ответов обязательны" });

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

        return Ok(new
        {
            id = question.Id,
            message = "Вопрос успешно добавлен"
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionRequest request)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            return Unauthorized(new { error = "Пользователь не авторизован" });

        var question = await _db.Questions
            .Include(q => q.Theme)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound(new { error = "Вопрос не найден" });

        if (question.Theme?.Author != login)
            return Forbid();

        // Обновляем поля
        if (!string.IsNullOrWhiteSpace(request.Text))
            question.Text = request.Text;

        if (!string.IsNullOrWhiteSpace(request.Status))
            question.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.CorrectAnswer))
            question.CorrectAnswer = request.CorrectAnswer;

        if (!string.IsNullOrWhiteSpace(request.OptionA))
            question.OptionA = request.OptionA;

        if (!string.IsNullOrWhiteSpace(request.OptionB))
            question.OptionB = request.OptionB;

        if (!string.IsNullOrWhiteSpace(request.OptionC))
            question.OptionC = request.OptionC;

        if (!string.IsNullOrWhiteSpace(request.OptionD))
            question.OptionD = request.OptionD;

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Вопрос обновлён" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var login = User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            return Unauthorized(new { error = "Пользователь не авторизован" });

        var question = await _db.Questions
            .Include(q => q.Theme)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound(new { error = "Вопрос не найден" });

        if (question.Theme?.Author != login)
            return Forbid();

        _db.Questions.Remove(question);

        if (question.Theme != null)
            question.Theme.QuestionCount--;

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Вопрос удалён" });
    }
}