namespace PSFuzzySelect;

public class FuzzySelector
{
    /// <summary>
    /// The collection of items to be displayed and matched in the fuzzy selector.
    /// Each item is represented as a tuple containing the original object and its corresponding display string.
    /// </summary>
    private readonly IEnumerable<(object obj, string display)> _items;

    /// <summary>The current search query entered by the user</summary>
    private string _searchQuery = string.Empty;

    /// <summary>The fuzzy matcher used to match items against the search query</summary>
    private FuzzyMatcher _matcher = new();

    /// <summary>A flag indicating whether the fuzzy selector should quit</summary>
    private bool shouldQuit = false;

    /// <summary>Initializes a new instance of the FuzzySelector class</summary>
    /// <param name="items">The collection of items to be displayed and matched in the fuzzy selector</param>
    public FuzzySelector(IEnumerable<(object obj, string display)> items)
    {
        _items = items;
    }

    /// <summary>
    /// Shows the fuzzy selector interface to the user, allowing them to enter a search query and view matching items
    /// </summary>
    /// <returns>The selected value, if any.</returns>
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
    /// Handles user input for the fuzzy selector, including character input for the search query,
    /// backspace for editing, and special keys for selection and exit.
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
    /// Renders the user interface for the fuzzy selector,
    /// including the search prompt and the list of items that match the current search query.
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
