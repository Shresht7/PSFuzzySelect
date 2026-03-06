using System.Management.Automation;
using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Helpers;
using PSFuzzySelect.UI.Layouts;
using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI;

/// <summary>The main fuzzy selector application component. It manages the application state and user interactions.</summary>
/// <remarks>Initializes a new instance of the FuzzySelector class</remarks>
/// <param name="prompt">The prompt message to display in the fuzzy selector UI.</param>
/// <param name="items">The collection of items to be displayed and matched in the fuzzy selector</param>
/// <param name="properties">An optional array of property names to use for display. If null or empty, the selector will attempt to use the object's default display properties or ToString() method.</param>
public class FuzzySelector : IApplication
{
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

    private readonly bool _multiSelect = false;

    private readonly HashSet<object> _selectedItems = new();

    public object? SelectedValue => _multiSelect ? _selectedItems.ToArray() : _selectedItems.FirstOrDefault();

    #endregion Result

    #region Components

    /// <summary>An instance of the Input component that manages the search query input</summary>
    private readonly Input _input;

    /// <summary>An instance of the List component that manages the display and navigation of the list of matches</summary>
    private readonly List _list;

    #endregion Components

    #region Preview

    /// <summary>Indicates whether to show a preview of the selected item(s) in the fuzzy selector interface.</summary>
    private bool _showPreview = false;

    private Preview _preview = new();

    private void UpdatePreview(int index)
    {
        if (index < 0 || index >= _list.Matches.Count) return; // Invalid index, do nothing

        var selectedItem = _list.Matches[index].Item;
        _preview.Clear();

        if (selectedItem is PSObject psObj)
        {
            foreach (var prop in psObj.Properties)
            {
                string valString;
                try
                {
                    // Need to do this because some properties throw when accessed.
                    valString = prop.Value?.ToString() ?? "null";
                }
                catch
                {
                    valString = "Error retrieving value";
                }
                _preview.AddLine(new TextBlock()
                    .Add(new TextSpan($"{prop.Name}: ", Style.Default.WithForeground(Color.Yellow)))
                    .Add(new TextSpan(valString, Style.Default.WithForeground(Color.White)))
                );
            }
        }
    }

    private Size _previewSize = Size.Fractional(0.5f);

    private Size GetPreviewSize(string previewSize)
    {
        if (previewSize.EndsWith("%") && int.TryParse(previewSize.TrimEnd('%'), out var percentage))
        {
            return Size.Fractional(percentage / 100.0f);
        }
        else if (int.TryParse(previewSize, out var fixedWidth))
        {
            return Size.Fixed(fixedWidth);
        }
        else
        {
            throw new ArgumentException("Invalid preview size format. Use a percentage (e.g., '50%') or a fixed width (e.g., '30').");
        }
    }

    #endregion Preview

    #region Constructor

    public FuzzySelector(
        string prompt,
        IEnumerable<object> items,
        string[]? properties = null,
        bool multiSelect = false,
        bool showPreview = false,
        string previewSize = "50%"
    )
    {
        _items = items;
        _displayAdapter = new(properties);
        _multiSelect = multiSelect;
        _showPreview = showPreview;
        _previewSize = GetPreviewSize(previewSize);

        _input = new(prompt, string.Empty);
        _list = new([], multiSelect, GetDisplayString, item => _selectedItems.Contains(item));
    }

    #endregion Constructor

    #region Show

    /// <summary>
    /// Shows the fuzzy selector UI for the provided collection of items and returns the selected item based on user interaction.
    /// </summary>
    /// <param name="prompt">The prompt message to display in the fuzzy selector UI.</param>
    /// <param name="items">The collection of items to be displayed and matched in the fuzzy selector.</param>
    /// <param name="properties">An optional array of property names to use for display. If null or empty, the selector will attempt to use the object's default display properties or ToString() method.</param>
    /// <param name="multiSelect">Indicates whether multiple items can be selected.</param>
    /// <param name="showPreview">Indicates whether to show a preview of the selected item(s).</param>
    /// <param name="previewSize">The size of the preview pane in the fuzzy selector interface, specified as a percentage (e.g., "50%") or fixed width (e.g., "30").</param>
    /// <returns>The selected item, or null if no selection was made.</returns>
    public static object? Show(
        string prompt,
        IEnumerable<object> items,
        string[]? properties = null,
        bool multiSelect = false,
        bool showPreview = false,
        string previewSize = "50%"
    )
    {
        // Initialize the fuzzy selector application with the provided parameters
        var selector = new FuzzySelector(prompt, items, properties, multiSelect, showPreview, previewSize);
        var engine = new Engine(selector);

        // Initial refresh to populate matches before the first render
        selector.RefreshList();

        // Run the main loop of the fuzzy selector
        engine.Run();

        // Return the selected value after the user has made a selection
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
            case Confirm:
                return ConfirmSelection();
            case Select msg:
                return SelectItem(msg.Item);
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

        // Compose the UI components according to the blueprint
        var leftPane = blueprint.Compose(
            _input,
            _list,
            new StatusBar(_list.Matches.Count, _list.Cursor)
        );

        var rightPane = _preview;

        // If preview is enabled, render the left and right panes side by side; otherwise, render only the left pane
        if (_showPreview)
        {
            var mainLayout = Layout.Horizontal(
                Size.Flexible(1),   // Left pane takes up available space
                _previewSize        // Right pane (preview) takes up specified size
            ).Gap(2);              // Add a gap between panes

            mainLayout.Compose(leftPane, rightPane).Render(surface);
        }
        else
        {
            leftPane.Render(surface);
        }
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
        if (key.Key == ConsoleKey.Escape)
        {
            _selectedItems.Clear(); // Clear any active selections due to cancellation
            return new Quit();
        }

        // Handle Confirmation
        if (key.Key == ConsoleKey.Enter)
        {
            return new Confirm();
        }

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
        UpdatePreview(0);
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
    private Message? SelectItem(object item)
    {
        UpdatePreview(_list.Cursor); // Update the preview to show details of the currently selected item
        if (_multiSelect)
        {
            if (!_selectedItems.Add(item)) _selectedItems.Remove(item); // Toggle selection if already selected
        }
        else
        {
            // Single-select mode: immediately confirm the selection
            _selectedItems.Clear();
            _selectedItems.Add(item);
            return new Confirm();
        }
        return null;
    }

    private Message? ConfirmSelection()
    {
        // If nothing is selected via Tab, we take the one under the cursor
        if (_selectedItems.Count == 0 && _list.Cursor >= 0 && _list.Cursor < _list.Matches.Count)
        {
            _selectedItems.Add(_list.Matches[_list.Cursor].Item);
        }
        return new Quit();
    }

    #endregion Actions
}
