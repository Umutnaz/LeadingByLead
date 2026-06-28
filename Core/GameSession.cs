namespace Core;

public class GameSession
{
    public string Id { get; set; } = "1";

    public Player Host { get; set; } = new();

    public List<Player> Players { get; set; } = new();

    public List<Character> Characters { get; set; } = new();

    public List<Question> Questions { get; set; } = new();

    public int CurrentQuestionIndex { get; set; }

    public QuestionPhase QuestionPhase { get; set; } =
        QuestionPhase.Answering;

    public GameState State { get; set; } = GameState.Lobby;

    public List<PlayerState> PlayerStates { get; set; } = new();
}