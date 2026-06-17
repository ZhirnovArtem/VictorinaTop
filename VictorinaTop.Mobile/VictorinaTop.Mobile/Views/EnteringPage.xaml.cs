using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class EnteringPage : ContentPage
{
    private readonly Entry _loginEntry;
    private readonly Entry _passwordEntry;
    private readonly Label _errorLabel;
    private readonly Button _enterBtn;
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;

    public EnteringPage(ApiService api, PreferencesService prefs)
    {
        _prefs = prefs;
        _api = api;
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
            Text = "Вход",
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
            Text = "🔐 Вход в аккаунт",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        _loginEntry = new Entry
        {
            Placeholder = "Логин или Email",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 55
        };

        _passwordEntry = new Entry
        {
            Placeholder = "Пароль",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            IsPassword = true,
            HeightRequest = 55
        };

        var forgotPasswordLabel = new Label
        {
            Text = "Забыли пароль?",
            TextColor = Color.FromArgb("#4A90E2"),
            TextDecorations = TextDecorations.Underline,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 5, 0, 0)
        };
        forgotPasswordLabel.GestureRecognizers.Add(new TapGestureRecognizer());
        ((TapGestureRecognizer)forgotPasswordLabel.GestureRecognizers[0]).Tapped += OnForgotPasswordTapped;

        _enterBtn = new Button
        {
            Text = "Войти",
            BackgroundColor = Color.FromArgb("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 18,
            CornerRadius = 15,
            HeightRequest = 55,
            Margin = new Thickness(0, 20, 0, 0)
        };
        _enterBtn.Clicked += OnEnterClicked;

        _errorLabel = new Label
        {
            TextColor = Color.FromArgb("#FF6B6B"),
            HorizontalOptions = LayoutOptions.Center,
            IsVisible = false
        };

        var formLayout = new VerticalStackLayout
        {
            Spacing = 15,
            Padding = new Thickness(30),
            VerticalOptions = LayoutOptions.Center,
            Children = { mainTitleLabel, _loginEntry, _passwordEntry, forgotPasswordLabel, _enterBtn, _errorLabel }
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

    private async void OnEnterClicked(object sender, EventArgs e)
    {
        var loginOrEmail = _loginEntry.Text?.Trim();
        var password = _passwordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(loginOrEmail) || string.IsNullOrEmpty(password))
        {
            _errorLabel.Text = "Заполните все поля!";
            _errorLabel.IsVisible = true;
            return;
        }

        var (success, token, login, maxScore) = await _api.Login(loginOrEmail, password);

        if (success)
        {
            await Navigation.PushAsync(new MenuPage(_api,_prefs));
        }
        else
        {
            _errorLabel.Text = "Неверный логин или пароль";
            _errorLabel.IsVisible = true;
        }
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        var email = await DisplayPromptAsync("Восстановление пароля",
            "Введите ваш email:", "Отправить", "Отмена", keyboard: Keyboard.Email);

        if (string.IsNullOrEmpty(email)) return;

        var (success, error) = await _api.ForgotPassword(email);

        if (success)
        {
            await Navigation.PushAsync(new ResetPasswordPage(_api, email));
        }
        else
        {
            await DisplayAlert("Ошибка", string.IsNullOrEmpty(error) ? "Email не найден" : error, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}