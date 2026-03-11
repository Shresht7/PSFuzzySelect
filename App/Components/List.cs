using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.App.Components;

public class List(
    List<MatchResult> matches,
    bool isMultiSelect,
    Func<object, bool> isSelected
) : IComponent
{
    /// <summary>The current list of matches to show in the UI</summary>
    public List<MatchResult> Matches { get; private set; } = matches;

    private readonly bool _isMultiSelect = isMultiSelect;

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
    public void SetMatches(List<MatchResult> newMatches, bool preserveCursor = false)
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

        // Calculate the number of items we can actually see
        int visibleCount = Math.Min(surface.Height, Matches.Count - _scrollOffset);

        // Render loop
        for (int i = 0; i < visibleCount; i++)
        {
            int matchIndex = i + _scrollOffset;
            var item = Matches[matchIndex];

            bool isCurrent = matchIndex == Cursor;
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


            // Create a sub-surface for each line to ensure the TextBlock is correctly aligned
            var lineSurface = surface.CreateSubSurface(new Rect(0, i, surface.Width, 1));

            new TextBlock()
                .Add(new TextSpan(cursorIndicator, cursorStyle))
                .Add(new TextSpan(selectionIndicator, selectionStyle))
                .AddRange(GetHighlightedSpans(item.DisplayString, item.Positions, isCurrent, isChecked))
                .Overflow(TextOverflow.Ellipsis)
                .Render(lineSurface);
        }
    }

    private static IEnumerable<TextSpan> GetHighlightedSpans(string displayString, int[] positions, bool isCurrent, bool isSelected = false)
    {
        var baseStyle = isCurrent ? Style.Default.Inverse() : Style.Default.WithForeground(Color.BrightBlack);
        if (isSelected) baseStyle = baseStyle.WithForeground(Color.BrightWhite).Bold();
        var highlightStyle = baseStyle.WithForeground(Color.Yellow).Bold();

        if (string.IsNullOrEmpty(displayString)) yield break;

        // Normalize positions: filter out-of-range, sort and dedupe
        if (positions == null || positions.Length == 0)
        {
            yield return new TextSpan(displayString.AsMemory(), baseStyle);
            yield break;
        }

        // filter, sort and dedupe positions
        var tmp = new List<int>(positions.Length);
        for (int idx = 0; idx < positions.Length; idx++)
        {
            int p = positions[idx];
            if (p >= 0 && p < displayString.Length)
            {
                tmp.Add(p);
            }
        }

        if (tmp.Count == 0)
        {
            yield return new TextSpan(displayString.AsMemory(), baseStyle);
            yield break;
        }

        tmp.Sort();
        var dedup = new List<int>(tmp.Count);
        int prev = int.MinValue;
        foreach (var v in tmp)
        {
            if (v != prev)
            {
                dedup.Add(v);
                prev = v;
            }
        }

        var pos = dedup.ToArray();

        int lastIdx = 0;
        int i = 0;
        while (i < pos.Length)
        {
            int start = pos[i];

            // non-highlighted before this highlighted run
            if (start > lastIdx)
            {
                yield return new TextSpan(displayString.AsMemory(lastIdx, start - lastIdx), baseStyle);
            }

            // coalesce adjacent highlighted positions into one highlighted span
            int end = start + 1;
            i++;
            while (i < pos.Length && pos[i] == end)
            {
                end++;
                i++;
            }

            yield return new TextSpan(displayString.AsMemory(start, end - start), highlightStyle);
            lastIdx = end;
        }

        // trailing non-highlighted
        if (lastIdx < displayString.Length)
        {
            yield return new TextSpan(displayString.AsMemory(lastIdx, displayString.Length - lastIdx), baseStyle);
        }
    }

    public Message? HandleKey(ConsoleKeyInfo key)
    {
        bool shiftPressed = (key.Modifiers & ConsoleModifiers.Shift) != 0;
        bool ctrlPressed = (key.Modifiers & ConsoleModifiers.Control) != 0;

        return key.Key switch
        {
            ConsoleKey.Tab => _isMultiSelect
                ? ToggleAndMove(shiftPressed ? -1 : 1)
                : CursorMove(shiftPressed ? -1 : 1),

            ConsoleKey.UpArrow when ctrlPressed => CursorMove(-Cursor),
            ConsoleKey.UpArrow => CursorMove(-1),

            ConsoleKey.DownArrow when ctrlPressed => CursorMove(Matches.Count - Cursor - 1),
            ConsoleKey.DownArrow => CursorMove(1),
            _ => null
        };
    }

    /// <summary>
    /// Toggles the selection of an item and moves the cursor.
    /// In the "Down" direction (delta > 0), it toggles before moving.
    /// In the "Up" direction (delta < 0), it moves before toggling.
    /// </summary>
    private Select? ToggleAndMove(int delta)
    {
        if (Matches.Count == 0 || Cursor < 0 || Cursor >= Matches.Count) return null;

        if (delta > 0)
        {
            // Toggle current, then move down
            var msg = new Select(Matches[Cursor].Item);
            CursorMove(delta);
            return msg;
        }
        else
        {
            // Move up, then toggle new current
            CursorMove(delta);
            return new Select(Matches[Cursor].Item);
        }
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
