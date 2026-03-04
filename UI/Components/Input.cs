using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;

namespace PSFuzzySelect.UI.Components;

public class Input(string prompt, string query) : IComponent
{
    private string Prompt { get; } = prompt;
    public string Query { get; private set; } = query;

    public void Render(ISurface surface)
    {
        new TextBlock(
            new TextSpan(Prompt, Style.Default.WithForeground(Color.Blue)),
            new TextSpan(Query, Style.Default.WithForeground(Color.White))
        ).Render(surface);
    }

    public Message? HandleKey(ConsoleKeyInfo key)
    {
        // Handle character input for search query
        if (!char.IsControl(key.KeyChar))
        {
            Query += key.KeyChar;
            return new QueryChange(Query);
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            if (Query.Length == 0) return null; // Nothing to remove

            var countToRemove = 1; // Number of characters to remove

            // If Ctrl is held, remove the last word instead of just one character
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                var words = Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    countToRemove = words.Last().Length;
                }
            }

            // Remove the appropriate number of characters from the end of the query
            Query = Query.TrimEnd()[..^countToRemove];
            return new QueryChange(Query);
        }

        return null; // No relevant input to handle
    }
}
