using PSFuzzySelect.UI.Helpers;
using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Components;

public class Input
{
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

    public static void Render(IRenderSurface surface, string query)
    {
        var prompt = "Search: ";
        surface.Write(0, 0, prompt, Style.Default.WithForeground(Ansi.Color.Magenta));
        surface.Write(prompt.Length, 0, query);
    }
}
