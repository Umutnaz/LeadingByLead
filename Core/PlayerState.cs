namespace Core;

public class PlayerState
{
    public string PlayerId { get; set; } = "";

    public string Name { get; set; } = "";

    public int CurrentQuestionIndex { get; set; }

    public string LatestQuestionId { get; set; } = "";

    public List<CharacterSnapshot> Characters { get; set; } = new();

    public List<CharacterResult> LatestResults { get; set; } = new();

    public List<AnswerRecord> Answers { get; set; } = new();

    /// <summary>
    /// Total engagement effect from the latest answer selection
    /// </summary>
    public double LatestEngagementEffect { get; set; } = 0;

    /// <summary>
    /// Phase events that occurred (e.g., at start of Drift phase)
    /// </summary>
    public List<PhaseEventResult> LatestPhaseEvents { get; set; } = new();
}

public class CharacterSnapshot
{
    public string CharacterId { get; set; } = "";

    public string Name { get; set; } = "";

    public CurrentStats CurrentStats { get; set; } = new();
}

public class CharacterResult
{
    public string CharacterId { get; set; } = "";

    public string CharacterName { get; set; } = "";

    public CurrentStats Before { get; set; } = new();

    public CurrentStats After { get; set; } = new();

    /// <summary>
    /// Reactions from this character's effects, one per selected answer/card
    /// </summary>
    public List<string> Reactions { get; set; } = new();

    /// <summary>
    /// Priority multiplier for action cards (100%, 85%, or 70%)
    /// </summary>
    public double PriorityMultiplier { get; set; } = 1.0;
}

public class PhaseEventResult
{
    public string CharacterName { get; set; } = "";

    public CurrentStats Effect { get; set; } = new();

    public string Explanation { get; set; } = "";
}

public class AnswerRecord
{
    public string QuestionId { get; set; } = "";

    public List<string> AnswerIds { get; set; } = new();
}