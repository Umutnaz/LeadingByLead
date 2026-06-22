using Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Services.IService;

public interface ICharacterService
{
    Task<List<Character>> GetAllCharactersAsync();
    Task<Character> GetCharacterAsync(string id);
    Task<Character> CreateCharacterAsync(Character character);
    Task UpdateCharacterAsync(string id, Character character);
    Task DeleteCharacterAsync(string id);
}