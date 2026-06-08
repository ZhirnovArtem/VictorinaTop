using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public partial class EmailVerificationPage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly string _email;

    public EmailVerificationPage(string email)
    {
        InitializeComponent();
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
        _email = email;
        EmailLabel.Text = $"Код отправлен на {email}";
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        var code = CodeEntry.Text?.Trim();

        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            ErrorLabel.Text = "Введите 6-значный код!";
            ErrorLabel.IsVisible = true;
            return;
        }

        var (success, token, login, maxScore) = await _api.Verify(_email, code);

        if (success)
        {
            await Navigation.PushAsync(new MenuPage());
        }
        else
        {
            ErrorLabel.Text = "Неверный код!";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}