using VictorinaTop.Mobile.Services;
using VictorinaTop.Mobile.Views;

namespace VictorinaTop.Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;

    public MainPage()
    {
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);

        BuildUI();
    }

    private void BuildUI()
    {
        var titleLabel = new Label
        {
            Text = "🏆 VICTORINA TOP 🏆",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center
        };

        var subtitleLabel = new Label
        {
            Text = "Проверь свои знания!",
            FontSize = 18,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        var loginBtn = new Button
        {
            Text = "🔐 Вход",
            BackgroundColor = Color.FromArgb("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 18,
            CornerRadius = 15,
            HeightRequest = 55,
            WidthRequest = 250
        };
        loginBtn.Clicked += OnLoginClicked;

        var registerBtn = new Button
        {
            Text = "📝 Регистрация",
            BackgroundColor = Color.FromArgb("#50C878"),
            TextColor = Colors.White,
            FontSize = 18,
            CornerRadius = 15,
            HeightRequest = 55,
            WidthRequest = 250
        };
        registerBtn.Clicked += OnRegisterClicked;

        var layout = new VerticalStackLayout
        {
            Spacing = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { titleLabel, subtitleLabel, loginBtn, registerBtn }
        };

        Content = layout;
        BackgroundColor = Color.FromArgb("#1A1A2E");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EnteringPage());
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistrationPage());
    }
}