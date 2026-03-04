using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components;

public class List(List<MatchResult> matches, int cursor) : IComponent
{
    public IReadOnlyList<MatchResult> Matches => matches;

    public int Cursor => cursor;

    /// <summary>A scroll offset to determine which portion of the matches list is currently visible in the UI</summary>
    private int _scrollOffset = 0;

    /// <summary>
    /// Updates the list of matches and ensures the cursor remains within valid bounds.
    /// </summary>
    /// <param name="newMatches">The new list of match results to display.</param>
    public void SetMatches(List<MatchResult> newMatches)
    {
        matches = newMatches;
        cursor = 0; // Reset cursor to the top of the list whenever matches are updated
        _scrollOffset = 0; // Reset scroll offset to ensure the top of the list is visible
    }

    public void Render(ISurface surface)
    {
        if (matches.Count == 0) return; // Don't render anything if there are no matches to display

        // Scroll the list if the cursor moves outside the visible area
        if (cursor < _scrollOffset)
            _scrollOffset = cursor;
        else if (cursor >= _scrollOffset + surface.Height)
            _scrollOffset = cursor - surface.Height + 1;

        // Use surface.Height to determine how many items to display
        var visibleMatches = matches.Skip(_scrollOffset).Take(surface.Height).ToList();

        for (var i = 0; i < visibleMatches.Count; i++)
        {
            var item = visibleMatches[i];
            bool isSelected = i + _scrollOffset == cursor;
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

    public Message? HandleKey(ConsoleKeyInfo key)
    {
        return key.Key switch
        {
            ConsoleKey.Enter => new Select(),
            ConsoleKey.UpArrow when key.Modifiers.HasFlag(ConsoleModifiers.Control) => CursorMove(-cursor),
            ConsoleKey.UpArrow => CursorMove(-1),
            ConsoleKey.DownArrow when key.Modifiers.HasFlag(ConsoleModifiers.Control) => CursorMove(matches.Count - cursor - 1),
            ConsoleKey.DownArrow => CursorMove(1),
            _ => null
        };
    }

    /// <summary>
    /// Moves the cursor up or down in the list of matches based on the provided delta value,
    /// ensuring that the cursor stays within the bounds of the current matches.
    /// </summary>
    /// <param name="delta">The number of positions to move the cursor. Positive values move the cursor down, negative values move it up.</param>
    private Message? CursorMove(int delta)
    {
        // If there are no matches, reset the cursor to an invalid position
        if (matches.Count == 0)
        {
            cursor = -1;
            return null;
        }
        // Move the cursor by the specified delta, ensuring it stays within the bounds of the matches list
        cursor = Math.Clamp(cursor + delta, 0, matches.Count - 1);

        return null;
    }
}
