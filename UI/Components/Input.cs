using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Text;

namespace PSFuzzySelect.UI.Components;

public class Input(string query) : IComponent
{
    public void Render(ISurface surface)
    {
        new TextBlock(
            new TextSpan("Search: ", Style.Default.WithForeground(Color.Blue)),
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
            return new QueryChange(currentQuery[..^1]);
        }

        return null; // No relevant input to handle
    }
}
