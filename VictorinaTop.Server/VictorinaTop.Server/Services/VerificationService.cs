using Microsoft.EntityFrameworkCore;
using VictorinaTop.Server.Data;
using VictorinaTop.Server.Models;

namespace VictorinaTop.Server.Services;

public class VerificationService
{
    private readonly AppDbContext _db;
    private readonly Random _random = new();

    public VerificationService(AppDbContext db) => _db = db;

    public string GenerateCode() => _random.Next(100000, 999999).ToString();

    public async Task SaveCode(string email, string code, string type)
    {
        _db.VerificationCodes.Add(new VerificationCode
        {
            Email = email,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Type = type,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> VerifyCode(string email, string code, string type)
    {
        var record = await _db.VerificationCodes
            .FirstOrDefaultAsync(v => v.Email == email && v.Code == code && v.Type == type && !v.IsUsed);

        if (record == null || record.ExpiresAt < DateTime.UtcNow)
            return false;

        record.IsUsed = true;
        await _db.SaveChangesAsync();
        return true;
    }
}