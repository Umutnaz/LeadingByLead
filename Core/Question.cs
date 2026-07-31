namespace Core;

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = "";
    
    public string Description { get; set; } = "";

    public int RequiredSelections { get; set; } = 1;

    public List<AnswerOption> AnswerOptions { get; set; } = new();
    
    /// <summary>
    /// Full question text combining title and description for display purposes
    /// </summary>
    public string Text => string.IsNullOrWhiteSpace(Description) 
        ? Title 
        : $"{Title}\n\n{Description}";
}