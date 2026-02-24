using PSFuzzySelect.Core;

namespace PSFuzzySelect.UI;

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

    /// <summary>
    /// The current list of match results based on the search query.
    /// This list is updated on each render to reflect the items that match the current search query.
    /// </summary>
    private List<MatchResult> _currentMatches = new();

    /// <summary>A cursor index to keep track of the currently selected item in the list of matches</summary>
    private int _cursor = 0;

    /// <summary>
    /// A selected index to keep track of the item
    /// that the user has selected (e.g., by pressing Enter)
    /// </summary>
    private int _selectedIndex = -1;

    /// <summary>A flag indicating whether the fuzzy selector should quit</summary>
    private bool shouldQuit = false;

    /// <summary>Initializes a new instance of the FuzzySelector class</summary>
    /// <param name="items">The collection of items to be displayed and matched in the fuzzy selector</param>
    public FuzzySelector(IEnumerable<(object obj, string display)> items)
    {
        _items = items;
    }

    /// <summary>
    /// Sets up the console UI for the fuzzy selector, including hiding the cursor and preparing any necessary state before entering the main loop.
    /// This method is called once at the beginning of the Show() method to ensure that the console is in the correct state for rendering the fuzzy selector interface.
    /// </summary>
    private void Setup()
    {
        Console.CursorVisible = false;  // Hide the cursor for a cleaner UI experience
    }

    /// <summary>
    /// Shows the fuzzy selector interface to the user, allowing them to enter a search query and view matching items
    /// </summary>
    /// <returns>The selected value, if any.</returns>
    public object? Show()
    {
        Setup();    // Set up the console UI for the fuzzy selector

        // Initial refresh to populate matches before the first render
        RefreshMatches();

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
            Cleanup();  // Clean up the console UI when exiting
        }

        return null;
    }

    /// <summary>
    /// Handles user input for the fuzzy selector, including character input for the search query,
    /// backspace for editing, and special keys for selection and exit.
    /// </summary>
    /// <returns>The selected value, if any.</returns>
    private object? HandleInput()
    {
        // Handle User Input
        var key = Console.ReadKey(intercept: true);

        // Exit on Escape key
        if (key.Key == ConsoleKey.Escape)
        {
            Quit();
            return null;
        }

        // Check if the user selected an item
        if (key.Key == ConsoleKey.Enter)
        {
            if (_currentMatches.Count > 0)
            {
                Select();
                return _currentMatches[_selectedIndex].Item;
            }
            return null; // No matches, so nothing to select
        }

        if (key.Key == ConsoleKey.UpArrow)
        {
            CursorUp();
        }
        if (key.Key == ConsoleKey.DownArrow)
        {
            CursorDown();
        }


        // Handle character input for search query
        if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar))
        {
            _searchQuery += key.KeyChar;
            RefreshMatches();
        }
        else if (key.Key == ConsoleKey.Backspace && _searchQuery.Length > 0)
        {
            _searchQuery = _searchQuery[..^1];
            RefreshMatches();
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

        var visibleMatches = _currentMatches.Take(5).ToList(); // Limit to top 5 matches for now to keep the display manageable

        for (var i = 0; i < visibleMatches.Count; i++)
        {
            var item = visibleMatches[i];
            var cursorIndicator = i == _cursor ? ">" : " ";
            Console.WriteLine($"{cursorIndicator} {item.DisplayString} (Score: {item.Score})");
        }
    }

    /// <summary>
    /// Refreshes the list of matches based on the current search query by invoking the fuzzy matcher against the collection of items.
    /// This method is called whenever the search query is updated to ensure that the displayed matches are always in sync with the user's input.
    /// </summary>
    private void RefreshMatches()
    {
        _currentMatches = _matcher.Match(_items, _searchQuery);
        _cursor = 0;
        _selectedIndex = -1;
    }

    private void CursorUp()
    {
        if (_cursor > 0)
        {
            _cursor--;
        }
    }

    private void CursorDown()
    {
        if (_cursor < _currentMatches.Count - 1)
        {
            _cursor++;
        }
    }

    private void Select()
    {
        if (_cursor >= 0 && _cursor < _currentMatches.Count)
        {
            _selectedIndex = _cursor;
        }
    }

    /// <summary>
    /// Cleans up the console UI by making the cursor visible and clearing any residual UI elements
    /// </summary>
    private void Cleanup()
    {
        Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
        Console.Clear();               // Clear the console to remove any residual UI elements
    }

    /// <summary>
    /// Sets the flag to quit the fuzzy selector, which will cause the main loop to exit and the Show() method to return.
    /// </summary>
    private void Quit()
    {
        shouldQuit = true;
    }
}
