using System.Collections.Generic;

namespace Core;

public class PlayerState
{
    public string? PlayerId { get; set; } = global::System.Guid.NewGuid().ToString();

    public string Name { get; set; } = "";

    public int CurrentQuestionIndex { get; set; }

    public List<CharacterSnapshot> Characters { get; set; } = new();

    public List<AnswerRecord> Answers { get; set; } = new();
}

public class CharacterSnapshot
{
    public string? CharacterId { get; set; } = global::System.Guid.NewGuid().ToString();

    public string Name { get; set; } = "";

    public CurrentStats CurrentStats { get; set; } = new();
}

public class AnswerRecord
{
    public string QuestionId { get; set; } = "";
    public string AnswerId { get; set; } = "";
}