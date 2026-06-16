public class CreateQuestionRequest
{
    public int ThemeId { get; set; }          
    public string ThemeName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Status { get; set; } = "Easy";
    public string CorrectAnswer { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}