using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class EmailVerificationPage : ContentPage
{
    private readonly Entry _codeEntry;
    private readonly Label _errorLabel;
    private readonly Label _emailLabel;
    private readonly Button _verifyBtn;
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly string _email;
    private readonly string _login;
    private readonly string _password;

    public EmailVerificationPage(string email, string login, string password)
    {
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
        _email = email;
        _login = login;
        _password = password;

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
            Text = "Подтверждение",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.Center
        };

        var headerLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            BackgroundColor = Color.FromArgb("#16213E"),
            Padding = new Thickness(15),
            Children = { backButton, titleLabel }
        };

        var mainTitleLabel = new Label
        {
            Text = "📧 Подтверждение email",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center
        };

        _emailLabel = new Label
        {
            Text = $"Код отправлен на {email}",
            FontSize = 14,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var infoLabel = new Label
        {
            Text = "Введите код из письма",
            FontSize = 14,
            TextColor = Color.FromArgb("#888888"),
            HorizontalOptions = LayoutOptions.Center
        };

        _codeEntry = new Entry
        {
            Placeholder = "Код подтверждения",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            FontSize = 20,
            Keyboard = Keyboard.Numeric,
            MaxLength = 6,
            HorizontalTextAlignment = TextAlignment.Center,
            HeightRequest = 60
        };

        _verifyBtn = new Button
        {
            Text = "✅ Подтвердить",
            BackgroundColor = Color.FromArgb("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 18,
            CornerRadius = 15,
            HeightRequest = 55
        };
        _verifyBtn.Clicked += OnVerifyClicked;

        _errorLabel = new Label
        {
            TextColor = Color.FromArgb("#FF6B6B"),
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false
        };

        var formLayout = new VerticalStackLayout
        {
            Spacing = 20,
            Padding = new Thickness(30),
            VerticalOptions = LayoutOptions.Center,
            Children = { mainTitleLabel, _emailLabel, infoLabel, _codeEntry, _verifyBtn, _errorLabel }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        grid.Add(headerLayout, 0, 0);
        grid.Add(formLayout, 0, 1);

        Content = grid;
        BackgroundColor = Color.FromArgb("#1A1A2E");
        NavigationPage.SetHasBackButton(this, false);
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        var code = _codeEntry.Text?.Trim();

        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            _errorLabel.Text = "Введите 6-значный код!";
            _errorLabel.IsVisible = true;
            return;
        }

        var (success, token, login, maxScore) = await _api.Verify(_email, code, _login, _password);

        if (success)
        {
            await Navigation.PushAsync(new MenuPage());
        }
        else
        {
            _errorLabel.Text = "Неверный код!";
            _errorLabel.IsVisible = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}