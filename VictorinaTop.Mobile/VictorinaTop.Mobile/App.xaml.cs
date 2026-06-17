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
        var token = _prefs.GetToken().GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(token))
        {
            return new Window(new NavigationPage(new MenuPage(_api, _prefs)));
        }

        return new Window(new NavigationPage(new MainPage(_api, _prefs)));
    }
}