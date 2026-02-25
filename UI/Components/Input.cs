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

    public static void Render(string query)
    {
        Console.Write("Search: ");
        Console.Write(query);
    }
}
