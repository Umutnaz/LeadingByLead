using System.Collections.Generic;

namespace Core;

public class GameSession
{
    public string Id { get; set; } = global::System.Guid.NewGuid().ToString();

    public Player Host { get; set; } = new();

    public List<Player> Players { get; set; } = new();

    public List<Character> Characters { get; set; } = new();

    public List<Question> Questions { get; set; } = new();

    public int CurrentQuestionIndex { get; set; } = 0;

    public GameState State { get; set; } = GameState.Lobby;
    
    // Per-player snapshots (latest) - populated when players submit their local character states
    public List<PlayerState> PlayerStates { get; set; } = new();
}