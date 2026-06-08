using VictorinaTop.Mobile.Models;
using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public partial class GamePage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly string _themeName;
    private readonly int _themeId;
    private readonly List<Question> _questions;
    private int _currentIndex;
    private int _score;

    public GamePage(string themeName, List<Question> questions, int themeId)
    {
        InitializeComponent();
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
        _themeName = themeName;
        _themeId = themeId;
        _questions = questions;
        _currentIndex = 0;
        _score = 0;

        Title = themeName;
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (_currentIndex >= _questions.Count)
        {
            EndGame();
            return;
        }

        var q = _questions[_currentIndex];
        QuestionLabel.Text = q.Text;
        BtnA.Text = $"A. {q.OptionA}";
        BtnB.Text = $"B. {q.OptionB}";
        BtnC.Text = $"C. {q.OptionC}";
        BtnD.Text = $"D. {q.OptionD}";

        CounterLabel.Text = $"Вопрос {_currentIndex + 1} из {_questions.Count}";
        ScoreLabel.Text = $"Счёт: {_score}";
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var answer = btn?.Text?.Substring(3);
        var correct = _questions[_currentIndex].CorrectAnswer;

        if (answer == correct)
        {
            _score += 10;
            await DisplayAlert("✅", "Правильно! +10 очков", "Далее");
        }
        else
        {
            await DisplayAlert("❌", $"Правильный ответ: {correct}", "Далее");
        }

        _currentIndex++;
        ShowQuestion();
    }

    private async void EndGame()
    {
        GameOverPanel.IsVisible = true;

        string medal = _score >= 80 ? "🏆 Золото!" :
                       _score >= 50 ? "🥈 Серебро!" :
                       _score >= 30 ? "🥉 Бронза!" :
                       "👍 Попробуйте ещё!";

        FinalScoreLabel.Text = $"Ваш счёт: {_score}\n{medal}";

        await _api.SubmitScore(_themeId, _score);
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}