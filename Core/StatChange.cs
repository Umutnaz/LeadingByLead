namespace Core;

public class StatChange
{
    public string StatName { get; set; } = nameof(CurrentStats.Stress);

    // Må være mellem -100 og 100.
    public int Amount { get; set; }
}