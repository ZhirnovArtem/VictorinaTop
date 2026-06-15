namespace VictorinaTop.Mobile.Services;

public class PreferencesService
{
    private const string TokenKey = "auth_token";
    private const string UserLoginKey = "user_login";

    public async Task SaveToken(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);
    }

    public async Task<string?> GetToken()
    {
        return await SecureStorage.GetAsync(TokenKey);
    }

    public async Task SetUserLogin(string login)
    {
        await SecureStorage.SetAsync(UserLoginKey, login);
    }

    public async Task<string?> GetUserLogin()
    {
        return await SecureStorage.GetAsync(UserLoginKey);
    }

    public async Task ClearAll()
    {
        SecureStorage.RemoveAll();
        Preferences.Clear();
    }
}