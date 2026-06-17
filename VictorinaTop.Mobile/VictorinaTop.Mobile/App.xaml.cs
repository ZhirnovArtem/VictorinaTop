using VictorinaTop.Mobile.Services;
using VictorinaTop.Mobile.Views;

namespace VictorinaTop.Mobile;

public partial class App : Application
{
    private readonly PreferencesService _prefs;
    private readonly ApiService _api;

    public App(PreferencesService prefs, ApiService api)
    {
        _prefs = prefs;
        _api = api;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Сначала показываем MainPage, потом проверяем токен
        var navPage = new NavigationPage(new MainPage(_api, _prefs));
        var window = new Window(navPage);

        // Асинхронно проверяем токен и переходим в меню если есть
        Task.Run(async () =>
        {
            var token = await _prefs.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    window.Page = new NavigationPage(new MenuPage(_api, _prefs));
                });
            }
        });

        return window;
    }
}