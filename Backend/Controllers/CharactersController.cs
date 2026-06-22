using Core;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly IMongoCollection<Character> _collection;

    public CharacterRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Character>("Characters");
    }

    public async Task<List<Character>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<Character?> GetAsync(string id) =>
        await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<Character> CreateAsync(Character character)
    {
        await _collection.InsertOneAsync(character);
        return character;
    }

    public async Task UpdateAsync(string id, Character character) =>
        await _collection.ReplaceOneAsync(c => c.Id == id, character);

    public async Task DeleteAsync(string id) =>
        await _collection.DeleteOneAsync(c => c.Id == id);
}