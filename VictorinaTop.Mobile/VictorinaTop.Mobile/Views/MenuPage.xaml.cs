using VictorinaTop.Mobile.Models;
using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public partial class MenuPage : ContentPage
{
    private readonly ApiService _api;
    private readonly LocalDatabaseService _cache;
    private readonly PreferencesService _prefs;
    private List<Theme> _themes;
    private readonly bool _isOfflineMode;
    public MenuPage() : this(false)
    {

    }
    public MenuPage(bool isOfflineMode)
    {
        InitializeComponent();
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
        _cache = new LocalDatabaseService();
        _isOfflineMode = isOfflineMode;

        if (_isOfflineMode)
        {
            Title = "Офлайн режим";
        }

        LoadData();
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();  
    }

    private async Task LoadDataAsync()
    {
        await LoadTopPlayers();
        await LoadThemes();
    }

    private void LoadData() 
    {
        Task.Run(async () => await LoadDataAsync());
    }

    private async Task LoadTopPlayers()
    {
        var topPlayers = await _api.GetLeaderboard(3);
        TopPlayersList.ItemsSource = topPlayers;
    }

    private async Task LoadThemes()
    {
        var themes = await _api.GetThemes();

        if (themes.Count == 0)
        {
            themes = _cache.GetCachedThemes();
            if (themes.Count == 0)
            {
                EmptyLabel.IsVisible = true;
                ThemesList.IsVisible = false;
                return;
            }
        }
        else
        {
            _cache.CacheThemes(themes);
        }

        _themes = themes;
        ThemesList.ItemsSource = _themes;
        EmptyLabel.IsVisible = false;
        ThemesList.IsVisible = true;
    }

    private async void OnThemeSelected(object sender, SelectionChangedEventArgs e)
    {
        var theme = e.CurrentSelection.FirstOrDefault() as Theme;
        if (theme == null) return;

        ThemesList.SelectedItem = null;

        var questions = await _api.GetQuestions(theme.Id);

        if (questions.Count == 0)
        {
            questions = _cache.GetCachedQuestions(theme.Id);
        }
        else
        {
            _cache.CacheQuestions(theme.Id, questions);
        }

        if (questions.Count == 0)
        {
            await DisplayAlert("Внимание", "В этой викторине пока нет вопросов", "OK");
            return;
        }

        await Navigation.PushAsync(new GamePage(theme.Name, questions, theme.Id));
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateGamePage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Выход", "Вы уверены, что хотите выйти?", "Да", "Нет");
        if (confirm)
        {
            await _api.Logout();
            await Navigation.PopToRootAsync();
        }
    }
}