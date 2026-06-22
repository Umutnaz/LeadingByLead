using Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories;

public interface ICharacterRepository
{
    Task<List<Character>> GetAllAsync();
    Task<Character?> GetAsync(string id);
    Task<Character> CreateAsync(Character character);
    Task UpdateAsync(string id, Character character);
    Task DeleteAsync(string id);
}

