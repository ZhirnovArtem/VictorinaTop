using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class ResetPasswordPage : ContentPage
{
    private readonly ApiService _api;
    private readonly string _email;
    private readonly Entry _codeEntry;
    private readonly Entry _passwordEntry;
    private readonly Entry _confirmPasswordEntry;
    private readonly Label _errorLabel;

    public ResetPasswordPage(ApiService api, string email)
    {
        _api = api;
        _email = email;

        BackgroundColor = Color.FromArgb("#1A1A2E");

        var titleLabel = new Label
        {
            Text = "🔑 Новый пароль",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var infoLabel = new Label
        {
            Text = $"Код отправлен на {email}",
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        _codeEntry = new Entry
        {
            Placeholder = "Код из письма",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            Keyboard = Keyboard.Numeric,
            HeightRequest = 55
        };

        _passwordEntry = new Entry
        {
            Placeholder = "Новый пароль",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            IsPassword = true,
            HeightRequest = 55
        };

        _confirmPasswordEntry = new Entry
        {
            Placeholder = "Подтвердите пароль",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            IsPassword = true,
            HeightRequest = 55
        };

        var confirmBtn = new Button
        {
            Text = "Сменить пароль",
            BackgroundColor = Color.FromArgb("#50C878"),
            TextColor = Colors.White,
            FontSize = 18,
            CornerRadius = 15,
            HeightRequest = 55,
            Margin = new Thickness(0, 20, 0, 0)
        };
        confirmBtn.Clicked += OnConfirmClicked;

        _errorLabel = new Label
        {
            TextColor = Color.FromArgb("#FF6B6B"),
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false
        };

        Content = new VerticalStackLayout
        {
            Spacing = 15,
            Padding = new Thickness(30),
            VerticalOptions = LayoutOptions.Center,
            Children = { titleLabel, infoLabel, _codeEntry, _passwordEntry, _confirmPasswordEntry, confirmBtn, _errorLabel }
        };
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        var code = _codeEntry.Text?.Trim();
        var password = _passwordEntry.Text?.Trim();
        var confirm = _confirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            _errorLabel.Text = "Заполните все поля!";
            _errorLabel.IsVisible = true;
            return;
        }

        if (password != confirm)
        {
            _errorLabel.Text = "Пароли не совпадают!";
            _errorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 4)
        {
            _errorLabel.Text = "Пароль должен быть не менее 4 символов!";
            _errorLabel.IsVisible = true;
            return;
        }

        var (success, error) = await _api.ResetPassword(_email, code, password);

        if (success)
        {
            await DisplayAlert("✅", "Пароль успешно изменён!", "OK");
            await Navigation.PopToRootAsync();
        }
        else
        {
            _errorLabel.Text = string.IsNullOrEmpty(error) ? "Неверный код" : error;
            _errorLabel.IsVisible = true;
        }
    }
}