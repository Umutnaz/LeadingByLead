namespace Core;

public class Character
{
    public string Id { get; set; } = global::System.Guid.NewGuid().ToString();

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public CurrentStats CurrentStats { get; set; } = new();
    public CharacterTraits Traits { get; set; } = new();
}