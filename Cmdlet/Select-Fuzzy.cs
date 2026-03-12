using System.Diagnostics;
using System.Management.Automation;

using PSFuzzySelect.App;
using PSFuzzySelect.App.Helpers;

namespace PSFuzzySelect.Cmdlet;

/// <summary>
/// <para type="synopsis">Provides an interactive fuzzy selection interface for PowerShell pipeline objects</para>
/// <para type="description">
/// `Select-Fuzzy` presents a full-screen terminal-user-interface for fuzzy-filtering and selecting objects
/// from the pipeline. Items appear in the user-interface as the upstream command produces them (streaming).
/// The selected objects are written to the output pipeline after the user confirms.
/// 
/// Navigation:
///     Up/Down Arrow     Move the cursor
///     Ctrl+Up/Down      Jump to top/bottom
///     Tab / Shift+Tab   Move the cursor (multi-select: toggle and move)
/// 
/// Selection:
///     Enter             Confirm selection and exit
///     Esc               Cancel and exit (no output)
/// 
/// Search:
///     Type characters   Filter the list
///     Backspace         Delete character before cursor
///     Delete            Delete character at cursor
///     Home/End          Move cursor to start/end of search query
///     Left/Right Arrow  Move cursor in search query (Ctrl for word-jump)
/// </para>
/// </summary>
/// <example>
///     <code>Get-ChildItem -File | Select-Fuzzy</code>
///     <para>Interactively select a file from the current directory.</para>
/// </example>
/// <example>
///     <code>Get-ChildItem -File -Recurse | Select-Fuzzy -Property Name</code>
///     <para>Pick a file by its name from a recursive list.</para>
/// </example>
/// <example>
///     <code>Get-Process | Select-Fuzzy -MultiSelect | Stop-Process</code>
///     <para>Select multiple processes and stop them.</para>
/// </example>
/// <example>
///     <code>Get-ChildItem -File -Recurse | Select-Fuzzy -ShowPreview -PreviewScript { Get-Content $_.FullName }</code>
///     <para>Select multiple files with a live content preview in a side pane.</para>
/// </example>
[Cmdlet(VerbsCommon.Select, "Fuzzy")]
[Alias("psfzf")]
[OutputType(typeof(PSObject))]
public sealed class SelectFuzzyCmdlet : PSCmdlet
{

    #region Parameters

    /// <summary>
    /// The input object(s) to be processed by the cmdlet.
    /// This parameter accepts input from the pipeline, allowing users to pipe objects directly into the cmdlet for fuzzy selection.
    /// </summary>
    [Parameter(ValueFromPipeline = true)]
    public PSObject? InputObject { get; set; }

    /// <summary>
    /// Indicates whether multiple items can be selected in the fuzzy selector.
    /// When enabled, use Tab or Shift+Tab to toggle selection on items.
    /// </summary>
    [Parameter]
    public SwitchParameter MultiSelect { get; set; }

