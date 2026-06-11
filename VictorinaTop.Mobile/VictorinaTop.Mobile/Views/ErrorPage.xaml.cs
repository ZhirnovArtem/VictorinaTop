using VictorinaTop.Mobile.Services;
using VictorinaTop.Mobile.Models;

namespace VictorinaTop.Mobile.Views;

public partial class ErrorPage : ContentPage
{
    private readonly ApiService _api;
    private readonly LocalDatabaseService _cache;
    private readonly string _errorDetails;

    public ErrorPage(string errorMessage = null)
    {
        InitializeComponent();
        _api = new ApiService(new PreferencesService());
        _cache = new LocalDatabaseService();

        if (!string.IsNullOrEmpty(errorMessage))
        {
            ErrorMessageLabel.Text = errorMessage;
        }
    }

    private async void OnRetryClicked(object sender, EventArgs e)
    {
        RetryBtn.IsEnabled = false;
        RetryBtn.Text = "Проверка...";

        try
        {
            var token = await _api.TestConnection();

            if (token)
            {
                // Проверяем, есть ли сохранённый токен
                var prefs = new PreferencesService();
                var savedToken = await prefs.GetToken();

                if (!string.IsNullOrEmpty(savedToken))
                {
                    // Пытаемся восстановить сессию
                    _api.SetAuthToken(savedToken);
                    await Navigation.PushAsync(new MenuPage());
                }
                else
                {
                    await Navigation.PopToRootAsync();
                }
            }
            else
            {
                await DisplayAlert("Ошибка", "Сервер всё ещё недоступен. Проверьте подключение.", "OK");
                RetryBtn.IsEnabled = true;
                RetryBtn.Text = "🔄 Попробовать снова";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось подключиться: {ex.Message}", "OK");
            RetryBtn.IsEnabled = true;
            RetryBtn.Text = "🔄 Попробовать снова";
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnOfflineClicked(object sender, EventArgs e)
    {
        var cachedThemes = _cache.GetCachedThemes();

        if (cachedThemes.Count > 0)
        {
            var result = await DisplayAlert("Офлайн режим",
                $"Найдено {cachedThemes.Count} викторин в кэше. Перейти в офлайн режим?",
                "Да", "Отмена");

            if (result)
            {
                await Navigation.PushAsync(new MenuPage(true));
            }
        }
        else
        {
            await DisplayAlert("Нет данных",
                "Нет сохранённых викторин. Подключитесь к интернету, чтобы загрузить викторины.",
                "OK");
        }
    }
}