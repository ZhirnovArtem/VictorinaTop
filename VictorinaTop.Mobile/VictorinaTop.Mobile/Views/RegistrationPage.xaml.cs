using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class RegistrationPage : ContentPage
{
    private readonly Entry _loginEntry;
    private readonly Entry _emailEntry;
    private readonly Entry _passwordEntry;
    private readonly Label _errorLabel;
    private readonly Button _registerBtn;
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;

    public RegistrationPage()
    {
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);

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
            Text = "Регистрация",
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
            Text = "📝 Создание аккаунта",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        _loginEntry = new Entry
        {
            Placeholder = "Логин",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            HeightRequest = 55
        };

        _emailEntry = new Entry
        {
            Placeholder = "Email",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            Keyboard = Keyboard.Email,
            HeightRequest = 55
        };

        _passwordEntry = new Entry
        {
            Placeholder = "Пароль (мин. 4 символа)",
            BackgroundColor = Color.FromArgb("#2D2D44"),
            TextColor = Colors.White,
            IsPassword = true,
            HeightRequest = 55
        };

        _registerBtn = new Button
        {
            Text = "📧 Зарегистрироваться",
            BackgroundColor = Color.FromArgb("#50C878"),
            TextColor = Colors.White,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 15,
            HeightRequest = 55,
            Margin = new Thickness(0, 20, 0, 0)
        };
        _registerBtn.Clicked += OnRegisterClicked;

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
            Children = { mainTitleLabel, _loginEntry, _emailEntry, _passwordEntry, _registerBtn, _errorLabel }
        };

        var scrollView = new ScrollView { Content = formLayout };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        grid.Add(headerLayout, 0, 0);
        grid.Add(scrollView, 0, 1);

        Content = grid;
        BackgroundColor = Color.FromArgb("#1A1A2E");
        NavigationPage.SetHasBackButton(this, false);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var login = _loginEntry.Text?.Trim();
        var email = _emailEntry.Text?.Trim();
        var password = _passwordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            _errorLabel.Text = "Заполните все поля!";
            _errorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 4)
        {
            _errorLabel.Text = "Пароль должен быть не менее 4 символов!";
            _errorLabel.IsVisible = true;
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            _errorLabel.Text = "Введите корректный email!";
            _errorLabel.IsVisible = true;
            return;
        }

        var (success, error, requiresVerification) = await _api.Register(login, email, password);

        if (success && requiresVerification)
        {
            await Navigation.PushAsync(new EmailVerificationPage(email));
        }
        else
        {
            _errorLabel.Text = error;
            _errorLabel.IsVisible = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}