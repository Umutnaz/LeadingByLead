namespace Core;

public class Character
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public BaseStats BaseStats { get; set; } = new();

    public CurrentStats CurrentStats { get; set; } = new();

    public void ResetCurrentStats()
    {
        CurrentStats = new CurrentStats
        {
            TjenesteMotivation = BaseStats.TjenesteMotivation,
            Stress = BaseStats.Stress,
            Sociallyst = BaseStats.Sociallyst,
            Tillid = BaseStats.Tillid
        };
    }
}