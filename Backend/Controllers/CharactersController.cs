using Backend.Repositories;
using Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CharactersController : ControllerBase
{
    private readonly ICharacterRepository _repo;
    public CharactersController(ICharacterRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<List<Character>> Get() => await _repo.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Character>> Get(string id)
    {
        var c = await _repo.GetAsync(id);
        if (c == null) return NotFound();
        return c;
    }

    [HttpPost]
    public async Task<ActionResult<Character>> Create([FromBody] Character character)
    {
        var created = await _repo.CreateAsync(character);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Character character)
    {
        await _repo.UpdateAsync(id, character);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}

