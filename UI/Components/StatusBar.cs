namespace PSFuzzySelect.UI.Components;

public class StatusBar
{
    public static void Render(int matchCount, int cursor)
    {
        Console.WriteLine($"[{cursor + 1}/{matchCount}]");
    }
}
