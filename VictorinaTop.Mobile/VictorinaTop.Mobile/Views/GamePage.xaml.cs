using VictorinaTop.Mobile.Models;
using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class GamePage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly string _themeName;
    private readonly int _themeId;
    private readonly List<Question> _questions;
    private int _currentIndex;
    private int _score;

    private Label _counterLabel;
    private Label _scoreLabel;
    private Label _questionLabel;
    private Button _btnA, _btnB, _btnC, _btnD;
    private Frame _gameOverPanel;
    private Label _finalScoreLabel;

    public GamePage(string themeName, List<Question> questions, int themeId, ApiService api, PreferencesService prefs)
    {
        _prefs = prefs;
        _api = api;
        _themeName = themeName;
        _themeId = themeId;
        _questions = questions;
        _currentIndex = 0;
        _score = 0;

        Title = themeName;
        BackgroundColor = Color.FromArgb("#1A1A2E");

        BuildUI();
        ShowQuestion();
    }

    private void BuildUI()
    {
        _counterLabel = new Label
        {
            Text = "Вопрос 1",
            FontSize = 16,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };

        _scoreLabel = new Label
        {
            Text = "Счёт: 0",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.EndAndExpand,
            VerticalOptions = LayoutOptions.Center
        };

        var headerLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            BackgroundColor = Color.FromArgb("#16213E"),
            Padding = new Thickness(20, 15),
            Children = { _counterLabel, _scoreLabel }
        };

        _questionLabel = new Label
        {
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var questionFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#16213E"),
            CornerRadius = 15,
            HasShadow = false,
            Padding = 20,
            Content = _questionLabel
        };

        _btnA = new Button
        {
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            FontSize = 16,
            CornerRadius = 12,
            HeightRequest = 55
        };
        _btnA.Clicked += OnAnswerClicked;

        _btnB = new Button
        {
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            FontSize = 16,
            CornerRadius = 12,
            HeightRequest = 55
        };
        _btnB.Clicked += OnAnswerClicked;

        _btnC = new Button
        {
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            FontSize = 16,
            CornerRadius = 12,
            HeightRequest = 55
        };
        _btnC.Clicked += OnAnswerClicked;

        _btnD = new Button
        {
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            FontSize = 16,
            CornerRadius = 12,
            HeightRequest = 55
        };
        _btnD.Clicked += OnAnswerClicked;

        var gameLayout = new VerticalStackLayout
        {
            Spacing = 20,
            Padding = 20,
            VerticalOptions = LayoutOptions.Center,
            Children = { questionFrame, _btnA, _btnB, _btnC, _btnD }
        };

        _finalScoreLabel = new Label
        {
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#50C878"),
            HorizontalOptions = LayoutOptions.Center
        };

        var menuButton = new Button
        {
            Text = "В меню",
            BackgroundColor = Color.FromArgb("#4A90E2"),
            CornerRadius = 15,
            HeightRequest = 50
        };
        menuButton.Clicked += OnMenuClicked;

        var gameOverLayout = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 20,
            Padding = 30,
            Children =
            {
                new Label
                {
                    Text = "🏆 ИГРА ОКОНЧЕНА 🏆",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#FFD700"),
                    HorizontalOptions = LayoutOptions.Center
                },
                _finalScoreLabel,
                menuButton
            }
        };

        _gameOverPanel = new Frame
        {
            BackgroundColor = Color.FromArgb("#CC000000"),
            IsVisible = false,
            VerticalOptions = LayoutOptions.Fill,   
            HorizontalOptions = LayoutOptions.Fill,
            Content = gameOverLayout
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        grid.Add(headerLayout, 0, 0);
        grid.Add(gameLayout, 0, 1);
        grid.Add(_gameOverPanel, 0, 1);

        Content = grid;
    }

    private void ShowQuestion()
    {
        if (_currentIndex >= _questions.Count)
        {
            EndGame();
            return;
        }

        var q = _questions[_currentIndex];
        _questionLabel.Text = q.Text;
        _btnA.Text = $"A. {q.OptionA}";
        _btnB.Text = $"B. {q.OptionB}";
        _btnC.Text = $"C. {q.OptionC}";
        _btnD.Text = $"D. {q.OptionD}";

        _counterLabel.Text = $"Вопрос {_currentIndex + 1} из {_questions.Count}";
        _scoreLabel.Text = $"Счёт: {_score}";
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var answer = btn?.Text?.Substring(3);
        var correct = _questions[_currentIndex].CorrectAnswer;

        if (answer == correct)
        {
            _score += 10;
            _scoreLabel.Text = $"Счёт: {_score}";  
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
        _gameOverPanel.IsVisible = true;

        string medal = _score >= 80 ? "🏆 Золото!" :
                       _score >= 50 ? "🥈 Серебро!" :
                       _score >= 30 ? "🥉 Бронза!" :
                       "👍 Попробуйте ещё!";

        _finalScoreLabel.Text = $"Ваш счёт: {_score}\n{medal}";

        Console.WriteLine($"[EndGame] Submitting score: themeId={_themeId}, points={_score}");
        var result = await _api.SubmitScore(_themeId, _score);
        Console.WriteLine($"[EndGame] SubmitScore result: {result}");
    }
    private async void OnMenuClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}