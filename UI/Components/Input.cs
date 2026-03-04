using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;

namespace PSFuzzySelect.UI.Components;

public class Input(string prompt, string query) : IComponent
{
    private string Prompt { get; } = prompt + ' ';
    public string Query { get; private set; } = query;

    /// <summary>The position of the cursor within the query</summary>
    private int _cursor = 0;

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
        // Handle left and right navigation
        if (key.Key == ConsoleKey.LeftArrow)
        {
            if (_cursor > 0) _cursor--; // Move cursor left by 1 character
        }
        else if (key.Key == ConsoleKey.RightArrow)
        {
            if (_cursor < Query.Length) _cursor++; // Move cursor right by 1 character
        }

        // Handle character input for search query
        else if (!char.IsControl(key.KeyChar))
        {
            Query += key.KeyChar;
            _cursor++;  // Advance the cursor by 1 character
            return new QueryChange(Query);
        }

        // Handle delete and backspace
        else if (key.Key == ConsoleKey.Backspace)
        {
            if (Query.Length == 0) return null; // Nothing to remove

            var countToRemove = 1; // Number of characters to remove

            // If Ctrl is held, remove the last word instead of just one character
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                var lastWordBoundary = FindLastWordBoundary();
                countToRemove = Query.Length - lastWordBoundary - 1;
            }

            // Remove the appropriate number of characters from the end of the query
            Query = Query[..^countToRemove];
            _cursor = Query.Length;  // Move the cursor to the end of the query
            return new QueryChange(Query);
        }

        return null; // No relevant input to handle
    }

    private int FindLastWordBoundary()
    {
        var target = _cursor - 1;
        while (target >= 0 && !char.IsWhiteSpace(Query[target])) target--; // Move left until we find a whitespace character or reach the start of the string
        while (target >= 0 && char.IsWhiteSpace(Query[target])) target--; // Continue moving left until we find a non-whitespace character or reach the start of the string
        return target;
    }

    private int FindNextWordBoundary()
    {
        var target = _cursor;
        while (target < Query.Length && !char.IsWhiteSpace(Query[target])) target++; // Move right until we find a whitespace character or reach the end of the string
        while (target < Query.Length && char.IsWhiteSpace(Query[target])) target++; // Continue moving right until we find a non-whitespace character or reach the end of the string
        return target;
    }

}
