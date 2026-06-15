using VictorinaTop.Mobile.Models;
using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class CreateGamePage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly List<Question> _questions = new();

    private Entry _themeEntry;
    private Entry _questionEntry;
    private Entry _correctEntry;
    private Entry _optionAEntry, _optionBEntry, _optionCEntry, _optionDEntry;
    private Picker _statusPicker;
    private Label _counterLabel;
    private Label _messageLabel;
    private Button _addQuestionBtn, _saveQuizBtn;

    public CreateGamePage()
    {
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);

        Title = "Создание викторины";
        BackgroundColor = Color.FromArgb("#1A1A2E");

        BuildUI();
        UpdateCounter();
    }

    private void BuildUI()
    {
        var backButton = new Button
        {
            Text = "← Назад",
            BackgroundColor = Color.FromArgb("#E74C3C"),
            TextColor = Colors.White,
            CornerRadius = 10,
            WidthRequest = 80,
            HeightRequest = 40
        };
        backButton.Clicked += OnBackClicked;

        var titleLabel = new Label
        {
            Text = "Создание викторины",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.Center
        };

        _counterLabel = new Label
        {
            Text = "0",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var counterFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#4A90E2"),
            CornerRadius = 20,
            Padding = new Thickness(10, 5),
            HasShadow = false,
            Content = _counterLabel
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = 15
        };
        headerGrid.Add(backButton, 0, 0);
        headerGrid.Add(titleLabel, 1, 0);
        headerGrid.Add(counterFrame, 2, 0);

        var headerLayout = new StackLayout
        {
            BackgroundColor = Color.FromArgb("#16213E"),
            Children = { headerGrid }
        };

        _themeEntry = new Entry
        {
            Placeholder = "Название викторины",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _questionEntry = new Entry
        {
            Placeholder = "Введите вопрос",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _statusPicker = new Picker
        {
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            Title = "Выберите сложность",
            HeightRequest = 50
        };
        _statusPicker.Items.Add("Easy");
        _statusPicker.Items.Add("Medium");
        _statusPicker.Items.Add("Hard");
        _statusPicker.SelectedIndex = 0;

        _correctEntry = new Entry
        {
            Placeholder = "Правильный ответ",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _optionAEntry = new Entry
        {
            Placeholder = "Вариант A",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _optionBEntry = new Entry
        {
            Placeholder = "Вариант B",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _optionCEntry = new Entry
        {
            Placeholder = "Вариант C",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _optionDEntry = new Entry
        {
            Placeholder = "Вариант D",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 50
        };

        _addQuestionBtn = new Button
        {
            Text = "➕ Добавить вопрос",
            BackgroundColor = Color.FromArgb("#3498DB"),
            TextColor = Colors.White,
            CornerRadius = 15,
            HeightRequest = 50
        };
        _addQuestionBtn.Clicked += OnAddQuestion;

        _saveQuizBtn = new Button
        {
            Text = "💾 Сохранить викторину",
            BackgroundColor = Color.FromArgb("#50C878"),
            TextColor = Colors.White,
            CornerRadius = 15,
            HeightRequest = 50
        };
        _saveQuizBtn.Clicked += OnSaveQuiz;

        _messageLabel = new Label
        {
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false
        };

        var formLayout = new VerticalStackLayout
        {
            Spacing = 15,
            Padding = 20,
            Children =
            {
                new Label { Text = "Название викторины", TextColor = Colors.White, FontSize = 14, FontAttributes = FontAttributes.Bold },
                _themeEntry,
                new BoxView { HeightRequest = 2, BackgroundColor = Color.FromArgb("#2D2D44"), Margin = new Thickness(0, 10) },
                new Label { Text = "➕ НОВЫЙ ВОПРОС", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FFD700"), HorizontalOptions = LayoutOptions.Center },
                new Label { Text = "Текст вопроса", TextColor = Colors.White, FontSize = 14, FontAttributes = FontAttributes.Bold },
                _questionEntry,
                new Label { Text = "Сложность", TextColor = Colors.White, FontSize = 14, FontAttributes = FontAttributes.Bold },
                _statusPicker,
                new Label { Text = "✅ Правильный ответ", TextColor = Color.FromArgb("#50C878"), FontSize = 13, FontAttributes = FontAttributes.Bold },
                _correctEntry,
                new Label { Text = "A", TextColor = Color.FromArgb("#4A90E2"), FontSize = 13, FontAttributes = FontAttributes.Bold },
                _optionAEntry,
                new Label { Text = "B", TextColor = Color.FromArgb("#4A90E2"), FontSize = 13, FontAttributes = FontAttributes.Bold },
                _optionBEntry,
                new Label { Text = "C", TextColor = Color.FromArgb("#4A90E2"), FontSize = 13, FontAttributes = FontAttributes.Bold },
                _optionCEntry,
                new Label { Text = "D", TextColor = Color.FromArgb("#4A90E2"), FontSize = 13, FontAttributes = FontAttributes.Bold },
                _optionDEntry,
                new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    ColumnSpacing = 10,
                },
                _messageLabel
            }
        };


        var scrollView = new ScrollView { Content = formLayout };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        grid.Add(headerLayout, 0, 0);
        grid.Add(scrollView, 0, 1);

        Content = grid;
        NavigationPage.SetHasBackButton(this, false);
    }

    private void UpdateCounter()
    {
        _counterLabel.Text = _questions.Count.ToString();
    }

    private async void OnAddQuestion(object sender, EventArgs e)
    {
        var questionText = _questionEntry.Text?.Trim();
        var correctAnswer = _correctEntry.Text?.Trim();
        var optionA = _optionAEntry.Text?.Trim();
        var optionB = _optionBEntry.Text?.Trim();
        var optionC = _optionCEntry.Text?.Trim();
        var optionD = _optionDEntry.Text?.Trim();
        var status = _statusPicker.SelectedItem?.ToString() ?? "Easy";

        if (string.IsNullOrEmpty(questionText) || string.IsNullOrEmpty(correctAnswer) ||
            string.IsNullOrEmpty(optionA) || string.IsNullOrEmpty(optionB) ||
            string.IsNullOrEmpty(optionC) || string.IsNullOrEmpty(optionD))
        {
            await ShowMessage("Заполните все поля!", false);
            return;
        }

        _questions.Add(new Question
        {
            Text = questionText,
            CorrectAnswer = correctAnswer,
            OptionA = optionA,
            OptionB = optionB,
            OptionC = optionC,
            OptionD = optionD,
            Status = status
        });

        UpdateCounter();
        await ShowMessage($"Вопрос {_questions.Count} добавлен!", true);

        _questionEntry.Text = "";
        _correctEntry.Text = "";
        _optionAEntry.Text = "";
        _optionBEntry.Text = "";
        _optionCEntry.Text = "";
        _optionDEntry.Text = "";
        _questionEntry.Focus();
    }

    private async void OnSaveQuiz(object sender, EventArgs e)
    {
        var themeName = _themeEntry.Text?.Trim();

        if (string.IsNullOrEmpty(themeName))
        {
            await ShowMessage("Введите название викторины!", false);
            return;
        }

        if (_questions.Count == 0)
        {
            await ShowMessage("Добавьте хотя бы один вопрос!", false);
            return;
        }

        var success = await _api.CreateTheme(themeName);

        if (!success)
        {
            await ShowMessage("Не удалось создать викторину!", false);
            return;
        }

        await DisplayAlert("Успех!", $"Викторина \"{themeName}\" создана!\nДобавлено вопросов: {_questions.Count}", "OK");
        await Navigation.PopAsync();
    }

    private async Task ShowMessage(string message, bool isSuccess)
    {
        _messageLabel.Text = message;
        _messageLabel.TextColor = isSuccess ? Colors.LightGreen : Colors.OrangeRed;
        _messageLabel.IsVisible = true;

        await Task.Delay(2000);
        _messageLabel.IsVisible = false;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_questions.Count > 0)
        {
            bool confirm = await DisplayAlert("Выход",
                $"У вас есть {_questions.Count} несохранённых вопросов. Выйти?",
                "Да", "Нет");
            if (!confirm) return;
        }

        await Navigation.PopAsync();
    }
}