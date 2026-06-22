using Core;
using Frontend.Services.IService;
using System.Text.Json;

namespace Frontend.Services;

public class CharacterService : ICharacterService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CharacterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Character>> GetAllCharactersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/characters");
            if (!response.IsSuccessStatusCode) return new List<Character>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Character>>(json, JsonOptions) ?? new List<Character>();
        }
        catch
        {
            return new List<Character>();
        }
    }

    public async Task<Character?> GetCharacterAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/characters/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Character>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Character?> CreateCharacterAsync(Character character)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(character, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{BaseUrl}/characters", content);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Character>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateCharacterAsync(string id, Character character)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(character, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            await _httpClient.PutAsync($"{BaseUrl}/characters/{id}", content);
        }
        catch
        {
        }
    }

    public async Task DeleteCharacterAsync(string id)
    {
        try
        {
            await _httpClient.DeleteAsync($"{BaseUrl}/characters/{id}");
        }
        catch
        {
        }
    }
}