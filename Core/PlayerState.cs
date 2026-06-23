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
}

public class AnswerRecord
{
    public string QuestionId { get; set; } = "";

    public List<string> AnswerIds { get; set; } = new();
}