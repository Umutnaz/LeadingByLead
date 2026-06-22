using Core;
using Frontend.Services.IService;
using System.Text.Json;

namespace Frontend.Services;

public class GameSessionService : IGameSessionService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GameSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameSession?> CreateSessionAsync(GameSession session)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(session, JsonOptions),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/gamesessions", content);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GameSession>(json, JsonOptions);
    }

    public async Task<GameSession?> GetSessionAsync(string id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/gamesessions/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GameSession>(json, JsonOptions);
    }

    public async Task<List<GameSession>> GetAllSessionsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/gamesessions");
        if (!response.IsSuccessStatusCode) return new List<GameSession>();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GameSession>>(json, JsonOptions) ?? new List<GameSession>();
    }

    public async Task<Player?> JoinSessionAsync(string sessionId, Player player)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(player, JsonOptions),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/gamesessions/{sessionId}/join", content);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Player>(json, JsonOptions);
    }

    public async Task StartSessionAsync(string sessionId)
    {
        await _httpClient.PostAsync($"{BaseUrl}/gamesessions/{sessionId}/start", null);
    }

    public async Task FinishSessionAsync(string sessionId)
    {
        await _httpClient.PostAsync($"{BaseUrl}/gamesessions/{sessionId}/finish", null);
    }

    public async Task AdvanceQuestionAsync(string sessionId)
    {
        await _httpClient.PostAsync($"{BaseUrl}/gamesessions/{sessionId}/next", null);
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        await _httpClient.DeleteAsync($"{BaseUrl}/gamesessions/{sessionId}");
    }

    public async Task PostPlayerStateAsync(string sessionId, PlayerState state)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(state, JsonOptions),
            System.Text.Encoding.UTF8,
            "application/json");

        await _httpClient.PostAsync($"{BaseUrl}/gamesessions/{sessionId}/playerstate", content);
    }

    public async Task<List<PlayerState>> GetPlayerStatesAsync(string sessionId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/gamesessions/{sessionId}/playerstates");
        if (!response.IsSuccessStatusCode) return new List<PlayerState>();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<PlayerState>>(json, JsonOptions) ?? new List<PlayerState>();
    }
}