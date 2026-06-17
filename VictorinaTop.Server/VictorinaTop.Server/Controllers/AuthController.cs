using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public bool RequiresVerification { get; set; }
    }

    public class VerifyRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "register";
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string LoginOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Login { get; set; }
        public int MaxScore { get; set; }
        public string? Error { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Ok(new { success = false, error = "Email не найден" });

        var code = _verification.GenerateCode();
        await _verification.SaveCode(request.Email, code, "reset");
        await _email.SendCode(request.Email, code);

        return Ok(new { success = true });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var isValid = await _verification.VerifyCode(request.Email, request.Code, "reset");
        if (!isValid)
            return BadRequest(new { success = false, error = "Неверный или истёкший код" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return BadRequest(new { success = false, error = "Пользователь не найден" });

        if (request.NewPassword.Length < 4)
            return BadRequest(new { success = false, error = "Пароль должен быть не менее 4 символов" });

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }

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

        return Ok(new RegisterResponse { Success = true, RequiresVerification = true });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request)
    {
        var isValid = await _verification.VerifyCode(request.Email, request.Code, "register");
        if (!isValid)
            return BadRequest(new { error = "Неверный код" });

        if (await _db.Users.AnyAsync(u => u.Login == request.Login))
            return BadRequest(new { error = "Логин занят" });

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { error = "Email уже зарегистрирован" });

        var user = new User
        {
            Login = request.Login,
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
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