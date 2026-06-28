using System.Text.Json;
using Core;
using Microsoft.JSInterop;

namespace Frontend.Services;

public class LocalStorageService
{
    private const string PlayerRoleKey = "lbl_player_role";
    private const string PlayerNameKey = "lbl_player_name";
    private const string PlayerIdKey = "lbl_player_id";
    private const string CurrentSessionKey = "lbl_session_current";
    private const string NavigationMessageKey = "lbl_navigation_message";

    private readonly IJSRuntime _jsRuntime;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task SetPlayerRoleAsync(string role)
    {
        return SetItemAsync(PlayerRoleKey, role);
    }

    public async Task<string> GetPlayerRoleAsync()
    {
        return await GetItemAsync(PlayerRoleKey) ?? "";
    }

    public Task SetPlayerNameAsync(string name)
    {
        return SetItemAsync(PlayerNameKey, name);
    }

    public async Task<string> GetPlayerNameAsync()
    {
        return await GetItemAsync(PlayerNameKey) ?? "";
    }

    public Task SetPlayerIdAsync(string id)
    {
        return SetItemAsync(PlayerIdKey, id);
    }

    public async Task<string> GetPlayerIdAsync()
    {
        return await GetItemAsync(PlayerIdKey) ?? "";
    }

    public Task SetCurrentSessionIdAsync(string sessionId)
    {
        return SetItemAsync(CurrentSessionKey, sessionId);
    }

    public async Task<string> GetCurrentSessionIdAsync()
    {
        return await GetItemAsync(CurrentSessionKey) ?? "";
    }

    public Task SetNavigationMessageAsync(string message)
    {
        return SetItemAsync(NavigationMessageKey, message);
    }

    public async Task<string> TakeNavigationMessageAsync()
    {
        var message =
            await GetItemAsync(NavigationMessageKey) ?? "";

        await RemoveItemAsync(NavigationMessageKey);

        return message;
    }

    public async Task SavePlayerCharactersAsync(
        List<Character> characters)
    {
        var key = await GetCharacterStorageKeyAsync();

        var json = JsonSerializer.Serialize(
            characters,
            JsonOptions);

        await SetItemAsync(key, json);
    }

    public async Task<List<Character>> GetPlayerCharactersAsync()
    {
        var key = await GetCharacterStorageKeyAsync();
        var json = await GetItemAsync(key);

        if (string.IsNullOrWhiteSpace(json))
            return new List<Character>();

        try
        {
            return JsonSerializer.Deserialize<List<Character>>(
                       json,
                       JsonOptions) ??
                   new List<Character>();
        }
        catch
        {
            await RemoveItemAsync(key);
            return new List<Character>();
        }
    }

    public async Task AddAnsweredQuestionAsync(
        string questionId,
        IEnumerable<string> answerIds)
    {
        var key = await GetAnswerStorageKeyAsync();

        var answers = await GetAnsweredQuestionsAsync();

        answers[questionId] = answerIds.ToList();

        var json = JsonSerializer.Serialize(
            answers,
            JsonOptions);

        await SetItemAsync(key, json);
    }

    public async Task<Dictionary<string, List<string>>>
        GetAnsweredQuestionsAsync()
    {
        var key = await GetAnswerStorageKeyAsync();
        var json = await GetItemAsync(key);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, List<string>>();
        }

        try
        {
            return JsonSerializer.Deserialize<
                       Dictionary<string, List<string>>>(
                       json,
                       JsonOptions) ??
                   new Dictionary<string, List<string>>();
        }
        catch
        {
            await RemoveItemAsync(key);

            return new Dictionary<string, List<string>>();
        }
    }

    public async Task SetCurrentQuestionIndexAsync(int index)
    {
        var key = await GetQuestionIndexStorageKeyAsync();

        await SetItemAsync(
            key,
            index.ToString());
    }

    public async Task<int> GetCurrentQuestionIndexAsync()
    {
        var key = await GetQuestionIndexStorageKeyAsync();
        var value = await GetItemAsync(key);

        return int.TryParse(value, out var index)
            ? index
            : 0;
    }

    public async Task ClearPlayerSessionDataAsync()
    {
        var sessionId = await GetCurrentSessionIdAsync();
        var playerId = await GetPlayerIdAsync();

        if (!string.IsNullOrWhiteSpace(sessionId) &&
            !string.IsNullOrWhiteSpace(playerId))
        {
            await RemoveItemAsync(
                GetCharacterStorageKey(
                    sessionId,
                    playerId));

            await RemoveItemAsync(
                GetAnswerStorageKey(
                    sessionId,
                    playerId));

            await RemoveItemAsync(
                GetQuestionIndexStorageKey(
                    sessionId,
                    playerId));
        }

        await RemoveItemAsync(CurrentSessionKey);
        await RemoveItemAsync(PlayerIdKey);
        await RemoveItemAsync(PlayerNameKey);
    }

    public async Task ClearSessionDataAsync()
    {
        var sessionId = await GetCurrentSessionIdAsync();
        var playerId = await GetPlayerIdAsync();

        if (!string.IsNullOrWhiteSpace(sessionId) &&
            !string.IsNullOrWhiteSpace(playerId))
        {
            await RemoveItemAsync(
                GetCharacterStorageKey(
                    sessionId,
                    playerId));

            await RemoveItemAsync(
                GetAnswerStorageKey(
                    sessionId,
                    playerId));

            await RemoveItemAsync(
                GetQuestionIndexStorageKey(
                    sessionId,
                    playerId));
        }

        await RemoveItemAsync(CurrentSessionKey);
    }

    public async Task ClearAllAsync()
    {
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.clear");
    }

    public async Task<bool> HasPlayerIdentityAsync()
    {
        var playerId = await GetPlayerIdAsync();
        var playerName = await GetPlayerNameAsync();

        return !string.IsNullOrWhiteSpace(playerId) &&
               !string.IsNullOrWhiteSpace(playerName);
    }

    private async Task<string> GetCharacterStorageKeyAsync()
    {
        var sessionId = await GetCurrentSessionIdAsync();
        var playerId = await GetPlayerIdAsync();

        return GetCharacterStorageKey(
            sessionId,
            playerId);
    }

    private async Task<string> GetAnswerStorageKeyAsync()
    {
        var sessionId = await GetCurrentSessionIdAsync();
        var playerId = await GetPlayerIdAsync();

        return GetAnswerStorageKey(
            sessionId,
            playerId);
    }

    private async Task<string> GetQuestionIndexStorageKeyAsync()
    {
        var sessionId = await GetCurrentSessionIdAsync();
        var playerId = await GetPlayerIdAsync();

        return GetQuestionIndexStorageKey(
            sessionId,
            playerId);
    }

    private static string GetCharacterStorageKey(
        string sessionId,
        string playerId)
    {
        return $"lbl_characters_{sessionId}_{playerId}";
    }

    private static string GetAnswerStorageKey(
        string sessionId,
        string playerId)
    {
        return $"lbl_session_{sessionId}_{playerId}_answers";
    }

    private static string GetQuestionIndexStorageKey(
        string sessionId,
        string playerId)
    {
        return $"lbl_session_{sessionId}_{playerId}_qindex";
    }

    private async Task SetItemAsync(
        string key,
        string value)
    {
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            key,
            value);
    }

    private async Task<string?> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            key);
    }

    private async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            key);
    }
}