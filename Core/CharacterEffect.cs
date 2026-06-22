namespace Core;

public class CharacterEffect
{
    public string CharacterId { get; set; } = "";

    public List<StatChange> Changes { get; set; } = new();
}