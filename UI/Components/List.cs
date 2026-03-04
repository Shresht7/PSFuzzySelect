using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components;

public class List(List<MatchResult> matches, int cursor) : IComponent
{
    /// <summary>
    /// Updates the list of matches and ensures the cursor remains within valid bounds.
    /// </summary>
    /// <param name="newMatches">The new list of match results to display.</param>
    public void SetMatches(List<MatchResult> newMatches)
    {
        matches = newMatches;
        cursor = Math.Clamp(cursor, 0, Math.Max(0, matches.Count - 1));
    }

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
                .Add(new TextSpan(cursorIndicator, Style.Default.WithForeground(Color.Cyan)))
                .AddRange(GetHighlightedSpans(item, isSelected))
                .Add(new TextSpan($" (Score: {item.Score})", Style.Default.Dim()))
                .Overflow(TextOverflow.Ellipsis)
                .Render(lineSurface);
        }
    }

    private static IEnumerable<TextSpan> GetHighlightedSpans(MatchResult item, bool isSelected)
    {
        var baseStyle = isSelected ? Style.Default.Inverse() : Style.Default;
        var highlightStyle = baseStyle.WithForeground(Color.Yellow).Bold();

        // If there are no highlighted positions, return the entire string as a single span
        if (item.Positions.Length == 0)
        {
            yield return new TextSpan(item.DisplayString, baseStyle);
            yield break;
        }

        // Otherwise, split the string into spans based on the highlighted positions
        int lastIdx = 0;    // Track the last index we processed in the string
        foreach (var position in item.Positions)
        {
            // Yield the non-highlighted part before the highlighted character
            if (position > lastIdx)
            {
                yield return new TextSpan(item.DisplayString[lastIdx..position], baseStyle);
            }
            // Yield the highlighted character
            yield return new TextSpan(item.DisplayString[position].ToString(), highlightStyle);
            // Update the last index
            lastIdx = position + 1;
        }

        // Yield any remaining non-highlighted part after the last highlighted character
        if (lastIdx < item.DisplayString.Length)
        {
            yield return new TextSpan(item.DisplayString[lastIdx..], baseStyle);
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
