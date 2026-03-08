using System.Management.Automation;
using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Helpers;
using PSFuzzySelect.UI.Layouts;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI;

/// <summary>The main fuzzy selector application component. It manages the application state and user interactions.</summary>
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

    private PreviewPosition _previewPosition = PreviewPosition.Right;

    /// <summary>
    /// Gets the item at the specified match index, or null if the index is out of bounds.
    /// </summary>
    private object? GetMatchItem(int index)
    {
        if (index < 0 || index >= _list.Matches.Count) return null;
        return _list.Matches[index].Item;
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
        string previewSize = "50%",
        PreviewPosition previewPosition = PreviewPosition.Right
    )
    {
        _items = items;
        _displayAdapter = new(properties);
        _multiSelect = multiSelect;
        _showPreview = showPreview;
        _previewSize = GetPreviewSize(previewSize);
        _previewPosition = previewPosition;

        _input = new(prompt, string.Empty);
        _list = new([], multiSelect, GetDisplayString, item => _selectedItems.Contains(item));

        // Initial refresh to populate matches before the first render
        RefreshList();
    }

    #endregion Constructor

    #region Update

    /// <summary>
    /// Updates the state of the fuzzy selector based on the received message.
    /// Each case dispatches to a handler that performs the state change and returns an optional follow-up message.
    /// </summary>
    public Message? Update(Message? message) => message switch
    {
        KeyEvent e => HandleKey(e.Key),
        QueryChange => HandleQueryChange(),
        Select e => HandleSelect(e.Item),
        Confirm => HandleConfirm(),
        UpdatePreview e => HandleUpdatePreview(e.Content),
        _ => null,
    };

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
            var mainLayout = _previewPosition switch
            {
                PreviewPosition.Right => Layout.Horizontal(Size.Flexible(1), _previewSize).Gap(2).Compose(leftPane, rightPane),
                PreviewPosition.Left => Layout.Horizontal(_previewSize, Size.Flexible(1)).Gap(2).Compose(rightPane, leftPane),
                PreviewPosition.Top => Layout.Vertical(_previewSize, Size.Flexible(1)).Gap(1).Compose(rightPane, leftPane),
                PreviewPosition.Bottom => Layout.Vertical(Size.Flexible(1), _previewSize).Gap(1).Compose(leftPane, rightPane),
                _ => throw new NotImplementedException("Unsupported preview position"),
            };
            mainLayout.Render(surface);
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
    private Message? HandleKey(ConsoleKeyInfo key)
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
        if (listMessage != null)
        {
            // Translate HighlightChange into a RequestPreview when preview is active
            if (listMessage is HighlightChange hc && _showPreview)
                return new RequestPreview(GetMatchItem(hc.Index));
            return listMessage;
        }

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
    }

    /// <summary>
    /// Handles a query change by refreshing the match list and optionally requesting a preview for the new top item.
    /// </summary>
    private Message? HandleQueryChange()
    {
        RefreshList();
        return _showPreview ? new RequestPreview(GetMatchItem(0)) : null;
    }

    /// <summary>
    /// Handles item selection. In single-select mode, immediately confirms.
    /// In multi-select mode, toggles the item and optionally requests a preview update.
    /// </summary>
    private Message? HandleSelect(object item)
    {
        if (_multiSelect)
        {
            if (!_selectedItems.Add(item)) _selectedItems.Remove(item); // Toggle selection
            return _showPreview ? new RequestPreview(GetMatchItem(_list.Cursor)) : null;
        }

        // Single-select mode: immediately confirm the selection
        _selectedItems.Clear();
        _selectedItems.Add(item);
        return new Confirm();
    }

    /// <summary>
    /// Handles confirmation. If nothing was explicitly selected, takes the item under the cursor.
    /// </summary>
    private Message? HandleConfirm()
    {
        if (_selectedItems.Count == 0 && _list.Cursor >= 0 && _list.Cursor < _list.Matches.Count)
        {
            _selectedItems.Add(_list.Matches[_list.Cursor].Item);
        }
        return new Quit();
    }

    /// <summary>
    /// Handles an incoming preview content update from the PreviewWorker.
    /// </summary>
    private Message? HandleUpdatePreview(string content)
    {
        _preview.SetContent(content);
        return null;
    }

    #endregion Actions

}

/// <summary>An enumeration representing the possible positions for the preview pane in the fuzzy selector interface</summary>
public enum PreviewPosition { Right, Left, Top, Bottom }

