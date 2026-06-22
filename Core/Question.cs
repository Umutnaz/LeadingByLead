using System.Collections.Generic;

namespace Core;

public class Question
{
    public string Id { get; set; } = global::System.Guid.NewGuid().ToString();

    public string Text { get; set; } = "";

    public List<AnswerOption> AnswerOptions { get; set; } = new();
}