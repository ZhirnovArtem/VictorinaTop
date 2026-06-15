namespace VictorinaTop.Server.Models;

public class Score
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ThemeName { get; set; } = string.Empty; 
    public int Points { get; set; }
    public DateTime AchievedAt { get; set; }
}