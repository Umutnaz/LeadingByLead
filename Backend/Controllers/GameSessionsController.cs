using Backend.Repositories;
using Backend.Services;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GameSessionsController : ControllerBase
{
    private readonly IGameSessionRepository _repository;

    public GameSessionsController(
        IGameSessionRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<ActionResult<GameSession>> Create(
        [FromBody] GameSession session)
    {
        session.Characters ??= new();

        // Hver session får sine egne kopier af CurrentStats.
        foreach (var character in session.Characters)
            character.ResetCurrentStats();

        var created = await _repository.CreateAsync(session);

        return CreatedAtAction(
            nameof(Get),
            new { id = created.Id },
            created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameSession>> Get(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        return session;
    }

    [HttpGet]
    public async Task<List<GameSession>> GetAll()
    {
        return await _repository.GetAllAsync();
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult<Player>> Join(
        string id,
        [FromBody] Player player)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(player.Id))
            player.Id = Guid.NewGuid().ToString();

        player.Role = PlayerRole.Player;

        await _repository.JoinPlayerAsync(id, player);

        return Ok(player);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        foreach (var character in session.Characters)
            character.ResetCurrentStats();

        session.State = GameState.Running;

        await _repository.UpdateAsync(id, session);

        return NoContent();
    }

    [HttpPost("{id}/next")]
    public async Task<IActionResult> Next(string id)
    {
        await _repository.AdvanceQuestionAsync(id);

        return NoContent();
    }

    [HttpPost("{id}/applyAnswer")]
    public async Task<IActionResult> ApplyAnswer(
        string id,
        [FromBody] ApplyAnswerRequest request)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        var question = session.Questions.FirstOrDefault(
            question => question.Id == request.QuestionId);

        if (question == null)
            return BadRequest("Spørgsmålet findes ikke i sessionen.");

        var answer = question.AnswerOptions.FirstOrDefault(
            answer => answer.Id == request.AnswerId);

        if (answer == null)
            return BadRequest("Svarmuligheden findes ikke.");

        RuleEvaluator.ApplyAnswerOptionToCharacters(
            session.Characters,
            answer);

        await _repository.UpdateAsync(id, session);

        return NoContent();
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> Finish(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        // Sessionens CurrentStats nulstilles.
        foreach (var character in session.Characters)
            character.ResetCurrentStats();

        session.State = GameState.Finished;

        await _repository.UpdateAsync(id, session);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);

        return NoContent();
    }

    [HttpPost("{id}/playerstate")]
    public async Task<IActionResult> PostPlayerState(
        string id,
        [FromBody] PlayerState state)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        var playerExists = session.Players.Any(
            player => player.Id == state.PlayerId);

        if (string.IsNullOrWhiteSpace(state.PlayerId) ||
            !playerExists)
        {
            return BadRequest("Spilleren er ikke med i sessionen.");
        }

        await _repository.PostPlayerStateAsync(id, state);

        return NoContent();
    }

    [HttpGet("{id}/playerstates")]
    public async Task<ActionResult<List<PlayerState>>> GetPlayerStates(
        string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        return await _repository.GetPlayerStatesAsync(id);
    }

    [HttpDelete("{id}/playerstate/{playerId}")]
    public async Task<IActionResult> DeletePlayerState(
        string id,
        string playerId)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        await _repository.RemovePlayerStateAsync(id, playerId);

        return NoContent();
    }

    public class ApplyAnswerRequest
    {
        public string PlayerId { get; set; } = "";
        public string QuestionId { get; set; } = "";
        public string AnswerId { get; set; } = "";
    }
}