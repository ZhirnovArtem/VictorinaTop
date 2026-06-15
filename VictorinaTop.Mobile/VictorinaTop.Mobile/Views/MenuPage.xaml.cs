using VictorinaTop.Mobile.Models;
using VictorinaTop.Mobile.Services;

namespace VictorinaTop.Mobile.Views;

public class MenuPage : ContentPage
{
    private readonly ApiService _api;
    private readonly LocalDatabaseService _cache;
    private readonly PreferencesService _prefs;
    private readonly bool _isOfflineMode;
    private CollectionView _themesList;
    private CollectionView _topPlayersList;
    private Label _emptyLabel;

    public MenuPage() : this(false) { }

    public MenuPage(bool isOfflineMode)
    {
        _prefs = new PreferencesService();
        _api = new ApiService(_prefs);
        _cache = new LocalDatabaseService();
        _isOfflineMode = isOfflineMode;

        Title = "Главное меню";
        BackgroundColor = Color.FromArgb("#1A1A2E");

        BuildUI();
        _ = LoadDataAsync(); 
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private void BuildUI()
    {
        // ТОП-3 блок
        var topPlayersTitle = new Label
        {
            Text = "🏆 ТОП-3 ИГРОКОВ 🏆",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center
        };

        _topPlayersList = new CollectionView
        {
            HeightRequest = 150,
            SelectionMode = SelectionMode.None
        };
        _topPlayersList.ItemTemplate = new DataTemplate(() =>
        {
            var idLabel = new Label();
            idLabel.SetBinding(Label.TextProperty, "Id");
            idLabel.TextColor = Color.FromArgb("#FFD700");

            var loginLabel = new Label();
            loginLabel.SetBinding(Label.TextProperty, "Login");
            loginLabel.TextColor = Colors.White;
            loginLabel.Margin = new Thickness(10, 0);

            var scoreLabel = new Label();
            scoreLabel.SetBinding(Label.TextProperty, "MaxScore");
            scoreLabel.TextColor = Color.FromArgb("#50C878");

            var grid = new Grid
            {
                Padding = new Thickness(10, 5),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            grid.Add(idLabel, 0, 0);
            grid.Add(loginLabel, 1, 0);
            grid.Add(scoreLabel, 2, 0);

            return new Frame { Content = grid, HasShadow = false };
        });

        var topPlayersFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#16213E"),
            Margin = 15,
            Padding = 15,
            CornerRadius = 15,
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Children = { topPlayersTitle, new BoxView { HeightRequest = 2, BackgroundColor = Color.FromArgb("#FFD700"), Margin = new Thickness(0, 10) }, _topPlayersList }
            }
        };

        var themesTitle = new Label
        {
            Text = "🎮 ВИКТОРИНЫ 🎮",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalOptions = LayoutOptions.Center
        };

        _themesList = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            HeightRequest = 200
        };
        _themesList.SelectionChanged += OnThemeSelected;
        _themesList.ItemTemplate = new DataTemplate(() =>
        {
            var iconLabel = new Label
            {
                Text = "📚",
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center
            };

            var nameLabel = new Label();
            nameLabel.SetBinding(Label.TextProperty, "Name");
            nameLabel.TextColor = Colors.White;
            nameLabel.FontSize = 16;
            nameLabel.FontAttributes = FontAttributes.Bold;

            var authorLabel = new Label();
            authorLabel.SetBinding(Label.TextProperty, "AuthorDisplay");
            authorLabel.TextColor = Color.FromArgb("#888888");
            authorLabel.FontSize = 11;

            var playButton = new Button
            {
                Text = "Играть",
                CornerRadius = 10,
                WidthRequest = 60,
                HeightRequest = 35,
                BackgroundColor = Color.FromArgb("#4A90E2")
            };

            var infoLayout = new VerticalStackLayout
            {
                Margin = new Thickness(10, 0),
                Children = { nameLabel, authorLabel }
            };

            var grid = new Grid
            {
                ColumnSpacing = 10,
                Padding = new Thickness(10, 5),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            grid.Add(iconLabel, 0, 0);
            grid.Add(infoLayout, 1, 0);
            grid.Add(playButton, 2, 0);

            return new Frame
            {
                BackgroundColor = Color.FromArgb("#2D2D44"),
                CornerRadius = 10,
                HasShadow = false,
                Margin = new Thickness(0, 5),
                Content = grid
            };
        });

        _emptyLabel = new Label
        {
            Text = "Нет доступных викторин",
            TextColor = Color.FromArgb("#888888"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20),
            IsVisible = false
        };

        var themesLayout = new VerticalStackLayout
        {
            Children = { themesTitle, new BoxView { HeightRequest = 2, BackgroundColor = Color.FromArgb("#FFD700"), Margin = new Thickness(0, 10) }, _themesList, _emptyLabel }
        };

        var themesFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#16213E"),
            Margin = new Thickness(15, 0, 15, 15),
            Padding = 15,
            CornerRadius = 15,
            HasShadow = false,
            Content = themesLayout
        };

        var createButton = new Button
        {
            Text = "✏️ Создать викторину",
            BackgroundColor = Color.FromArgb("#9B59B6"),
            TextColor = Colors.White,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 15,
            HeightRequest = 50
        };
        createButton.Clicked += OnCreateClicked;

        var logoutButton = new Button
        {
            Text = "🚪 Выйти",
            BackgroundColor = Color.FromArgb("#E74C3C"),
            TextColor = Colors.White,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 15,
            HeightRequest = 50
        };
        logoutButton.Clicked += OnLogoutClicked;

        var buttonsLayout = new VerticalStackLayout
        {
            Spacing = 15,
            Padding = new Thickness(30),
            Children = { createButton, logoutButton }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        grid.Add(topPlayersFrame, 0, 0);
        grid.Add(themesFrame, 0, 1);
        grid.Add(buttonsLayout, 0, 2);

        Content = grid;
    }

    private async Task LoadDataAsync()
    {
        await LoadTopPlayers();
        await LoadThemes();
    }

    private void LoadData()
    {
        _ = LoadDataAsync();  
    }

    private async Task LoadTopPlayers()
    {
        var topPlayers = await _api.GetLeaderboard(3);
        _topPlayersList.ItemsSource = topPlayers;
    }

    private async Task LoadThemes()
    {
        var themes = await _api.GetThemes();

        if (themes.Count == 0)
        {
            themes = _cache.GetCachedThemes();
            if (themes.Count == 0)
            {
                _emptyLabel.IsVisible = true;
                _themesList.IsVisible = false;
                return;
            }
        }
        else
        {
            _cache.CacheThemes(themes);
        }

        _themesList.ItemsSource = themes;
        _emptyLabel.IsVisible = false;
        _themesList.IsVisible = true;
    }

    private async void OnThemeSelected(object sender, SelectionChangedEventArgs e)
    {
        var theme = e.CurrentSelection.FirstOrDefault() as Theme;
        if (theme == null) return;

        _themesList.SelectedItem = null;

        var questions = await _api.GetQuestions(theme.Id);

        if (questions.Count == 0)
        {
            questions = _cache.GetCachedQuestions(theme.Id);
        }
        else
        {
            _cache.CacheQuestions(theme.Id, questions);
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