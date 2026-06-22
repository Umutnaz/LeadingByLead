namespace Core;

public class Player
{
    public string Id { get; set; } = global::System.Guid.NewGuid().ToString();

    public string Name { get; set; } = "";

    public PlayerRole Role { get; set; } = PlayerRole.Player;

    public bool IsReady { get; set; } = false;

    public int? SpillerNummer { get; set; }
}