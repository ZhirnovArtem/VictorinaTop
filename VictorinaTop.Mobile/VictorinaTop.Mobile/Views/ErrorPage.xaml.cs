using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class ErrorPage : ContentPage
{
    private readonly ApiService _api;
    private readonly PreferencesService _prefs;
    private readonly Label _errorMessageLabel;
    private Button _retryBtn;

    public ErrorPage(string errorMessage = null)
    {
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);

        BackgroundColor = Color.FromArgb("#1A1A2E");
        NavigationPage.SetHasBackButton(this, false);

        var errorIcon = new Label
        {
            Text = "⚠️",
            FontSize = 80,
            TextColor = Color.FromArgb("#FF6B6B"),
            HorizontalOptions = LayoutOptions.Center
        };

        var titleLabel = new Label
        {
            Text = "Ошибка подключения",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 10)
        };

        _errorMessageLabel = new Label
        {
            Text = errorMessage ?? "Не удалось подключиться к серверу.\nПроверьте интернет-соединение.",
            FontSize = 16,
            TextColor = Color.FromArgb("#CCCCCC"),
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(30, 0)
        };

        _retryBtn = new Button
        {
            Text = "🔄 Попробовать снова",
            BackgroundColor = Color.FromArgb("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 15,
            HeightRequest = 55
        };
        _retryBtn.Clicked += OnRetryClicked;

        var homeBtn = new Button
        {
            Text = "🏠 На главную",
            BackgroundColor = Color.FromArgb("#34495E"),
            TextColor = Colors.White,
            FontSize = 16,
            CornerRadius = 15,
            HeightRequest = 50
        };
        homeBtn.Clicked += OnHomeClicked;

        var offlineBtn = new Button
        {
            Text = "📝 Попробовать без интернета (офлайн режим)",
            BackgroundColor = Color.FromArgb("#9B59B6"),
            TextColor = Colors.White,
            FontSize = 14,
            CornerRadius = 15,
            HeightRequest = 45
        };
        offlineBtn.Clicked += OnOfflineClicked;

        var buttonsLayout = new VerticalStackLayout
        {
            Spacing = 15,
            Padding = new Thickness(30),
            Children = { _retryBtn, homeBtn, offlineBtn }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        grid.Add(errorIcon, 0, 1);
        grid.Add(titleLabel, 0, 2);
        grid.Add(_errorMessageLabel, 0, 3);
        grid.Add(buttonsLayout, 0, 4);

        Content = grid;
    }

    private async void OnRetryClicked(object sender, EventArgs e)
    {
        _retryBtn.IsEnabled = false;
        _retryBtn.Text = "Проверка...";

        var isConnected = await _api.TestConnection();

        if (isConnected)
        {
            var savedToken = await _prefs.GetToken();

            if (!string.IsNullOrEmpty(savedToken))
            {
                await Navigation.PushAsync(new MenuPage());
            }
            else
            {
                await Navigation.PopToRootAsync();
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Сервер всё ещё недоступен", "OK");
            _retryBtn.IsEnabled = true;
            _retryBtn.Text = "🔄 Попробовать снова";
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnOfflineClicked(object sender, EventArgs e)
    {
        var cache = new LocalDatabaseService();
        var cachedThemes = cache.GetCachedThemes();

        if (cachedThemes.Count > 0)
        {
            var result = await DisplayAlert("Офлайн режим",
                $"Найдено {cachedThemes.Count} викторин в кэше. Перейти в офлайн режим?",
                "Да", "Отмена");

            if (result)
            {
                await Navigation.PushAsync(new MenuPage());
            }
        }
        else
        {
            await DisplayAlert("Нет данных",
                "Нет сохранённых викторин. Подключитесь к интернету.",
                "OK");
        }
    }
}