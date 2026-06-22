using Backend.Repositories;
using Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionRepository _repo;
    public QuestionsController(IQuestionRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<List<Question>> Get() => await _repo.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> Get(string id)
    {
        var q = await _repo.GetAsync(id);
        if (q == null) return NotFound();
        return q;
    }

    [HttpPost]
    public async Task<ActionResult<Question>> Create([FromBody] Question question)
    {
        var created = await _repo.CreateAsync(question);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Question question)
    {
        await _repo.UpdateAsync(id, question);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}

