using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Renderer;

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
    private bool _shouldQuit = false;

    private readonly ConsoleRenderer _renderer;

    /// <summary>Initializes a new instance of the FuzzySelector class</summary>
    /// <param name="items">The collection of items to be displayed and matched in the fuzzy selector</param>
    public FuzzySelector(IEnumerable<(object obj, string display)> items)
    {
        _items = items;
        _renderer = new ConsoleRenderer(Console.WindowWidth, Console.WindowHeight);
    }

    /// <summary>
    /// Sets up the console UI for the fuzzy selector, including hiding the cursor and preparing any necessary state before entering the main loop.
    /// This method is called once at the beginning of the Show() method to ensure that the console is in the correct state for rendering the fuzzy selector interface.
    /// </summary>
    private void Setup()
    {
        Console.CursorVisible = false;  // Hide the cursor for a cleaner UI experience
        Console.Clear();                // Clear the console to prepare for first-paint
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
            while (!_shouldQuit)
            {
                // Render the User Interface
                Render();

                // Handle User Input
                var msg = HandleInput();

                // Perform the update based on the received messages
                Update(msg);
            }
        }
        finally
        {
            Cleanup();  // Clean up the console UI when exiting
        }

        // Return the selected item if any, otherwise null
        return _selectedIndex >= 0 ? _currentMatches[_selectedIndex].Item : null;
    }

    /// <summary>
    /// Handles user input for the fuzzy selector, including character input for the search query,
    /// backspace for editing, and special keys for selection and exit.
    /// </summary>
    /// <returns>A Message object representing the user action, which will be processed by the main loop to update the state of the fuzzy selector</returns>
    private Message? HandleInput()
    {
        // Handle User Input
        var key = Console.ReadKey(intercept: true);

        // Exit on Escape key
        if (key.Key == ConsoleKey.Escape)
        {
            return new Quit();
        }

        // Handle list navigation and selection keys
        var listMessage = List.HandleKey(key);
        if (listMessage != null)
        {
            return listMessage;
        }

        // Handle character input for search query
        var inputMessage = Input.HandleKey(key, _searchQuery);
        if (inputMessage != null)
        {
            return inputMessage;
        }

        return null;
    }

    /// <summary>
    /// Updates the state of the fuzzy selector based on the received message, which can represent various user actions
    /// such as changing the search query, moving the cursor, selecting an item, or quitting the selector.
    /// </summary>
    /// <param name="message">The message representing a user action.</param>
    private void Update(Message? message)
    {
        switch (message)
        {
            case QueryChange msg:
                UpdateQuery(msg.Query);
                break;
            case CursorMove msg:
                CursorMove(msg.Delta);
                break;
            case Select msg:
                Select();
                Quit(); // Exit after selection. At least until we setup multi-select
                break;
            case Quit msg:
                Quit();
                break;
            case null:
            default:
                // No message to process, do nothing
                break;
        }
    }

    /// <summary>
    /// Renders the user interface for the fuzzy selector,
    /// including the search prompt and the list of items that match the current search query.
    /// This method is called on each iteration of the main loop to update the display based on user input.
    /// </summary>
    private void Render()
    {
        var buffer = _renderer.CreateBuffer();
        buffer.Write(0, 0, "Search: " + _searchQuery);   // Render the search prompt with the current query

        // TODO: Need a layout engine to manage positioning of components instead of hardcoding values

        Input.Render(buffer, 0, 0, _searchQuery);
        List.Render(buffer, 0, 2, _currentMatches, _cursor);
        StatusBar.Render(buffer, 0, 8, _currentMatches.Count, _cursor);

        _renderer.Render(buffer);    // Render the current buffer to the console

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

    private void UpdateQuery(string newQuery)
    {
        _searchQuery = newQuery;
        RefreshMatches();
    }

    private void CursorMove(int delta)
    {
        if (_currentMatches.Count == 0)
        {
            _cursor = -1;
            return;
        }
        _cursor = Math.Clamp(_cursor + delta, 0, _currentMatches.Count - 1);
    }

    private void Select()
    {
        if (_cursor >= 0 && _cursor < _currentMatches.Count)
        {
            _selectedIndex = _cursor;
        }
    }

    /// <summary>
    /// Cleans up the console UI by making the cursor visible, resetting the console colors, and clearing any residual UI elements
    /// </summary>
    private void Cleanup()
    {
        Console.ResetColor();           // Reset console colors to default
        Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
        Console.Clear();                // Clear the console to remove any residual UI elements
    }

    /// <summary>
    /// Sets the flag to quit the fuzzy selector, which will cause the main loop to exit and the Show() method to return.
    /// </summary>
    private void Quit()
    {
        _shouldQuit = true;
    }
}
