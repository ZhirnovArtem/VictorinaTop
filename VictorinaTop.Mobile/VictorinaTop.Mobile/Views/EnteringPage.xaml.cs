using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public partial class EnteringPage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;

    public EnteringPage()
    {
        InitializeComponent();
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
    }

    private async void OnEnterClicked(object sender, EventArgs e)
    {
        var loginOrEmail = LoginEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(loginOrEmail) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Заполните все поля!";
            ErrorLabel.IsVisible = true;
            return;
        }

        var (success, token, login, maxScore) = await _api.Login(loginOrEmail, password);

        if (success)
        {
            await Navigation.PushAsync(new MenuPage());
        }
        else
        {
            ErrorLabel.Text = "Неверный логин или пароль";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        var email = await DisplayPromptAsync("Восстановление пароля",
            "Введите ваш email:", "Отправить", "Отмена", keyboard: Keyboard.Email);

        if (!string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Информация", "Код отправлен на email", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}