using Core;

namespace Backend.Repositories;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllAsync();

    Task<Question?> GetAsync(string id);

    Task<Question> CreateAsync(Question question);

    Task UpdateAsync(string id, Question question);

    Task DeleteAsync(string id);

    Task RemoveCharacterEffectsAsync(string characterId);
}