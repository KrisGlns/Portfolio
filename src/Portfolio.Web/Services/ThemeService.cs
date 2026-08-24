using Microsoft.JSInterop;

namespace Portfolio.Web.Services;

/// <summary>
/// Thin wrapper over the theme helpers in <c>wwwroot/js/site.js</c>. The initial theme is resolved
/// by an inline script in index.html before first paint; this only handles switching afterwards.
/// </summary>
public sealed class ThemeService(IJSRuntime js)
{
    public string Current { get; private set; } = "dark";

    public bool IsDark => Current == "dark";

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        Current = await js.InvokeAsync<string>("portfolio.getTheme");
        Changed?.Invoke();
    }

    public async Task ToggleAsync()
    {
        Current = await js.InvokeAsync<string>("portfolio.setTheme", IsDark ? "light" : "dark");
        Changed?.Invoke();
    }
}
