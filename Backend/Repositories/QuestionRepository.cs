using Core;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly IMongoCollection<Question> _collection;

    public QuestionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Question>("Questions");
    }

    public async Task<List<Question>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<Question?> GetAsync(string id) =>
        await _collection.Find(q => q.Id == id).FirstOrDefaultAsync();

    public async Task<Question> CreateAsync(Question question)
    {
        await _collection.InsertOneAsync(question);
        return question;
    }

    public async Task UpdateAsync(string id, Question question) =>
        await _collection.ReplaceOneAsync(q => q.Id == id, question);

    public async Task DeleteAsync(string id) =>
        await _collection.DeleteOneAsync(q => q.Id == id);
}

