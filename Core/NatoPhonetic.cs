namespace Core;

public static class NatoPhonetic
{
    private static readonly string[] Labels =
    {
        "ALPHA",
        "BRAVO",
        "CHARLIE",
        "DELTA",
        "ECHO",
        "FOXTROT",
        "GOLF",
        "HOTEL"
    };

    public static string GetLabel(int index)
    {
        return index >= 0 && index < Labels.Length
            ? Labels[index]
            : $"OPTION {index + 1}";
    }
}
