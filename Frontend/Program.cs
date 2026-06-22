using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Services;
using Frontend.Services.IService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for backend API calls
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient();
    client.BaseAddress = new("http://localhost:5291");
    client.Timeout = TimeSpan.FromSeconds(30);
    return client;
});

// Register services
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<IQuestionsService, QuestionService>();
builder.Services.AddScoped<LocalStorageService>();

await builder.Build().RunAsync();