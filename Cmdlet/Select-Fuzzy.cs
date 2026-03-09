using System.Management.Automation;

using PSFuzzySelect.UI;

namespace PSFuzzySelect.Cmdlet;

/// <summary>
/// A PowerShell cmdlet that provides a fuzzy selection interface for selecting items from a list of input objects.
/// </summary>
[Cmdlet(VerbsCommon.Select, "Fuzzy")]
[OutputType(typeof(PSObject))]
public class SelectFuzzyCmdlet : PSCmdlet
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
    /// If set to <see langword="true"/>, users can select multiple items; otherwise, only a single item can be selected.
    /// </summary>
    [Parameter]
    public SwitchParameter MultiSelect { get; set; }

    /// <summary>
    /// Properties to display and search on.
    /// If not specified, uses the object's default display properties or `ToString()` representation.
    /// </summary>
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty]
    public string[]? Property { get; set; }

    /// <summary>
    /// The prompt message to display in the fuzzy selector user-interface.
    /// </summary>
    [Parameter]
    public string Prompt { get; set; } = "Search:";

    /// <summary>
    /// Indicates whether to show a preview of the selected item(s) in the fuzzy selector interface.
    /// </summary>
    [Parameter]
    public SwitchParameter Preview { get; set; }

    /// <summary>
    /// The size of the preview pane in the fuzzy selector interface, specified as a percentage (e.g., "50%") or fixed width (e.g., "30").
    /// </summary>
    [Parameter]
    public string PreviewSize { get; set; } = "50%";

    /// <summary>
    /// The position of the preview pane in the fuzzy selector interface (e.g., left, right, top, bottom).
    /// </summary>
    [Parameter]
    public PreviewPosition PreviewPosition { get; set; } = PreviewPosition.Right;


    /// <summary>
    /// A script block used to generate the preview content for each item in the fuzzy selector interface.
    /// The current item is provided as <c>$PSItem</c> / <c>$_</c>, and the script output is displayed in the preview pane.
    /// If omitted while <see cref="Preview"/> is enabled, a default formatter is used.
    /// </summary>
    [Parameter]
    public ScriptBlock? PreviewScript { get; set; }

    #endregion Parameters

    #region Fields

    private FuzzySelector? _selector;

    private Engine? _engine;

    private Thread? _uiThread;

    /// <summary>An exception that may occur on the UI thread. This is captured to allow for proper error handling and reporting back to the PowerShell pipeline.</summary>
    private Exception? _uiThreadException;

    /// <summary>
    ///  Runs the fuzzy selector user-interface on a separate thread to avoid blocking the PowerShell pipeline.
    /// </summary>
    /// <remarks>
    /// This method captures any exceptions that occur on the UI thread and stores them in the <see cref="_uiThreadException"/> field for later handling.
    /// </remarks>
    private void RunUIThread()
    {
        try { _engine!.Run(); }
        catch (Exception ex) { _uiThreadException = ex; }
    }

    /// <summary>A buffer for storing input objects before they are sent to the fuzzy selector user-interface</summary>
    private readonly List<object> _inputBuffer = [];

    /// <summary>The batch size for processing input objects. This determines how many items are buffered before being sent to the UI for display</summary>
    private readonly int _batchSize = 64;

    private void FlushInputBuffer()
    {
        if (_inputBuffer.Count == 0) return;

        // Send the buffered items to the UI for display
        _engine!.EnqueueMessage(new ItemsAdded(_inputBuffer.ToArray()));

        // Clear the buffer after flushing
        _inputBuffer.Clear();
    }

    #endregion Fields

    #region Begin

    protected override void BeginProcessing()
    {
        _selector = new FuzzySelector(
            Prompt,
            null,
            Property,
            MultiSelect.IsPresent,
            Preview.IsPresent,
            PreviewSize,
            PreviewPosition
        );

        _engine = new Engine(_selector);
        if (Preview.IsPresent) _engine.EnablePreview(PreviewScript);

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
        if (InputObject == null) return;
        if (_engine == null) throw new InvalidOperationException("Engine not initialized!");

        // Buffer incoming items until we reach the batch size...
        _inputBuffer.Add(InputObject);
        // ...then flush the buffer to the UI to update the displayed items
        if (_inputBuffer.Count >= _batchSize) FlushInputBuffer();
    }

    #endregion Process

    #region End

    /// <summary>Called once after all input has been processed</summary>
    protected override void EndProcessing()
    {
        if (_engine == null || _selector == null || _uiThread == null)
            throw new InvalidOperationException("Failed to initialize correctly!");

        try
        {
            // Flush any remaining items in the buffer
            FlushInputBuffer();

            // Signal the UI that no more items will be added, allowing it to update its state accordingly
            _engine.EnqueueMessage(new ItemsFinished());

            // Wait for the UI Thread to finish (i.e., the user has made a selection and closed the UI)
            _uiThread.Join();

            // If an exception occurred on the UI thread, rethrow it here to ensure it is properly reported in the PowerShell pipeline
            if (_uiThreadException != null) throw new InvalidOperationException("UI Error", _uiThreadException);

            // Retrieve the selected item(s) from the engine after the UI has closed
            var selected = _selector.SelectedValue;

            // Write the selected object (or array of objects) to the pipeline if a selection was made
            if (selected != null)
            {
                WriteObject(selected, enumerateCollection: true);
            }
        }
        finally
        {
            _engine.Dispose();
        }
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
        _engine?.Dispose();
    }

    #endregion Stop
}
