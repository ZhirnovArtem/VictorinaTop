using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using VictorinaTop.Server.Data;
using VictorinaTop.Server.Models;
using VictorinaTop.Server.Services;

namespace VictorinaTop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly EmailService _email;
    private readonly PasswordHasher _hasher;
    private readonly VerificationService _verification;

    public AuthController(
        AppDbContext db,
        JwtService jwt,
        EmailService email,
        PasswordHasher hasher,
        VerificationService verification)
    {
        _db = db;
        _jwt = jwt;
        _email = email;
        _hasher = hasher;
        _verification = verification;
    }

    public class RegisterRequest { public string Login { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class RegisterResponse { public bool Success { get; set; } public string? Error { get; set; } public bool RequiresVerification { get; set; } }
    public class VerifyRequest { public string Email { get; set; } = ""; public string Code { get; set; } = ""; public string Type { get; set; } = "register"; }
    public class LoginRequest { public string LoginOrEmail { get; set; } = ""; public string Password { get; set; } = ""; }
    public class LoginResponse { public bool Success { get; set; } public string? Token { get; set; } public string? Login { get; set; } public int MaxScore { get; set; } public string? Error { get; set; } }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Login == request.Login))
            return Ok(new RegisterResponse { Success = false, Error = "Логин занят" });

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Ok(new RegisterResponse { Success = false, Error = "Email уже зарегистрирован" });

        var code = _verification.GenerateCode();
        await _verification.SaveCode(request.Email, code, "register");
        await _email.SendCode(request.Email, code);

        HttpContext.Items[$"temp_{request.Email}"] = $"{request.Login}|{_hasher.Hash(request.Password)}";

        return Ok(new RegisterResponse { Success = true, RequiresVerification = true });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request)
    {
        var isValid = await _verification.VerifyCode(request.Email, request.Code, "register");
        if (!isValid)
            return BadRequest(new { error = "Неверный код" });

        var tempData = HttpContext.Items[$"temp_{request.Email}"]?.ToString();
        if (string.IsNullOrEmpty(tempData))
            return BadRequest(new { error = "Данные не найдены" });

        var parts = tempData.Split('|');
        var user = new User
        {
            Login = parts[0],
            Email = request.Email,
            PasswordHash = parts[1],
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user.Id, user.Login);
        return Ok(new LoginResponse { Success = true, Token = token, Login = user.Login, MaxScore = user.MaxScore });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Login == request.LoginOrEmail || u.Email == request.LoginOrEmail);

        if (user == null || !user.IsEmailVerified)
            return Unauthorized(new LoginResponse { Success = false, Error = "Неверный логин или пароль" });

        if (!_hasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new LoginResponse { Success = false, Error = "Неверный логин или пароль" });

        var token = _jwt.GenerateToken(user.Id, user.Login);
        return Ok(new LoginResponse { Success = true, Token = token, Login = user.Login, MaxScore = user.MaxScore });
    }
}