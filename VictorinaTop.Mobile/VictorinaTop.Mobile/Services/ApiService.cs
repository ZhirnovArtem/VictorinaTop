using System.Net.Http.Json;
using VictorinaTop.Mobile.Models;

namespace VictorinaTop.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly PreferencesService _prefs;
    private bool _tokenLoaded = false;

#if DEBUG
#if ANDROID
    private const string BaseUrl = "http://10.0.2.2:5000/api/";
#else
    private const string BaseUrl = "http://localhost:5000/api/";
#endif
#else
    private const string BaseUrl = "https://your-server.com/api/";
#endif

    public ApiService(PreferencesService prefs)
    {
        _prefs = prefs;

        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

        _http = new HttpClient(handler);
        _http.BaseAddress = new Uri(BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(30);
        _http.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task EnsureTokenLoaded()
    {
        if (_tokenLoaded) return;
        var token = await _prefs.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _tokenLoaded = true; 
        }
    }

    public async Task<(bool success, string error, bool requiresVerification)>
        Register(string login, string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("auth/register", new { login, email, password });
            var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            return (result?.success == true, result?.error ?? "", result?.requiresVerification == true);
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.Register] {ex.Message}");
#endif
            return (false, "Ошибка соединения", false);
        }
    }

    public async Task<(bool success, string token, string login, int maxScore)>
        Verify(string email, string code, string login, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("auth/verify", new { email, code, type = "register", login, password });
            var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();

            if (result?.success == true && result.token != null)
            {
                await _prefs.SaveToken(result.token);
                await _prefs.SetUserLogin(result.login);

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.token);
                _tokenLoaded = true;

                return (true, result.token, result.login ?? "", result.maxScore);
            }
            return (false, "", "", 0);
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.Verify] {ex.Message}");
#endif
            return (false, "", "", 0);
        }
    }

    public async Task<(bool success, string token, string login, int maxScore)>
        Login(string loginOrEmail, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("auth/login", new { loginOrEmail, password });
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result?.success == true && result.token != null)
            {
                await _prefs.SaveToken(result.token);
                await _prefs.SetUserLogin(result.login);
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.token);
                _tokenLoaded = true;

                return (true, result.token, result.login ?? "", result.maxScore);
            }
            return (false, "", "", 0);
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.Login] {ex.Message}");
#endif
            return (false, "", "", 0);
        }
    }

    public async Task Logout()
    {
        await _prefs.ClearAll();
        _http.DefaultRequestHeaders.Authorization = null;
        _tokenLoaded = false;
    }

    public async Task<List<Theme>> GetThemes()
    {
        try
        {
            await EnsureTokenLoaded();
            return await _http.GetFromJsonAsync<List<Theme>>("themes") ?? new List<Theme>();
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.GetThemes] {ex.Message}");
#endif
            return new List<Theme>();
        }
    }

    public async Task<List<Question>> GetQuestions(int themeId)
    {
        try
        {
            await EnsureTokenLoaded();
            return await _http.GetFromJsonAsync<List<Question>>($"themes/{themeId}/questions") ?? new List<Question>();
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.GetQuestions] {ex.Message}");
#endif
            return new List<Question>();
        }
    }

    public async Task<int> CreateTheme(string name)
    {
        try
        {
            await EnsureTokenLoaded(); 
            var response = await _http.PostAsJsonAsync("themes", new { name });
#if DEBUG
            Console.WriteLine($"[CreateTheme] Status: {response.StatusCode}");
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[CreateTheme] Body: {body}");
#endif
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateThemeResponse>();
                return result?.id ?? 0;
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> AddQuestion(int themeId, Question question)
    {
        try
        {
            await EnsureTokenLoaded();
            var request = new
            {
                themeId,
                question.Text,
                question.Status,
                question.CorrectAnswer,
                question.OptionA,
                question.OptionB,
                question.OptionC,
                question.OptionD
            };
            var response = await _http.PostAsJsonAsync("questions", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private class CreateThemeResponse
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }
    public async Task<bool> SubmitScore(int themeId, int points)
    {
        try
        {
            await EnsureTokenLoaded();
            var response = await _http.PostAsJsonAsync("scores", new { themeId, points });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.SubmitScore] {ex.Message}");
#endif
            return false;
        }
    }

    public async Task<List<User>> GetLeaderboard(int limit = 10)
    {
        try
        {
            Console.WriteLine($"[GetLeaderboard] start, tokenLoaded={_tokenLoaded}");
            await EnsureTokenLoaded();
            Console.WriteLine($"[GetLeaderboard] token loaded, requesting...");
            var raw = await _http.GetStringAsync($"scores/leaderboard?limit={limit}");
            Console.WriteLine($"[GetLeaderboard] raw: {raw}");
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<List<User>>(raw, options) ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService.GetLeaderboard] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            return new List<User>();
        }
    }
    public async Task<bool> TestConnection()
    {
        try
        {
            var response = await _http.GetAsync("health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"[ApiService.TestConnection] {ex.Message}");
#endif
            return false;
        }
    }

    private class RegisterResponse { public bool success { get; set; } public string? error { get; set; } public bool requiresVerification { get; set; } }
    private class VerifyResponse { public bool success { get; set; } public string? token { get; set; } public string? login { get; set; } public int maxScore { get; set; } }
    private class LoginResponse { public bool success { get; set; } public string? token { get; set; } public string? login { get; set; } public int maxScore { get; set; } public string? error { get; set; } }
}