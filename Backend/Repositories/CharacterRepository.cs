using Backend.Repositories;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CharactersController : ControllerBase
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IQuestionRepository _questionRepository;

    public CharactersController(
        ICharacterRepository characterRepository,
        IQuestionRepository questionRepository)
    {
        _characterRepository = characterRepository;
        _questionRepository = questionRepository;
    }

    [HttpGet]
    public async Task<List<Character>> Get()
    {
        return await _characterRepository.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Character>> Get(string id)
    {
        var character = await _characterRepository.GetAsync(id);

        if (character == null)
            return NotFound();

        return character;
    }

    [HttpPost]
    public async Task<ActionResult<Character>> Create(
        [FromBody] Character character)
    {
        NormalizeCharacter(character);
        character.ResetCurrentStats();

        var created = await _characterRepository.CreateAsync(character);

        return CreatedAtAction(
            nameof(Get),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] Character character)
    {
        NormalizeCharacter(character);

        character.Id = id;
        character.ResetCurrentStats();

        await _characterRepository.UpdateAsync(id, character);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _characterRepository.DeleteAsync(id);

        // Fjern characteren fra alle gemte spørgsmål.
        await _questionRepository.RemoveCharacterEffectsAsync(id);

        return NoContent();
    }

    private static void NormalizeCharacter(Character character)
    {
        character.BaseStats ??= new BaseStats();

        character.BaseStats.TjenesteMotivation = Math.Clamp(
            character.BaseStats.TjenesteMotivation,
            0,
            100);

        character.BaseStats.Stress = Math.Clamp(
            character.BaseStats.Stress,
            0,
            100);

        character.BaseStats.Sociallyst = Math.Clamp(
            character.BaseStats.Sociallyst,
            0,
            100);

        character.BaseStats.Tillid = Math.Clamp(
            character.BaseStats.Tillid,
            0,
            100);
    }
}