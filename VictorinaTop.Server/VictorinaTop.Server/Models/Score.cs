namespace VictorinaTop.Server.Models;

public class Score
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ThemeId { get; set; }
    public int Points { get; set; }
    public DateTime AchievedAt { get; set; }
    public User? User { get; set; }
    public Theme? Theme { get; set; }
}