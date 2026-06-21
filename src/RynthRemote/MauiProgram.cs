using System.Net;
using Microsoft.Extensions.Logging;
using RynthRemote.AcStatus;
using RynthRemote.Services;

namespace RynthRemote;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Settings persisted via MAUI Preferences (the status URL + token). No database.
        builder.Services.AddSingleton<SettingsStore>();

        // The one HTTP client: talks to the user's RynthCore StatusAgent (GET /status, POST /command).
        builder.Services.AddHttpClient<IAcStatusClient, AcStatusClient>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
