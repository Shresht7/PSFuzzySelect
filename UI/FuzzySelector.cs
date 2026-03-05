using System.Management.Automation;
using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Helpers;
using PSFuzzySelect.UI.Layouts;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI;

/// <summary>The main fuzzy selector application component. It manages the application state and user interactions.</summary>
/// <remarks>Initializes a new instance of the FuzzySelector class</remarks>
/// <param name="prompt">The prompt message to display in the fuzzy selector UI.</param>
/// <param name="items">The collection of items to be displayed and matched in the fuzzy selector</param>
/// <param name="properties">An optional array of property names to use for display. If null or empty, the selector will attempt to use the object's default display properties or ToString() method.</param>
public class FuzzySelector : IApplication
{
    #region Input State

    /// <summary>An instance of the Input component that manages the search query input</summary>
    private readonly Input _input;

    #endregion Input State

    #region List State

    /// <summary>An instance of the List component that manages the display and navigation of the list of matches</summary>
    private readonly List _list;

    #endregion List State

    #region Matcher

    /// <summary>The collection of items to be displayed and matched in the fuzzy selector</summary>
    private readonly IEnumerable<object> _items;

    private readonly ObjectDisplayAdapter _displayAdapter;
    private readonly Dictionary<object, string> _displayCache = new();

    /// <summary>The fuzzy matcher used to match items against the search query</summary>
    private readonly FuzzyMatcher _matcher = new();

    /// <summary>
    /// Gets the display string for the specified item, using the display adapter if available.
    /// <param name="item">The item for which to get the display string.</param>
    /// </summary>
    /// <returns>The display string for the item.</returns>
    private string GetDisplayString(object item)
    {
        // Check if the display string for this item is already cached to avoid redundant computations
        if (_displayCache.TryGetValue(item, out var display)) return display;

        // Generate the display string using the adapter. If the item is a PSObject,
        // use the adapter to get the display string; otherwise, fall back to ToString()
        display = item is PSObject psObj
            ? _displayAdapter.GetDisplayString(psObj)
            : item?.ToString() ?? string.Empty;

        if (item != null)
        {
            _displayCache[item] = display;  // Cache the display string for future use
        }
        return display;
    }

    #endregion Matcher

    #region Result

    /// <summary>
    /// A selected index to keep track of the item
    /// that the user has selected (e.g., by pressing Enter)
    /// </summary>
    private int _selectedIndex = -1;

    /// <summary>Gets the currently selected item, or null if no selection has been made.</summary>
    public object? SelectedValue => _selectedIndex >= 0 && _selectedIndex < _list.Matches.Count
        ? _list.Matches[_selectedIndex].Item
        : null;

    #endregion Result

    #region Constructor

    public FuzzySelector(string prompt, IEnumerable<object> items, string[]? properties = null)
    {
        _items = items;
        _displayAdapter = new(properties);
        _matcher = new();

        _input = new(prompt, string.Empty);
        _list = new([], GetDisplayString);
    }

    #endregion Constructor

    #region Show

    /// <summary>
    /// Shows the fuzzy selector UI for the provided collection of items and returns the selected item based on user interaction.
    /// </summary>
    /// <param name="prompt">The prompt message to display in the fuzzy selector UI.</param>
    /// <param name="items">The collection of items to be displayed and matched in the fuzzy selector.</param>
    /// <param name="properties">An optional array of property names to use for display. If null or empty, the selector will attempt to use the object's default display properties or ToString() method.</param>
    /// <returns>The selected item, or null if no selection was made.</returns>
    public static object? Show(string prompt, IEnumerable<object> items, string[]? properties = null)
    {
        var selector = new FuzzySelector(prompt, items, properties);
        var engine = new Engine(selector);

        // Initial refresh to populate matches before the first render
        selector.RefreshList();

        // Run the main loop of the fuzzy selector
        engine.Run();

        return selector.SelectedValue;
    }

    #endregion Show

    #region Update

    /// <summary>
    /// Updates the state of the fuzzy selector based on the received message, which can represent various user actions
    /// such as changing the search query, moving the cursor, selecting an item, or quitting the selector.
    /// </summary>
    /// <param name="message">The message representing a user action.</param>
    public Message? Update(Message? message)
    {
        switch (message)
        {
            case Select:
                SelectItem(_list.Cursor);
                return new Quit(); // Exit after selection. At least until we setup multi-select
            case QueryChange msg:
                UpdateQuery(msg.Query);
                break;
            case KeyEvent keyEvent:
                return HandleKey(keyEvent.Key);
            case null:
            default:
                // No message to process, do nothing
                break;
        }
        return null;
    }

    #endregion Update

    #region Render

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
            Size.Fixed(1)           // Status bar at the bottom
        ).Gap(1);                       // Add a gap between sections

        // Compose the UI components according to the blueprint and render them to the buffer
        blueprint.Compose(
            _input,
            _list,
            new StatusBar(_list.Matches.Count, _list.Cursor)
        ).Render(surface);
    }

    #endregion Render

    #region Key Handlers

    /// <summary>
    /// Handles user input for the fuzzy selector, including character input for the search query,
    /// backspace for editing, and special keys for selection and exit.
    /// </summary>
    /// <returns>A Message object representing the user action, which will be processed by the main loop to update the state of the fuzzy selector</returns>
    public Message? HandleKey(ConsoleKeyInfo key)
    {
        // Handle Quit
        if (key.Key == ConsoleKey.Escape) return new Quit();

        // Handle list navigation and selection keys
        var listMessage = _list.HandleKey(key);
        if (listMessage != null) return listMessage;


        // Handle character input for search query
        var inputMessage = _input.HandleKey(key);
        if (inputMessage != null) return inputMessage;

        return null;
    }

    #endregion Key Handlers

    #region Actions

    /// <summary>
    /// Refreshes the list of matches based on the current search query by invoking the fuzzy matcher against the collection of items.
    /// This method is called whenever the search query is updated to ensure that the displayed matches are always in sync with the user's input.
    /// </summary>
    private void RefreshList()
    {
        var currentMatches = _matcher.Match(_items, _input.Query, GetDisplayString);
        _list.SetMatches(currentMatches);
        _selectedIndex = -1;
    }

    /// <summary>
    /// Updates the search query with the new value entered by the user and refreshes the list of matches accordingly.
    /// </summary>
    /// <param name="newQuery">The updated search query</param>
    private void UpdateQuery(string newQuery)
    {
        RefreshList();
    }


    /// <summary>
    /// Selects the currently highlighted item in the list of matches based on the cursor position and updates the selected index accordingly.
    /// </summary>
    /// <param name="index">The index of the item to select.</param>
    private void SelectItem(int index)
    {
        if (index >= 0 && index < _list.Matches.Count)
        {
            _selectedIndex = index;
        }
    }

    #endregion Actions
}
