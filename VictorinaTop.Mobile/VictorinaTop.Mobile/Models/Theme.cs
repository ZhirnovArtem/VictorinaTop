namespace VictorinaTop.Mobile.Models;

public class Theme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorDisplay => $"Автор: {Author}";
}