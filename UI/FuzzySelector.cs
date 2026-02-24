namespace PSFuzzySelect;

public class FuzzySelector
{
    private readonly IEnumerable<(object obj, string display)> _items;

    private string _searchQuery = string.Empty;

    private FuzzyMatcher _matcher = new();

    private bool shouldQuit = false;

    public FuzzySelector(IEnumerable<(object obj, string display)> items)
    {
        _items = items;
    }

    public string? Show()
    {
        Console.CursorVisible = false;

        try
        {
            while (!shouldQuit)
            {
                // Clear the console to redraw the UI on each iteration
                Console.Clear();

                // Render the User Interface
                Render();

                // Handle User Input
                var value = HandleInput();
                if (value != null)
                {
                    return value;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
        }

        return null;
    }

    /// <summary>
    /// Handles user input for the fuzzy selector, including character input for the search query, backspace for editing, and special keys for selection and exit.
    /// </summary>
    /// <returns>The selected value, if any.</returns>
    private string? HandleInput()
    {
        // Handle User Input
        var key = Console.ReadKey(intercept: true);

        // Exit on Escape key
        if (key.Key == ConsoleKey.Escape)
        {
            return Quit();
        }

        // Check if the user selected an item
        if (key.Key == ConsoleKey.Enter)
        {
            return _searchQuery.Length > 0 ? _searchQuery : null;
        }

        // Handle character input for search query
        if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar))
        {
            _searchQuery += key.KeyChar;
        }
        else if (key.Key == ConsoleKey.Backspace && _searchQuery.Length > 0)
        {
            _searchQuery = _searchQuery[..^1];
        }

        return null;
    }

    /// <summary>
    /// Renders the user interface for the fuzzy selector, including the search prompt and the list of items that match the current search query.
    /// This method is called on each iteration of the main loop to update the display based on user input.
    /// </summary>
    private void Render()
    {
        var prompt = "> ";
        Console.Write(prompt + _searchQuery);
        Console.WriteLine();

        var matches = _matcher.Match(_items, _searchQuery);

        // For now, just list all the items.
        foreach (var item in matches.Take(5)) // Limit to top 5 items for display for now to keep it manageable
        {
            Console.WriteLine($"  -  {item.DisplayString} (Score: {item.Score})");
        }
    }

    /// <summary>
    /// Sets the flag to quit the fuzzy selector, which will cause the main loop to exit and the Show() method to return.
    /// </summary>
    private string? Quit()
    {
        shouldQuit = true;
        return null;
    }
}
