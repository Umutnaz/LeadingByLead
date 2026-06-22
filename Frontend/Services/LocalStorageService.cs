using Core;
using System.Text.Json;

namespace Frontend.Services;

public class LocalStorageService
{
    private const string PlayerPrefix = "lbl_player";
    private const string SessionPrefix = "lbl_session";
    private const string CharactersPrefix = "lbl_characters";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, string> Storage = new();

    public void SetPlayerRole(string role)
    {
        SetItem($"{PlayerPrefix}_role", role);
    }

    public string GetPlayerRole()
    {
        return GetItem($"{PlayerPrefix}_role") ?? "";
    }

    public void SetPlayerName(string name)
    {
        SetItem($"{PlayerPrefix}_name", name);
    }

    public string GetPlayerName()
    {
        return GetItem($"{PlayerPrefix}_name") ?? "";
    }

    public void SetPlayerId(string id)
    {
        SetItem($"{PlayerPrefix}_id", id);
    }

    public string GetPlayerId()
    {
        return GetItem($"{PlayerPrefix}_id") ?? "";
    }

    public void SetCurrentSessionId(string sessionId)
    {
        SetItem($"{SessionPrefix}_current", sessionId);
    }

    public string GetCurrentSessionId()
    {
        return GetItem($"{SessionPrefix}_current") ?? "";
    }

    public void SavePlayerCharacters(List<Character> characters)
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();
        var key = $"{CharactersPrefix}_{sessionId}_{playerId}";

        SetItem(key, JsonSerializer.Serialize(characters, JsonOptions));
    }

    public List<Character> GetPlayerCharacters()
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();
        var key = $"{CharactersPrefix}_{sessionId}_{playerId}";
        var json = GetItem(key);

        if (string.IsNullOrEmpty(json)) return new List<Character>();

        try
        {
            return JsonSerializer.Deserialize<List<Character>>(json, JsonOptions) ?? new List<Character>();
        }
        catch
        {
            return new List<Character>();
        }
    }

    public void AddAnsweredQuestion(string questionId, string answerId)
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();
        var key = $"{SessionPrefix}_{sessionId}_{playerId}_answers";

        var answers = GetAnsweredQuestions();
        answers[questionId] = answerId;

        SetItem(key, JsonSerializer.Serialize(answers, JsonOptions));
    }

    public Dictionary<string, string> GetAnsweredQuestions()
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();
        var key = $"{SessionPrefix}_{sessionId}_{playerId}_answers";
        var json = GetItem(key);

        if (string.IsNullOrEmpty(json)) return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public void SetCurrentQuestionIndex(int index)
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();
        var key = $"{SessionPrefix}_{sessionId}_{playerId}_qindex";

        SetItem(key, index.ToString());
    }

    public int GetCurrentQuestionIndex()
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();
        var key = $"{SessionPrefix}_{sessionId}_{playerId}_qindex";
        var value = GetItem(key);

        if (int.TryParse(value, out var index)) return index;

        return 0;
    }

    public void ClearSessionData()
    {
        var sessionId = GetCurrentSessionId();
        var playerId = GetPlayerId();

        RemoveItem($"{SessionPrefix}_current");
        RemoveItem($"{CharactersPrefix}_{sessionId}_{playerId}");
        RemoveItem($"{SessionPrefix}_{sessionId}_{playerId}_answers");
        RemoveItem($"{SessionPrefix}_{sessionId}_{playerId}_qindex");
    }

    public void ClearAll()
    {
        Storage.Clear();
    }

    private static void SetItem(string key, string value)
    {
        Storage[key] = value;
    }

    private static string? GetItem(string key)
    {
        if (Storage.ContainsKey(key)) return Storage[key];

        return null;
    }

    private static void RemoveItem(string key)
    {
        Storage.Remove(key);
    }
}