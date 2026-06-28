using System.Net;
using System.Net.Http.Json;
using Core;
using Frontend.Services.IService;

namespace Frontend.Services;

public class GameSessionService : IGameSessionService
{
    private const string BaseUrl = "api/v1/GameSessions";

    private readonly HttpClient _httpClient;

    public string LastError { get; private set; } = "";

    public GameSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameSession?> CreateSessionAsync(
        GameSession session)
    {
        ClearError();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BaseUrl,
                session);

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Spillet kunne ikke oprettes.");

                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<GameSession>();
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return null;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en ukendt fejl ved oprettelse af spillet.";

            return null;
        }
    }

    public async Task<GameSession?> GetSessionAsync(string id)
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(id))
        {
            LastError = "Session-ID mangler.";
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(id)}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                LastError = "Sessionen findes ikke.";
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Sessionen kunne ikke hentes.");

                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<GameSession>();
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return null;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl, mens sessionen blev hentet.";

            return null;
        }
    }

    public async Task<List<GameSession>> GetAllSessionsAsync()
    {
        ClearError();

        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Sessionerne kunne ikke hentes.");

                return new List<GameSession>();
            }

            return await response.Content
                       .ReadFromJsonAsync<List<GameSession>>() ??
                   new List<GameSession>();
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return new List<GameSession>();
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl, mens sessionerne blev hentet.";

            return new List<GameSession>();
        }
    }

    public async Task<Player?> JoinSessionAsync(
        string sessionId,
        Player player)
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            LastError = "Session-ID mangler.";
            return null;
        }

        if (string.IsNullOrWhiteSpace(player.Name))
        {
            LastError = "Du skal indtaste dit navn.";
            return null;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}/join",
                player);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                LastError = "Sessionen findes ikke.";
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Du kunne ikke deltage i spillet.");

                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<Player>();
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return null;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl, mens du forsøgte at deltage.";

            return null;
        }
    }

    public async Task<bool> RemovePlayerAsync(
        string sessionId,
        string playerId)
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(sessionId) ||
            string.IsNullOrWhiteSpace(playerId))
        {
            LastError = "Session eller spiller mangler.";
            return false;
        }

        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}" +
                $"/players/{Uri.EscapeDataString(playerId)}");

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Spilleren kunne ikke fjernes.");

                return false;
            }

            return true;
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return false;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl, mens spilleren blev fjernet.";

            return false;
        }
    }

    public Task<bool> StartSessionAsync(string sessionId)
    {
        return PostWithoutBodyAsync(
            $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}/start",
            "Spillet kunne ikke startes.");
    }

    public Task<bool> FinishSessionAsync(string sessionId)
    {
        return PostWithoutBodyAsync(
            $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}/finish",
            "Spillet kunne ikke afsluttes.");
    }

    public Task<bool> AdvanceQuestionAsync(string sessionId)
    {
        return PostWithoutBodyAsync(
            $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}/next",
            "Spillet kunne ikke gå videre.");
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            LastError = "Session-ID mangler.";
            return false;
        }

        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}");

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Sessionen kunne ikke slettes.");

                return false;
            }

            return true;
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return false;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl, mens sessionen blev slettet.";

            return false;
        }
    }

    public async Task<bool> PostPlayerStateAsync(
        string sessionId,
        PlayerState state)
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            LastError = "Session-ID mangler.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(state.PlayerId))
        {
            LastError = "Spiller-ID mangler.";
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}/playerstate",
                state);

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Dit svar kunne ikke gemmes.");

                return false;
            }

            return true;
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return false;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl, mens dit svar blev gemt.";

            return false;
        }
    }

    public async Task<List<PlayerState>> GetPlayerStatesAsync(
        string sessionId)
    {
        return await GetPlayerStateListAsync(
            $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}/playerstates");
    }

    public async Task<List<PlayerState>> GetCurrentPlayerStatesAsync(
        string sessionId)
    {
        return await GetPlayerStateListAsync(
            $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}" +
            "/playerstates/current");
    }

    public async Task<bool> DeletePlayerStateAsync(
        string sessionId,
        string playerId)
    {
        ClearError();

        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(sessionId)}" +
                $"/playerstate/{Uri.EscapeDataString(playerId)}");

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Spillerens resultater kunne ikke slettes.");

                return false;
            }

            return true;
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return false;
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl ved sletning af resultaterne.";

            return false;
        }
    }

    private async Task<List<PlayerState>> GetPlayerStateListAsync(
        string url)
    {
        ClearError();

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                LastError = "Sessionen findes ikke.";
                return new List<PlayerState>();
            }

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    "Spillerresultaterne kunne ikke hentes.");

                return new List<PlayerState>();
            }

            return await response.Content
                       .ReadFromJsonAsync<List<PlayerState>>() ??
                   new List<PlayerState>();
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return new List<PlayerState>();
        }
        catch (Exception)
        {
            LastError =
                "Der opstod en fejl ved hentning af resultaterne.";

            return new List<PlayerState>();
        }
    }

    private async Task<bool> PostWithoutBodyAsync(
        string url,
        string fallbackError)
    {
        ClearError();

        try
        {
            var response = await _httpClient.PostAsync(
                url,
                null);

            if (!response.IsSuccessStatusCode)
            {
                await SetErrorFromResponse(
                    response,
                    fallbackError);

                return false;
            }

            return true;
        }
        catch (HttpRequestException)
        {
            LastError =
                "Der kunne ikke oprettes forbindelse til serveren.";

            return false;
        }
        catch (Exception)
        {
            LastError = fallbackError;
            return false;
        }
    }

    private async Task SetErrorFromResponse(
        HttpResponseMessage response,
        string fallbackError)
    {
        try
        {
            var responseText =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseText))
            {
                LastError = fallbackError;
                return;
            }

            LastError = responseText
                .Trim()
                .Trim('"');
        }
        catch
        {
            LastError = fallbackError;
        }
    }

    private void ClearError()
    {
        LastError = "";
    }
}