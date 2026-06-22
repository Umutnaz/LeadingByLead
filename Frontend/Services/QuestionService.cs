using Core;
using Frontend.Services.IService;
using System.Text.Json;

namespace Frontend.Services;

public class QuestionService : IQuestionsService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuestionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/questions");
            if (!response.IsSuccessStatusCode) return new List<Question>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Question>>(json, JsonOptions) ?? new List<Question>();
        }
        catch
        {
            return new List<Question>();
        }
    }

    public async Task<Question?> GetQuestionAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/questions/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Question>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Question?> CreateQuestionAsync(Question question)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(question, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{BaseUrl}/questions", content);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Question>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateQuestionAsync(string id, Question question)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(question, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            await _httpClient.PutAsync($"{BaseUrl}/questions/{id}", content);
        }
        catch
        {
        }
    }

    public async Task DeleteQuestionAsync(string id)
    {
        try
        {
            await _httpClient.DeleteAsync($"{BaseUrl}/questions/{id}");
        }
        catch
        {
        }
    }
}