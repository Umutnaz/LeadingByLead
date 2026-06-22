using Backend.Repositories;
using Backend.Services;
using Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GameSessionsController : ControllerBase
{
    private readonly IGameSessionRepository _repo;
    private readonly IQuestionRepository _questionRepo;

    public GameSessionsController(IGameSessionRepository repo, IQuestionRepository questionRepo)
    {
        _repo = repo;
        _questionRepo = questionRepo;
    }

    [HttpPost]
    public async Task<ActionResult<GameSession>> Create([FromBody] GameSession session)
    {
        var created = await _repo.CreateAsync(session);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameSession>> Get(string id)
    {
        var s = await _repo.GetAsync(id);
        if (s == null) return NotFound();
        return s;
    }

    [HttpGet]
    public async Task<List<GameSession>> GetAll() => await _repo.GetAllAsync();

    [HttpPost("{id}/join")]
    public async Task<ActionResult<Player>> Join(string id, [FromBody] Player player)
    {
        var session = await _repo.GetAsync(id);
        if (session == null) return NotFound();

        // Ensure player has id and role
        if (string.IsNullOrEmpty(player.Id)) player.Id = Guid.NewGuid().ToString();
        player.Role = PlayerRole.Player;

        await _repo.JoinPlayerAsync(id, player);

        return Ok(player);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id)
    {
        await _repo.SetStateAsync(id, GameState.Running);
        return NoContent();
    }

    [HttpPost("{id}/next")]
    public async Task<IActionResult> Next(string id)
    {
        await _repo.AdvanceQuestionAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/applyAnswer")]
    public async Task<IActionResult> ApplyAnswer(string id, [FromBody] ApplyAnswerRequest req)
    {
        var session = await _repo.GetAsync(id);
        if (session == null) return NotFound();

        var question = session.Questions.FirstOrDefault(q => q.Id == req.QuestionId);
        if (question == null)
            return BadRequest("Question not found in session.");

        var answer = question.AnswerOptions.FirstOrDefault(a => a.Id == req.AnswerId);
        if (answer == null) return BadRequest("Answer not found.");

        // By default this applies to the session's characters (global). For POC, clients should apply locally.
        RuleEvaluator.ApplyAnswerOptionToCharacters(session.Characters, answer);

        await _repo.UpdateAsync(session.Id!, session);
        return NoContent();
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> Finish(string id)
    {
        await _repo.SetStateAsync(id, GameState.Finished);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    // Player state endpoints: players POST their snapshot after answering; host can GET all snapshots
    [HttpPost("{id}/playerstate")]
    public async Task<IActionResult> PostPlayerState(string id, [FromBody] PlayerState state)
    {
        var session = await _repo.GetAsync(id);
        if (session == null) return NotFound();

        // Validate that the player is part of the session
        if (string.IsNullOrEmpty(state.PlayerId) || session.Players == null || !session.Players.Any(p => p.Id == state.PlayerId))
            return BadRequest("Player not part of session.");

        await _repo.PostPlayerStateAsync(id, state);
        return NoContent();
    }

    [HttpGet("{id}/playerstates")]
    public async Task<ActionResult<List<PlayerState>>> GetPlayerStates(string id)
    {
        var session = await _repo.GetAsync(id);
        if (session == null) return NotFound();
        var states = await _repo.GetPlayerStatesAsync(id);
        return states;
    }

    [HttpDelete("{id}/playerstate/{playerId}")]
    public async Task<IActionResult> DeletePlayerState(string id, string playerId)
    {
        var session = await _repo.GetAsync(id);
        if (session == null) return NotFound();
        await _repo.RemovePlayerStateAsync(id, playerId);
        return NoContent();
    }

    public class ApplyAnswerRequest
    {
        public string PlayerId { get; set; } = "";
        public string QuestionId { get; set; } = "";
        public string AnswerId { get; set; } = "";
    }
}
