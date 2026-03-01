using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Renderer;
using PSFuzzySelect.UI.Layouts;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI;

public class FuzzySelector : IInteractiveComponent
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


    /// <summary>Initializes a new instance of the FuzzySelector class</summary>
    /// <param name="items">The collection of items to be displayed and matched in the fuzzy selector</param>
    public FuzzySelector(IEnumerable<(object obj, string display)> items) => _items = items;

    /// <summary>
    /// Shows the fuzzy selector interface to the user, allowing them to enter a search query and view matching items
    /// </summary>
    /// <returns>The selected value, if any.</returns>
    public object? Show()
    {
        // Instantiate the TUI Engine with the FuzzySelector as the root component
        var engine = new Engine(this);

        // Initial refresh to populate matches before the first render
        RefreshMatches();

        // Run the main loop of the fuzzy selector
        engine.Run();

        // Return the selected item if any, otherwise null
        return _selectedIndex >= 0 ? _currentMatches[_selectedIndex].Item : null;
    }

    /// <summary>
    /// Handles user input for the fuzzy selector, including character input for the search query,
    /// backspace for editing, and special keys for selection and exit.
    /// </summary>
    /// <returns>A Message object representing the user action, which will be processed by the main loop to update the state of the fuzzy selector</returns>
    public Message? HandleKey(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Enter)
        {
            Select();
            return new Quit(); // Exit after selection. At least until we setup multi-select
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
    public Message? Update(Message? message)
    {
        switch (message)
        {
            case QueryChange msg:
                UpdateQuery(msg.Query);
                break;
            case CursorMove msg:
                CursorMove(msg.Delta);
                break;
            case null:
            default:
                // No message to process, do nothing
                break;
        }
        return null;
    }

    /// <summary>
    /// Renders the user interface for the fuzzy selector,
    /// including the search prompt and the list of items that match the current search query.
    /// This method is called on each iteration of the main loop to update the display based on user input.
    /// </summary>
    public void Render(ISurface surface)
    {
        // Create the layout blueprint for the fuzzy selector UI
        var blueprint = Layout.Vertical(
            Size.Fixed(1),          // Search input at the top
            Size.Flexible(1),       // List takes up the remaining space
            Size.Fixed(2)           // Status bar at the bottom
        ).Gap(1);                       // Add a gap between sections

        // Compose the UI components according to the blueprint and render them to the buffer
        blueprint.Compose(
            new Input(_searchQuery),
            new List(_currentMatches, _cursor),
            new StatusBar(_currentMatches.Count, _cursor)
        ).Render(surface);
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


}