    /// <summary>
    /// The properties of the input objects to display and search against.
    /// If not specified, the cmdlet uses the object's default display properties or its <c>ToString()</c> representation.
    /// </summary>
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty]
    public string[]? Property { get; set; }

    /// <summary>
    /// The prompt message to display next to the search input field (default: "Search:").
    /// </summary>
    [Parameter]
    public string Prompt { get; set; } = "Search:";

    /// <summary>
    /// The initial search query to populate the search input field when the fuzzy selector UI is launched.
    /// </summary>
    [Parameter]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether to show the preview pane. This is implicitly enabled if <see cref="PreviewScript"/> is provided.
    /// </summary>
    [Parameter]
    [Alias("Details")]
    public SwitchParameter ShowPreview { get; set; }

    /// <summary>
    /// The size of the preview pane, specified as a percentage (e.g., "50%") or a fixed number of columns/rows (e.g., "30").
    /// Defaults to "50%".
    /// </summary>
    [Parameter]
    public string PreviewSize { get; set; } = "50%";

    /// <summary>
    /// The position of the preview pane relative to the main list.
    /// Valid values: Right, Left, Top, Bottom. Defaults to Right.
    /// </summary>
    [Parameter]
    public PreviewPosition PreviewPosition { get; set; } = PreviewPosition.Right;


    /// <summary>
    /// A script block used to generate the preview content for the currently highlighted item.
    /// The current item is available as <c>$_</c> or <c>$PSItem</c>. The output of the script is rendered as text in the preview pane.
    /// </summary>
    [Parameter]
    [Alias("Preview", "PreviewScriptBlock")]
    public ScriptBlock? PreviewScript { get; set; }

    #endregion Parameters

    #region Fields

    /// <summary>The fuzzy selector instance that manages the user-interface and selection logic</summary>
    private FuzzySelector? _selector;

    /// <summary>The engine that runs the fuzzy selector user-interface and handles communication between the UI thread and the PowerShell pipeline</summary>
    private Engine? _engine;

    /// <summary>The thread on which the fuzzy selector user-interface is running</summary>
    private Thread? _uiThread;

    /// <summary>An exception that may occur on the UI thread. This is captured to allow for proper error handling and reporting back to the PowerShell pipeline</summary>
    private Exception? _uiThreadException;

    /// <summary>
    ///  Runs the fuzzy selector user-interface on a separate thread to avoid blocking the PowerShell pipeline
    /// </summary>
    /// <remarks>
    /// This method captures any exceptions that occur on the UI thread and stores them in the <see cref="_uiThreadException"/> field for later handling
    /// </remarks>
    private void RunUIThread()
    {
        try { _engine!.Run(); } // Run the fuzzy selector UI. This call blocks until the user has made a selection and closed the UI.
        catch (Exception ex) { _uiThreadException = ex; }   // Capture any exceptions that occur on the UI thread
    }

    /// <summary>The batch size for processing input objects. This determines how many items are buffered before being sent to the UI for display</summary>
    private const int _batchSize = 64;

    /// <summary>A buffer for storing input objects before they are sent to the fuzzy selector user-interface</summary>
    private readonly MatchableItem[] _inputBuffer = new MatchableItem[_batchSize];

    private int _inputBufferIndex = 0; // The current index in the input buffer, tracking how many items have been buffered

    /// <summary>
    /// The interval in milliseconds for flushing the input buffer to the UI.
    /// This is used to control how frequently the buffered items are dispatched if the batch size is not reached,
    /// ensuring that the UI remains responsive even with a continuous stream of input objects.
    /// </summary>
    private readonly int _flushIntervalMs = 100;

    /// <summary>
    /// A stopwatch to track the time since the last flush of the input buffer.
    /// This is used in conjunction with the <see cref="_flushIntervalMs"/> to determine
    /// when to automatically flush the buffer to the fuzzy-selector user-interface,
    /// ensuring that items are displayed in a timely manner even if the batch size is not reached.
    /// </summary>
    private readonly Stopwatch _streamStopwatch = Stopwatch.StartNew();

    /// <summary>Flushes the input buffer to the fuzzy selector user-interface.</summary>
    private void FlushInputBuffer()
    {
        // If there are no items in the buffer, there is nothing to flush
        if (_inputBufferIndex == 0) return;

        // Create a copy of the filled portion of the buffer to send to the UI
        var itemsToSend = new MatchableItem[_inputBufferIndex];
        Array.Copy(_inputBuffer, itemsToSend, _inputBufferIndex);

        // Send the buffered items to the UI for display
        _engine!.EnqueueMessage(new ItemsAdded(itemsToSend));

        // Clear the used portion of the buffer after flushing
        Array.Clear(_inputBuffer, 0, _inputBufferIndex);
        _inputBufferIndex = 0;

        // Restart the stopwatch to track the time until the next flush
        _streamStopwatch.Restart();
    }

    private ObjectDisplayAdapter? _displayAdapter;

    /// <summary>A counter to track the total number of input objects processed</summary>
    private int _processedCount = 0;

    #endregion Fields

    #region Begin

    protected override void BeginProcessing()
    {
        // Determine whether to show the preview pane based on the presence of the ShowPreview switch or a provided PreviewScript.
        // If either is present, the preview pane will be enabled in the fuzzy selector UI.
        bool showPreview = ShowPreview.IsPresent || PreviewScript != null;

        // Initialize the fuzzy selector with the provided parameters
        _selector = new FuzzySelector(
            Prompt,
            Query,
            null,
            Property,
            MultiSelect.IsPresent,
            showPreview,
            PreviewSize,
            PreviewPosition
        );

        // Initialize the display adapter that will convert input objects into display strings for the UI based on the specified properties
        _displayAdapter = new ObjectDisplayAdapter(Property);

        // Initialize the engine that will run the fuzzy selector UI and handle communication between the UI thread and the PowerShell pipeline
        _engine = new Engine(_selector);
        if (showPreview) _engine.EnablePreview(PreviewScript);

        // Start the User-Interface on a separate thread so as not to block the PowerShell pipeline
        // The PSObjects will be streamed into the UI by dispatching a ItemsAdded Message.
        _uiThread = new Thread(RunUIThread) { IsBackground = false };
        _uiThread.Start();
    }

    #endregion Begin

    #region Process

    /// <summary>
    /// Processes each record received from the pipeline.
    /// This method is called for each input object.
    /// </summary>
    protected override void ProcessRecord()
    {
        // If the UI has already finished (e.g. user made a selection or cancelled), 
        // stop the pipeline immediately to avoid unnecessary processing of remaining items.
        if (_engine is { IsRunning: false })
        {
            EndProcessing();    // Ensure EndProcessing is called to perform any necessary cleanup and finalization
            throw new PipelineStoppedException();   // Throwing this exception signals to PowerShell that the pipeline has been stopped, which is the appropriate way to halt processing in this context
        }

        if (InputObject != null) EnqueueInput(InputObject); // Enqueue the input object for processing and display in the fuzzy selector UI
    }

    private void EnqueueInput(PSObject input)
    {
        _processedCount++;          // Increment the count of processed input objects

        // Buffer incoming items
        var display = _displayAdapter?.GetDisplayString(input);
        _inputBuffer[_inputBufferIndex++] = new MatchableItem(input, display ?? string.Empty);

        // Flush the buffer if we've reached the batch size or if the flush interval has elapsed to ensure timely updates to the UI
        if (_inputBufferIndex >= _batchSize || _streamStopwatch.ElapsedMilliseconds >= _flushIntervalMs)
        {
            FlushInputBuffer();
        }
    }

    #endregion Process

    #region End

    /// <summary>
    /// A flag to ensure that the <see cref="EndProcessing"/> logic is only executed once.
    /// This is necessary because <see cref="EndProcessing"/> can be called manually from <see cref="ProcessRecord"/> 
    /// (to stop the pipeline early) and will still be called by PowerShell's infrastructure during the normal cleanup cycle.
    /// </summary>
    private bool _isEndProcessingCalled = false;

    /// <summary>
    /// Finalizes the processing of the cmdlet. This method is called by the PowerShell engine after all records 
    /// have been processed or if the pipeline is stopped early.
    /// </summary>
    protected override void EndProcessing()
    {
        // Ensure that the teardown and output logic only runs once, regardless of whether it was triggered
        // by the end of the pipeline or an early selection in ProcessRecord.
        if (_isEndProcessingCalled) return;
        _isEndProcessingCalled = true;

        if (_engine == null || _selector == null || _uiThread == null)
            throw new InvalidOperationException("Failed to initialize correctly!");

        try
        {
            // If no input was processed, it means this was a standalone call, default to Get-ChildItem in the current directory
            if (ShouldUseFallbackScript())
            {
                var fallbackResults = InvokeCommand.InvokeScript("Get-ChildItem");
                foreach (var item in fallbackResults) EnqueueInput(item);
            }

            // Flush any remaining items in the buffer
            FlushInputBuffer();

            // Signal the UI that no more items will be added, allowing it to update its state accordingly
            if (_engine.IsRunning) _engine.EnqueueMessage(new ItemsFinished());

            // Wait for the UI Thread to finish (i.e., the user has made a selection and closed the UI)
            if (_uiThread.IsAlive) _uiThread.Join();

            // If an exception occurred on the UI thread, rethrow it here to ensure it is properly reported in the PowerShell pipeline
            if (_uiThreadException != null) throw new InvalidOperationException("UI Error", _uiThreadException);

            // Retrieve the selected item(s) from the engine after the UI has closed
            var selected = _selector.SelectedValue;

            // Write the selected object (or array of objects) to the pipeline if a selection was made
            if (selected != null) WriteObject(selected, enumerateCollection: true);
        }
        finally
        {
            _engine.Dispose();
        }
    }

    private bool ShouldUseFallbackScript()
    {
        return _processedCount == 0 && !MyInvocation.ExpectingInput && !MyInvocation.BoundParameters.ContainsKey(nameof(InputObject));
    }

    #endregion End

    #region Stop

    protected override void StopProcessing()
    {
        // If the cmdlet is stopped (e.g., by the user), signal the UI to quit immediately
        _engine?.EnqueueMessage(new Quit());

        // Await the UI thread to finish to ensure a clean shutdown. (5 second timeout to prevent hanging indefinitely if the UI fails to close properly)
        if (_uiThread != null && _uiThread.IsAlive) _uiThread.Join(5000);

        // Dispose off the engine if not done already
        if (_uiThread == null || !_uiThread.IsAlive) _engine?.Dispose();
    }

    #endregion Stop
}
