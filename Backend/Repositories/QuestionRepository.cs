using Core;
using MongoDB.Driver;

namespace Backend.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly IMongoCollection<Question> _collection;

    public QuestionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Question>("Questions");
    }

    public async Task<List<Question>> GetAllAsync()
    {
        return await _collection
            .Find(_ => true)
            .ToListAsync();
    }

    public async Task<Question?> GetAsync(string id)
    {
        return await _collection
            .Find(question => question.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Question> CreateAsync(Question question)
    {
        NormalizeQuestion(question);

        await _collection.InsertOneAsync(question);

        return question;
    }

    public async Task UpdateAsync(
        string id,
        Question question)
    {
        NormalizeQuestion(question);

        question.Id = id;

        await _collection.ReplaceOneAsync(
            existingQuestion => existingQuestion.Id == id,
            question);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(
            question => question.Id == id);
    }

    public async Task RemoveCharacterEffectsAsync(string characterId)
    {
        var questions = await GetAllAsync();

        foreach (var question in questions)
        {
            var changed = false;

            foreach (var answer in question.AnswerOptions)
            {
                var removed = answer.CharacterEffects.RemoveAll(
                    effect => effect.CharacterId == characterId);

                if (removed > 0)
                    changed = true;
            }

            if (changed)
                await UpdateAsync(question.Id, question);
        }
    }

    private static void NormalizeQuestion(Question question)
    {
        question.AnswerOptions ??= new();

        foreach (var answer in question.AnswerOptions)
        {
            answer.CharacterEffects ??= new();

            foreach (var effect in answer.CharacterEffects)
            {
                effect.Changes ??= new();

                foreach (var change in effect.Changes)
                {
                    change.Amount = Math.Clamp(
                        change.Amount,
                        -100,
                        100);
                }
            }
        }
    }
}