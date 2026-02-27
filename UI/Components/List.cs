using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Renderer;
using PSFuzzySelect.UI.Text;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components;

public class List(List<MatchResult> matches, int cursor) : IComponent
{
    public void Render(ISurface surface)
    {
        var visibleMatches = matches.Take(surface.Height).ToList(); // Use surface.Height to determine how many items to display

        for (var i = 0; i < visibleMatches.Count; i++)
        {
            var item = visibleMatches[i];
            bool isSelected = i == cursor;
            var cursorIndicator = isSelected ? "> " : "  ";
            
            // Create a sub-surface for each line to ensure the TextBlock is correctly aligned
            var lineSurface = surface.CreateSubSurface(new Rect(0, i, surface.Width, 1));
            
            new TextBlock()
                .Add(new TextSpan(cursorIndicator, Style.Default.WithForeground(Ansi.Color.Cyan)))
                .Add(new TextSpan(item.DisplayString, isSelected ? Style.Default.Inverse() : Style.Default))
                .Add(new TextSpan($" (Score: {item.Score})", Style.Default.Dim()))
                .Overflow(TextOverflow.Ellipsis)
                .Render(lineSurface);
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
