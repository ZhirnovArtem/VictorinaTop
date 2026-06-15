using VictorinaTop.Mobile.Services;
using VictorinaTop.Mobile.Views;

namespace VictorinaTop.Mobile;

public partial class App : Application
{
    private readonly PreferencesService _prefs;

    public App()
    {
        _prefs = new PreferencesService();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var token = _prefs.GetToken().GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(token))
        {
            return new Window(new NavigationPage(new MenuPage()));
        }

        return new Window(new NavigationPage(new MainPage()));
    }
}