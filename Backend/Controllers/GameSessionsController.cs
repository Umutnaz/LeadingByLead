using Backend.Repositories;
using Backend.Services;
using Core;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GameSessionsController : ControllerBase
{
    private const string FixedSessionId = "1";

    private readonly IGameSessionRepository _repository;

    // Cache for excel data to avoid repeated file IO
    private static JsonDocument? _excelDoc;

    public GameSessionsController(
        IGameSessionRepository repository)
    {
        _repository = repository;
    }

    private static void EnsureExcelLoaded()
    {
        if (_excelDoc != null)
            return;

        // Try several candidate paths relative to current directory
        var candidates = new[]
        {
            "excel_data.json",
            "..\\excel_data.json",
            "..\\..\\excel_data.json",
            Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\excel_data.json"),
        };

        foreach (var path in candidates)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    _excelDoc = JsonDocument.Parse(json);
                    return;
                }
            }
            catch
            {
                // ignore and continue
            }
        }

        // If not found, leave _excelDoc null
    }

    private static string? FindExplanationForOption(string optionText, string characterId)
    {
        if (string.IsNullOrWhiteSpace(optionText) || string.IsNullOrWhiteSpace(characterId))
            return null;

        EnsureExcelLoaded();
        if (_excelDoc == null)
            return null;

        try
        {
            var root = _excelDoc.RootElement;

            // Search in gears
            if (root.TryGetProperty("gears", out var gears))
            {
                foreach (var item in gears.EnumerateArray())
                {
                    if (item.TryGetProperty("ID", out var idProp) && idProp.GetString() == characterId)
                    {
                        // Try match by Gearnavn contained in option text
                        if (item.TryGetProperty("Gearnavn", out var gn))
                        {
                            var gearName = gn.GetString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(gearName) && optionText.Contains(gearName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (item.TryGetProperty("Psykologisk begrundelse", out var expl))
                                    return expl.GetString();
                            }
                        }
                    }
                }
            }

            // Search in action_cards (kortnavn)
            if (root.TryGetProperty("action_cards", out var cards))
            {
                foreach (var item in cards.EnumerateArray())
                {
                    if (item.TryGetProperty("ID", out var idProp) && idProp.GetString() == characterId)
                    {
                        if (item.TryGetProperty("Kortnavn", out var kn))
                        {
                            var cardName = kn.GetString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(cardName) && optionText.Contains(cardName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (item.TryGetProperty("Psykologisk begrundelse", out var expl))
                                    return expl.GetString();
                            }
                        }
                    }
                }
            }

            // Fallback: try to find any entry for characterId with a Psykologisk begrundelse
            if (root.TryGetProperty("gears", out var gears2))
            {
                foreach (var item in gears2.EnumerateArray())
                {
                    if (item.TryGetProperty("ID", out var idProp) && idProp.GetString() == characterId)
                    {
                        if (item.TryGetProperty("Psykologisk begrundelse", out var expl))
                            return expl.GetString();
                    }
                }
            }
        }
        catch
        {
            // ignore parsing errors
        }

        return null;
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
        session.EffectsRevealed = false;

        await _repository.UpdateAsync(id, session);

        return NoContent();
    }

    [HttpPost("{id}/reveal-effects")]
    public async Task<IActionResult> RevealEffects(string id)
    {
        var session = await _repository.GetAsync(id);

        if (session == null)
            return NotFound();

        if (session.State != GameState.Running)
            return BadRequest("Spillet kører ikke.");

        // Mark effects revealed so player UIs can display the preview
        session.EffectsRevealed = true;

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

        // Reset the reveal flag whenever we change phase
        session.EffectsRevealed = false;

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

        // Store character states before applying impacts
        var characterSnapshots = session.Characters
            .Select(c => new CharacterSnapshot
            {
                CharacterId = c.Id,
                Name = c.Name,
                CurrentStats = new CurrentStats
                {
                    TjenesteMotivation = c.CurrentStats.TjenesteMotivation,
                    Sociallyst = c.CurrentStats.Sociallyst,
                    Tillid = c.CurrentStats.Tillid,
                    Stress = c.CurrentStats.Stress
                }
            })
            .ToList();

        state.Characters = characterSnapshots;

        // Get the current question
        var currentQuestion = session.Questions
            .ElementAtOrDefault(session.CurrentQuestionIndex);

        if (currentQuestion != null && state.Answers.Count > 0)
        {
            // Get the selected answer IDs
            var latestAnswer = state.Answers
                .OrderByDescending(a => session.Questions
                    .FindIndex(q => q.Id == a.QuestionId))
                .FirstOrDefault();

            if (latestAnswer != null)
            {
                double totalEngagementEffect = 0;

                // Apply each selected answer with appropriate multiplier
                for (int i = 0; i < latestAnswer.AnswerIds.Count; i++)
                {
                    var answerId = latestAnswer.AnswerIds[i];
                    var option = currentQuestion.AnswerOptions
                        .FirstOrDefault(ao => ao.Id == answerId);

                    if (option != null)
                    {
                        // Calculate multiplier based on selection order (for action cards)
                        double multiplier = currentQuestion.RequiredSelections == 3
                            ? i switch
                            {
                                0 => 1.0,  // First: 100%
                                1 => 0.85, // Second: 85%
                                2 => 0.70, // Third: 70%
                                _ => 1.0
                            }
                            : 1.0; // Single selection: always 100%

                        // Apply the impact with multiplier
                        var engagementEffect = RuleEvaluator
                            .ApplyAnswerOptionToCharacters(
                                session.Characters,
                                option,
                                multiplier);

                        totalEngagementEffect += engagementEffect;

                        // Collect reactions for each answer
                        foreach (var effect in option.CharacterEffects)
                        {
                            var playerResult = state.LatestResults
                                .FirstOrDefault(r => r.CharacterId == effect.CharacterId);

                            if (playerResult == null)
                            {
                                playerResult = new CharacterResult
                                {
                                    CharacterId = effect.CharacterId,
                                    CharacterName = session.Characters
                                        .First(c => c.Id == effect.CharacterId).Name,
                                    PriorityMultiplier = multiplier,
                                    Before = characterSnapshots
                                        .First(cs => cs.CharacterId == effect.CharacterId)
                                        .CurrentStats
                                };
                                state.LatestResults.Add(playerResult);
                            }

                            if (!string.IsNullOrEmpty(effect.Reaction))
                            {
                                var combined = effect.Reaction;

                                // Prefer explicit Explanation on the effect; otherwise try to find one in the excel JSON
                                var explanation = !string.IsNullOrWhiteSpace(effect.Explanation)
                                    ? effect.Explanation
                                    : FindExplanationForOption(option.Text ?? string.Empty, effect.CharacterId) ?? string.Empty;

                                if (!string.IsNullOrWhiteSpace(explanation))
                                    combined = combined + " — " + explanation;

                                playerResult.Reactions.Add(combined);
                            }
                        }
                    }
                }

                // Set after stats for all results
                foreach (var result in state.LatestResults)
                {
                    var character = session.Characters
                        .First(c => c.Id == result.CharacterId);

                    result.After = new CurrentStats
                    {
                        TjenesteMotivation = character.CurrentStats.TjenesteMotivation,
                        Sociallyst = character.CurrentStats.Sociallyst,
                        Tillid = character.CurrentStats.Tillid,
                        Stress = character.CurrentStats.Stress
                    };
                }

                state.LatestEngagementEffect = totalEngagementEffect;
            }
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