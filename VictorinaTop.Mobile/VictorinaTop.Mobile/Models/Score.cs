namespace VictorinaTop.Mobile.Models;

public class Score
{
    public int Id { get; set; }
    public int Points { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public DateTime AchievedAt { get; set; }
}