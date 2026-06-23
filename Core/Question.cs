namespace Core;

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Text { get; set; } = "";

    public int RequiredSelections { get; set; } = 1;

    public List<AnswerOption> AnswerOptions { get; set; } = new();
}