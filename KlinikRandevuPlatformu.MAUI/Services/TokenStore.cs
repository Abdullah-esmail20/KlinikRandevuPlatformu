using Microsoft.Maui.Storage;

namespace KlinikRandevuPlatformu.MAUI.Services;

public class TokenStore
{
    private const string TokenKey = "auth_token";
    private const string RoleKey = "auth_role";

    public void Save(string token, string role)
    {
        Preferences.Set(TokenKey, token);
        Preferences.Set(RoleKey, role);
    }

    public string? GetToken() => Preferences.Get(TokenKey, null);
    public string? GetRole() => Preferences.Get(RoleKey, null);

    public void Clear()
    {
        Preferences.Remove(TokenKey);
        Preferences.Remove(RoleKey);
    }
}