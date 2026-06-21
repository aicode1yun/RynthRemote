using Microsoft.Maui.Storage;

namespace RynthRemote.Services;

/// <summary>
/// Tiny settings store backed by MAUI Preferences — holds the StatusAgent URL + token.
/// No database; mirrors the 3-method shape the AC Clients page expects.
/// </summary>
public sealed class SettingsStore
{
    public Task<string?> GetAsync(string key)
    {
        string v = Preferences.Default.Get(key, string.Empty);
        return Task.FromResult(string.IsNullOrEmpty(v) ? null : v);
    }

    public Task<string> GetAsync(string key, string fallback)
        => Task.FromResult(Preferences.Default.Get(key, fallback));

    public Task SetAsync(string key, string value)
    {
        Preferences.Default.Set(key, value ?? string.Empty);
        return Task.CompletedTask;
    }
}
