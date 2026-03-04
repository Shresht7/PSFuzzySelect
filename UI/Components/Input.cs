using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;

namespace PSFuzzySelect.UI.Components;

public class Input(string prompt, string query) : IComponent
{
    private string Prompt { get; } = prompt + ' ';
    public string Query { get; private set; } = query;

    /// <summary>The position of the cursor within the query</summary>
    private int _cursor = query.Length;

    public void Render(ISurface surface)
    {
        new TextBlock(
            new TextSpan(Prompt, Style.Default.WithForeground(Color.Blue)),
            new TextSpan(Query, Style.Default.WithForeground(Color.White))
        ).Render(surface);

        var cursorCell = surface.GetCell(Prompt.Length + _cursor, 0) with { Style = Style.Default.WithTextStyle(TextStyle.Inverse) };
        surface.Write(Prompt.Length + _cursor, 0, cursorCell);
    }

    public Message? HandleKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            // Cursor Navigation
            case ConsoleKey.LeftArrow:
                _cursor = key.Modifiers.HasFlag(ConsoleModifiers.Control)
                    ? Math.Max(0, FindLastWordBoundary() + 1)
                    : Math.Max(0, _cursor - 1);
                break;
            case ConsoleKey.RightArrow:
                _cursor = key.Modifiers.HasFlag(ConsoleModifiers.Control)
                    ? Math.Min(Query.Length, FindNextWordBoundary())
                    : Math.Min(Query.Length, _cursor + 1);
                break;
            case ConsoleKey.Home:
                _cursor = 0;
                break;
            case ConsoleKey.End:
                _cursor = Query.Length;
                break;

            // Deletion
            case ConsoleKey.Backspace:
                if (Query.Length == 0 || _cursor == 0) break; // Nothing to remove

                // Determine how many characters to remove based on whether Ctrl is held
                var countToRemove = key.Modifiers.HasFlag(ConsoleModifiers.Control)
                    ? _cursor - FindLastWordBoundary() - 1
                    : 1;

                Query = Query[..^countToRemove]; // Remove the appropriate number of characters from the end of the query
                _cursor = Math.Max(0, _cursor - countToRemove); // Move the cursor left by the number of characters removed
                return new QueryChange(Query);
            case ConsoleKey.Delete:
                if (Query.Length == 0 || _cursor >= Query.Length) break; // Nothing to remove

                // Determine how many characters to remove based on whether Ctrl is held
                countToRemove = key.Modifiers.HasFlag(ConsoleModifiers.Control)
                    ? FindNextWordBoundary() - _cursor
                    : 1;

                Query = Query.Remove(_cursor, Math.Min(countToRemove, Query.Length - _cursor)); // Remove the appropriate number of characters starting from the cursor position
                return new QueryChange(Query);

            // Character Input
            default:
                if (!char.IsControl(key.KeyChar))
                {
                    Query = Query.Insert(_cursor, key.KeyChar.ToString()); // Insert the character at the cursor position
                    _cursor++; // Advance the cursor by 1 character
                    return new QueryChange(Query);
                }
                break;
        }

        return null; // No relevant input to handle
    }

    /// <summary>
    /// Finds the index of the last word boundary to the left of the cursor.
    /// A word boundary is defined as a transition between a whitespace character and a non-whitespace character.
    /// </summary>
    /// <returns>The index of the last word boundary to the left of the cursor (or -1 if none is found)</returns>
    private int FindLastWordBoundary()
    {
        var target = _cursor - 1;
        while (target >= 0 && !char.IsWhiteSpace(Query[target])) target--; // Skip any whitespace immediately to the left of the cursor
        while (target >= 0 && char.IsWhiteSpace(Query[target])) target--; // Skip the word itself
        return target;
    }

    /// <summary>
    /// Finds the index of the next word boundary to the right of the cursor.
    /// A word boundary is defined as a transition between a whitespace character and a non-whitespace character.
    /// </summary>
    /// <returns>The index of the next word boundary to the right of the cursor (or the length of the query if none is found)</returns>
    private int FindNextWordBoundary()
    {
        var target = _cursor;
        while (target < Query.Length && !char.IsWhiteSpace(Query[target])) target++; // Move right until we find a whitespace character or reach the end of the string
        while (target < Query.Length && char.IsWhiteSpace(Query[target])) target++; // Continue moving right until we find a non-whitespace character or reach the end of the string
        return target;
    }

}
