using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Components;

public class List(List<MatchResult> matches, int cursor) : IComponent
{
    public void Render(ISurface surface)
    {
        var visibleMatches = matches.Take(5).ToList(); // Limit to top 5 matches for now to keep the display manageable

        for (var i = 0; i < visibleMatches.Count; i++)
        {
            var item = visibleMatches[i];
            var cursorIndicator = i == cursor ? ">" : " ";
            surface.Write(0, i, $"{cursorIndicator} {item.DisplayString} (Score: {item.Score})");
        }
    }

    public static Message? HandleKey(ConsoleKeyInfo key)
    {
        return key.Key switch
        {
            ConsoleKey.Enter => new Select(),
            ConsoleKey.UpArrow => new CursorMove(-1),
            ConsoleKey.DownArrow => new CursorMove(1),
            _ => null
        };
    }
}
