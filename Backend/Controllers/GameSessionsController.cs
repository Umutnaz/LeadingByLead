using Backend.Repositories;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GameSessionsController : ControllerBase
{
    private const string FixedSessionId = "1";

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
        session.Id = FixedSessionId;
        session.Characters ??= new();
        session.Questions ??= new();
        session.Players = new();
        session.PlayerStates = new();

        foreach (var character in session.Characters)
            character.ResetCurrentStats();

        session.CurrentQuestionIndex = 0;
        session.QuestionPhase = QuestionPhase.Answering;
        session.State = GameState.Lobby;

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
            return NotFound("Sessionen findes ikke.");

        if (session.State != GameState.Lobby)
            return BadRequest("Spillet er allerede startet.");

        if (string.IsNullOrWhiteSpace(player.Name))
            return BadRequest("Navn mangler.");

        var duplicateName = session.Players.Any(
            existing =>
                existing.Name.Trim().ToLowerInvariant() ==
                player.Name.Trim().ToLowerInvariant());

        if (duplicateName)
            return BadRequest("Der er allerede en spiller med det navn.");

        if (string.IsNullOrWhiteSpace(player.Id))
            player.Id = Guid.NewGuid().ToString();

        var duplicatePlayerId = session.Players.Any(
            existing => existing.Id == player.Id);

        if (duplicatePlayerId)
            return Ok(player);

        player.Role = PlayerRole.Player;
        player.Name = player.Name.Trim();

        await _repository.JoinPlayerAsync(id, player);

        return Ok(player);
    }

    [HttpDelete("{id}/players/{playerId}")]
    public async Task<IActionResult> RemovePlayer(
        string id,
        string playerId)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        await _repository.RemovePlayerAsync(id, playerId);
        await _repository.RemovePlayerStateAsync(id, playerId);

        return NoContent();
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        if (session.Players.Count == 0)
            return BadRequest("Der skal være mindst én spiller.");

        foreach (var character in session.Characters)
            character.ResetCurrentStats();

        session.CurrentQuestionIndex = 0;
        session.QuestionPhase = QuestionPhase.Answering;
        session.State = GameState.Running;

        await _repository.UpdateAsync(id, session);

        return NoContent();
    }

    [HttpPost("{id}/next")]
    public async Task<IActionResult> Next(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        if (session.State != GameState.Running)
            return BadRequest("Spillet kører ikke.");

        if (session.QuestionPhase == QuestionPhase.Answering)
        {
            session.QuestionPhase = QuestionPhase.Results;
        }
        else
        {
            session.CurrentQuestionIndex++;

            if (session.CurrentQuestionIndex >= session.Questions.Count)
            {
                session.State = GameState.Finished;
            }
            else
            {
                session.QuestionPhase = QuestionPhase.Answering;
            }
        }

        await _repository.UpdateAsync(id, session);

        return NoContent();
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> Finish(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

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

        if (session.State != GameState.Running)
            return BadRequest("Spillet kører ikke.");

        if (state.CurrentQuestionIndex != session.CurrentQuestionIndex)
            return BadRequest("Spilleren svarer på et forkert spørgsmål.");

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

    [HttpGet("{id}/playerstates/current")]
    public async Task<ActionResult<List<PlayerState>>> GetCurrentPlayerStates(
        string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        var states = await _repository.GetPlayerStatesAsync(id);

        return states
            .Where(state =>
                state.CurrentQuestionIndex ==
                session.CurrentQuestionIndex)
            .ToList();
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
}