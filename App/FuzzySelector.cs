
using PSFuzzySelect.Core;
using PSFuzzySelect.UI.Layouts;
using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.App.Components;

namespace PSFuzzySelect.App;

/// <summary>The main fuzzy selector application component. It manages the application state and user interactions.</summary>
public class FuzzySelector : IApplication
{
    #region Matcher

    /// <summary>
    /// The complete list of items to be matched against the search query.
    /// This list is populated from the input pipeline and can be updated dynamically as new items arrive.
    /// </summary>
    private readonly List<MatchableItem> _items = [];

    /// <summary>Indicates whether the fuzzy selector is done receiving items from the pipeline.</summary>
    private bool _isStreamingFinished = false;

    /// <summary>The fuzzy matcher used to match items against the search query</summary>
    private readonly FuzzyMatcher _matcher = new();

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

    private const int QueryDebounceMs = 30;
    private bool _queryDirty = false;
    private long _lastQueryChangeMs = 0;

    private static long NowMs() => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

    private RequestPreview? FlushDebouncedQueryIfReady(bool force = false)
    {
        if (!_queryDirty) return null;

        var elapsed = NowMs() - _lastQueryChangeMs;
        if (!force && elapsed < QueryDebounceMs) return null;

        _queryDirty = false;
        RefreshList();
        return _showPreview ? new RequestPreview(GetMatchItem(0)) : null;
    }

    #endregion Components

    #region Preview

    /// <summary>Indicates whether to show a preview of the selected item(s) in the fuzzy selector interface.</summary>
    private readonly bool _showPreview = false;

    private readonly Preview _preview = new();

    private readonly PreviewPosition _previewPosition = PreviewPosition.Right;

    /// <summary>
    /// Gets the item at the specified match index, or null if the index is out of bounds.
    /// </summary>
    private object? GetMatchItem(int index)
    {
        if (index < 0 || index >= _list.Matches.Count) return null;
        return _list.Matches[index].Item;
    }

    /// <summary>
    /// Creates the initial preview request for the currently highlighted item.
    /// Returns null when preview is disabled.
    /// </summary>
    internal Message? CreateInitialPreviewRequest()
    {
        return _showPreview ? new RequestPreview(GetMatchItem(_list.Cursor)) : null;
    }

    private readonly Size _previewSize = Size.Fractional(0.5f);

    /// <summary>
    /// Parses the preview size string into a layout size.
    /// </summary>
    /// <param name="previewSize">Size as percentage (for example, <c>50%</c>) or fixed size (for example, <c>30</c>).</param>
    /// <returns>A <see cref="Size"/> value used by layout composition.</returns>
    /// <exception cref="ArgumentException">Thrown when the input format is invalid.</exception>
    private static Size GetPreviewSize(string previewSize)
    {
        if (previewSize.EndsWith('%') && int.TryParse(previewSize.TrimEnd('%'), out var percentage))
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
        List<MatchableItem>? initialItems = null,
        string[]? properties = null,
        bool multiSelect = false,
        bool showPreview = false,
        string previewSize = "50%",
        PreviewPosition previewPosition = PreviewPosition.Right
    )
    {
        _items = initialItems ?? [];
        _multiSelect = multiSelect;
        _showPreview = showPreview;
        _previewSize = GetPreviewSize(previewSize);
        _previewPosition = previewPosition;

        _input = new(prompt, string.Empty);
        _list = new([], multiSelect, item => _selectedItems.Contains(item));

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
        ItemsAdded e => HandleItemsAdded(e.Items),
        ItemsFinished e => HandleItemsFinished(),
        FrameTick => FlushDebouncedQueryIfReady(),
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
            new StatusBar(_list.Matches.Count, _list.Cursor, !_isStreamingFinished)
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
    private void RefreshList(MatchableItem[]? newItems = null)
    {
        // Determine the list of items that match the current query
        var currentMatches = newItems == null
            ? FuzzyMatcher.Match(_items, _input.Query) // if newItems is null, perform a full match against the entire list
            : FuzzyMatcher.MatchIncremental(_list.Matches, newItems, _input.Query); // otherwise, perform an incremental match on existing matches

        if (ReferenceEquals(currentMatches, _list.Matches)) return; // No change in matches, skip update

        // Update the list component with the new matches, which will trigger a re-render of the match list in the user-interface
        _list.SetMatches(currentMatches, preserveCursor: newItems != null);
    }

    /// <summary>
    /// Handles a query change by refreshing the match list and optionally requesting a preview for the new top item.
    /// </summary>
    private Message? HandleQueryChange()
    {
        _queryDirty = true;
        _lastQueryChangeMs = NowMs();
        return null; // The actual refresh will be triggered by a FrameTick message after the debounce delay
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
    private Quit? HandleConfirm()
    {
        // Ensure pending debounced query is applied before selection
        FlushDebouncedQueryIfReady(force: true);

        if (_selectedItems.Count == 0 && _list.Cursor >= 0 && _list.Cursor < _list.Matches.Count)
        {
            _selectedItems.Add(_list.Matches[_list.Cursor].Item);
        }
        return new Quit();
    }

    /// <summary>
    /// Handles an incoming preview content update from the PreviewWorker.
    /// ANSI CSI sequences are stripped as a stopgap before rendering.
    /// </summary>
    private Message? HandleUpdatePreview(string content)
    {
        _preview.SetContent(Ansi.Strip(content));
        return null;
    }

    private RequestPreview? HandleItemsAdded(MatchableItem[] newItems)
    {
        _items.AddRange(newItems);
        RefreshList(newItems);

        if (_showPreview && _list.Matches.Count > 0)
            return new RequestPreview(GetMatchItem(_list.Cursor));

        return null;
    }

    private Message? HandleItemsFinished()
    {
        _isStreamingFinished = true;
        return null;
    }

    #endregion Actions

}

/// <summary>An enumeration representing the possible positions for the preview pane in the fuzzy selector interface</summary>
public enum PreviewPosition { Right, Left, Top, Bottom }

