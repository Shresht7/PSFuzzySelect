namespace PSFuzzySelect;

public class FuzzySelector
{
    private readonly IEnumerable<(object obj, string display)> _items;

    private string _searchQuery = string.Empty;

    private FuzzyMatcher _matcher = new();

    public FuzzySelector(IEnumerable<(object obj, string display)> items)
    {
        _items = items;
    }

    public string? Show()
    {
        Console.CursorVisible = false;

        try
        {
            while (true)
            {
                // Clear the console to redraw the UI on each iteration
                Console.Clear();

                // Render the User Interface
                Render();

                // Handle User Input
                (bool exit, string? value) = HandleInput();
                if (exit)
                {
                    return value;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
        }
    }

    /// <summary>
    /// Handles user input for the fuzzy selector, including character input for the search query, backspace for editing, and special keys for selection and exit.
    /// </summary>
    /// <returns>A tuple indicating whether to exit and the selected value, if any.</returns>
    private (bool exit, string? value) HandleInput()
    {
        // Handle User Input
        var key = Console.ReadKey(intercept: true);

        // Exit on Escape key
        if (key.Key == ConsoleKey.Escape)
        {
            return (exit: true, value: null);
        }

        // Check if the user selected an item
        if (key.Key == ConsoleKey.Enter)
        {
            return (exit: true, value: _searchQuery.Length > 0 ? _searchQuery : null);
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

        return (exit: false, value: null);
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
}
