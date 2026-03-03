using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;

namespace PSFuzzySelect.UI.Components;

public class Input(string prompt, string query) : IComponent
{
    public void Render(ISurface surface)
    {
        new TextBlock(
            new TextSpan(prompt + " ", Style.Default.WithForeground(Color.Blue)),
            new TextSpan(query, Style.Default.WithForeground(Color.White))
        ).Render(surface);
    }

    public static Message? HandleKey(ConsoleKeyInfo key, string currentQuery)
    {
        // Handle character input for search query
        if (!char.IsControl(key.KeyChar))
        {
            return new QueryChange(currentQuery + key.KeyChar);
        }
        else if (key.Key == ConsoleKey.Backspace && currentQuery.Length > 0)
        {
            var countToRemove = 1; // Number of characters to remove

            // If Ctrl is held, remove the last word instead of just one character
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                var words = currentQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    countToRemove = words.Last().Length;
                }
            }

            // Remove the appropriate number of characters from the end of the query
            return new QueryChange(currentQuery.TrimEnd()[..^countToRemove]);
        }

        return null; // No relevant input to handle
    }
}
