using VictorinaTop.Mobile.Models;
using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public partial class CreateGamePage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly List<Question> _questions;

    public CreateGamePage()
    {
        InitializeComponent();
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
        _questions = new List<Question>();
        StatusPicker.SelectedIndex = 0;
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        CounterLabel.Text = _questions.Count.ToString();
    }

    private async void OnAddQuestion(object sender, EventArgs e)
    {
        var questionText = QuestionEntry.Text?.Trim();
        var correctAnswer = CorrectEntry.Text?.Trim();
        var optionA = OptionAEntry.Text?.Trim();
        var optionB = OptionBEntry.Text?.Trim();
        var optionC = OptionCEntry.Text?.Trim();
        var optionD = OptionDEntry.Text?.Trim();
        var status = StatusPicker.SelectedItem?.ToString() ?? "Easy";

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

        QuestionEntry.Text = "";
        CorrectEntry.Text = "";
        OptionAEntry.Text = "";
        OptionBEntry.Text = "";
        OptionCEntry.Text = "";
        OptionDEntry.Text = "";
        QuestionEntry.Focus();
    }

    private async void OnSaveQuiz(object sender, EventArgs e)
    {
        var themeName = ThemeEntry.Text?.Trim();

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
        MessageLabel.Text = message;
        MessageLabel.TextColor = isSuccess ? Colors.LightGreen : Colors.OrangeRed;
        MessageLabel.IsVisible = true;

        await Task.Delay(2000);
        MessageLabel.IsVisible = false;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}