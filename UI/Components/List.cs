using PSFuzzySelect.Core;

namespace PSFuzzySelect.UI.Components;

public class List
{
    public static Message? HandleKey(ConsoleKeyInfo key, List<MatchResult> matches, int cursor)
    {
        // Check if the user selected an item
        if (key.Key == ConsoleKey.Enter)
        {
            if (matches.Count > 0)
            {
                return new Select();
            }
        }

        // Handle cursor movement
        if (key.Key == ConsoleKey.UpArrow)
        {
            return new CursorMove(-1);
        }
        if (key.Key == ConsoleKey.DownArrow)
        {
            return new CursorMove(1);
        }

        return null;
    }

    public static void Render(List<MatchResult> matches, int cursor)
    {
        var visibleMatches = matches.Take(5).ToList(); // Limit to top 5 matches for now to keep the display manageable

        for (var i = 0; i < visibleMatches.Count; i++)
        {
            var item = visibleMatches[i];
            var cursorIndicator = i == cursor ? ">" : " ";
            Console.WriteLine($"{cursorIndicator} {item.DisplayString} (Score: {item.Score})");
        }
    }
}
