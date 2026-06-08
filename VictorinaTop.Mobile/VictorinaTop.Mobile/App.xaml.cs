using VictorinaTop.Mobile.Services;
using VictorinaTop.Mobile.Views;

namespace VictorinaTop.Mobile;

public partial class App : Application
{
    private readonly PreferencesService _prefs;

    public App(PreferencesService prefs)
    {
        InitializeComponent();
        _prefs = prefs;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        var token = await _prefs.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            MainPage = new NavigationPage(new MenuPage());
        }
        else
        {
            MainPage = new NavigationPage(new MainPage());
        }
    }
}