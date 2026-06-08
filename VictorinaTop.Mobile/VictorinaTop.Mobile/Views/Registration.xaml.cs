using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public partial class RegistrationPage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;

    public RegistrationPage()
    {
        InitializeComponent();
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var login = LoginEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Заполните все поля!";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 4)
        {
            ErrorLabel.Text = "Пароль должен быть не менее 4 символов!";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            ErrorLabel.Text = "Введите корректный email!";
            ErrorLabel.IsVisible = true;
            return;
        }

        var (success, error, requiresVerification) = await _api.Register(login, email, password);

        if (success && requiresVerification)
        {
            await Navigation.PushAsync(new EmailVerificationPage(email));
        }
        else
        {
            ErrorLabel.Text = error;
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}