using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components;

public class List(
    List<MatchResult> matches,
    bool isMultiSelect,
    Func<object, string> displaySelector,
    Func<object, bool> isSelected
) : IComponent
{
    /// <summary>The current list of matches to show in the UI</summary>
    public IReadOnlyList<MatchResult> Matches { get; private set; } = matches;

    private readonly bool _isMultiSelect = isMultiSelect;

    private readonly Func<object, string> _displaySelector = displaySelector;
    private readonly Func<object, bool> _isSelected = isSelected;

    /// <summary>
    /// The current index of the highlighted item in the list of matches.
    /// This is updated in response to user input and is used to track the user's selection.
    /// </summary>
    public int Cursor { get; private set; } = 0;

    /// <summary>A scroll offset to determine which portion of the matches list is currently visible in the UI</summary>
    private int _scrollOffset = 0;

    /// <summary>
    /// Updates the list of matches and ensures the cursor remains within valid bounds.
    /// </summary>
    /// <param name="newMatches">The new list of match results to display.</param>
    /// <param name="preserveCursor">Whether to attempt to preserve the current cursor position if possible, or reset to the top of the list.</param>
    public void SetMatches(IReadOnlyList<MatchResult> newMatches, bool preserveCursor = false)
    {
        Matches = newMatches; // Update the matches list

        if (!preserveCursor)
        {
            Cursor = 0; // Reset cursor to the top of the list whenever matches are updated
            _scrollOffset = 0; // Reset scroll offset to ensure the top of the list is visible
            return;
        }

        if (Matches.Count == 0)
        {
            Cursor = -1; // No matches, set cursor to an invalid position
            _scrollOffset = 0; // Reset scroll offset
            return;
        }

        // Ensure the cursor is within the bounds of the new matches list
        Cursor = Math.Clamp(Cursor, 0, Matches.Count - 1);

        // Keep the scroll offset valid
        if (_scrollOffset > Cursor) _scrollOffset = Cursor;
        if (_scrollOffset < 0) _scrollOffset = 0;
    }

    public void Render(ISurface surface)
    {
        if (Matches.Count == 0) return; // Don't render anything if there are no matches to display

        // Scroll the list if the cursor moves outside the visible area
        if (Cursor < _scrollOffset)
            _scrollOffset = Cursor;
        else if (Cursor >= _scrollOffset + surface.Height)
            _scrollOffset = Cursor - surface.Height + 1;

        // Use surface.Height to determine how many items to display
        var visibleMatches = Matches.Skip(_scrollOffset).Take(surface.Height).ToList();

        // Render loop update
        for (var i = 0; i < visibleMatches.Count; i++)
        {
            var item = visibleMatches[i];
            bool isCurrent = i + _scrollOffset == Cursor;
            bool isChecked = _isSelected(item.Item);

            var cursorIndicator = isCurrent ? "❯ " : "  ";
            var cursorStyle = isCurrent ? Style.Default.WithForeground(Color.BrightCyan).Bold() : Style.Default;

            var selectionIndicator = string.Empty;
            var selectionStyle = Style.Default.WithForeground(Color.Cyan);
            if (_isMultiSelect)
            {
                selectionIndicator = isChecked ? "[◈] " : "[ ] ";
                selectionStyle = isChecked ? selectionStyle.Bold() : selectionStyle.Dim();
            }

            // Fetch the display string for the item using the provided display selector function
            var displayString = _displaySelector(item.Item);

            // Create a sub-surface for each line to ensure the TextBlock is correctly aligned
            var lineSurface = surface.CreateSubSurface(new Rect(0, i, surface.Width, 1));

            new TextBlock()
                .Add(new TextSpan(cursorIndicator, cursorStyle))
                .Add(new TextSpan(selectionIndicator, selectionStyle))
                .AddRange(GetHighlightedSpans(displayString, item.Positions, isCurrent, isChecked))
                .Overflow(TextOverflow.Ellipsis)
                .Render(lineSurface);
        }
    }

    private static IEnumerable<TextSpan> GetHighlightedSpans(string displayString, int[] positions, bool isCurrent, bool isSelected = false)
    {
        var baseStyle = isCurrent ? Style.Default.Inverse() : Style.Default.WithForeground(Color.BrightBlack);
        if (isSelected) baseStyle = baseStyle.WithForeground(Color.BrightWhite).Bold();
        var highlightStyle = baseStyle.WithForeground(Color.Yellow).Bold();

        // If there are no highlighted positions, return the entire string as a single span
        if (positions.Length == 0)
        {
            yield return new TextSpan(displayString, baseStyle);
            yield break;
        }

        // Otherwise, split the string into spans based on the highlighted positions
        int lastIdx = 0;    // Track the last index we processed in the string
        foreach (var position in positions)
        {
            // Yield the non-highlighted part before the highlighted character
            if (position > lastIdx)
            {
                yield return new TextSpan(displayString[lastIdx..position], baseStyle);
            }
            // Yield the highlighted character
            yield return new TextSpan(displayString[position].ToString(), highlightStyle);
            // Update the last index
            lastIdx = position + 1;
        }

        // Yield any remaining non-highlighted part after the last highlighted character
        if (lastIdx < displayString.Length)
        {
            yield return new TextSpan(displayString[lastIdx..], baseStyle);
        }
    }

    public Message? HandleKey(ConsoleKeyInfo key)
    {
        return key.Key switch
        {
            ConsoleKey.Tab => SelectItem(Matches[Cursor].Item),
            ConsoleKey.UpArrow when key.Modifiers.HasFlag(ConsoleModifiers.Control) => CursorMove(-Cursor),
            ConsoleKey.UpArrow => CursorMove(-1),
            ConsoleKey.DownArrow when key.Modifiers.HasFlag(ConsoleModifiers.Control) => CursorMove(Matches.Count - Cursor - 1),
            ConsoleKey.DownArrow => CursorMove(1),
            _ => null
        };
    }

    /// <summary>
    /// Creates a message to select the currently highlighted item in the list of matches.
    /// This is triggered when the user presses the Tab or Spacebar key, indicating they want to select the current item.
    /// </summary>
    /// <param name="item">The item to be selected</param>
    /// <returns>A message indicating the selection of the item, or null if the selection is invalid</returns>
    private Select? SelectItem(object item)
    {
        if (Cursor >= 0 && Cursor < Matches.Count)
        {
            if (Cursor + 1 < Matches.Count) Cursor++; // Advance the cursor to the next item after selection, if possible
            return new Select(item);
        }
        return null;
    }

    /// <summary>
    /// Moves the cursor up or down in the list of matches based on the provided delta value,
    /// ensuring that the cursor stays within the bounds of the current matches.
    /// </summary>
    /// <param name="delta">The number of positions to move the cursor. Positive values move the cursor down, negative values move it up.</param>
    private HighlightChange? CursorMove(int delta)
    {
        // If there are no matches, reset the cursor to an invalid position
        if (Matches.Count == 0)
        {
            Cursor = -1;
            return null;
        }

        // Move the cursor by the specified delta, ensuring it stays within the bounds of the matches list
        Cursor = Math.Clamp(Cursor + delta, 0, Matches.Count - 1);

        return new HighlightChange(Cursor);
    }
}
