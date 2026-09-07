namespace Core;

public class CharacterEffect
{
    public string CharacterId { get; set; } = "";

    public List<StatChange> Changes { get; set; } = new();

    public string Reaction { get; set; } = "";

    // The longer psychological explanation ("Psykologisk begrundelse")
    public string Explanation { get; set; } = "";
}