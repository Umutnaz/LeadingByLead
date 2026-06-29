using Frontend;
using Frontend.Services;
using Frontend.Services.IService;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder =
    WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Lokalt læses URL'en fra appsettings.Development.json.
// På Render bruges samme domæne som frontenden.
var configuredApiBaseUrl =
    builder.Configuration["ApiBaseUrl"];

var apiBaseUrl =
    string.IsNullOrWhiteSpace(configuredApiBaseUrl)
        ? builder.HostEnvironment.BaseAddress
        : configuredApiBaseUrl;

builder.Services.AddScoped(_ =>
{
    return new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddScoped<
    ICharacterService,
    CharacterService>();

builder.Services.AddScoped<
    IQuestionsService,
    QuestionService>();

builder.Services.AddScoped<
    IGameSessionService,
    GameSessionService>();

builder.Services.AddScoped<
    LocalStorageService>();

await builder.Build().RunAsync();