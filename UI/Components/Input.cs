using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Components;

public class Input(string query) : IComponent
{
    public void Render(ISurface surface)
    {
        var prompt = "Search: ";
        surface.Write(0, 0, prompt, Style.Default.WithForeground(Ansi.Color.Magenta));
        surface.Write(prompt.Length, 0, query);
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
