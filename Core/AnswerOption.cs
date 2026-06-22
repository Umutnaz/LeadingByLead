using System.Collections.Generic;

namespace Core;

public class AnswerOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Text { get; set; } = "";

    public List<Rule> Rules { get; set; } = new();
}