using System.Management.Automation;

namespace PSFuzzySelect;

[Cmdlet(VerbsCommon.Select, "Fuzzy")]
[OutputType(typeof(PSObject))]
public class SelectFuzzyCmdlet : PSCmdlet
{

    /// <summary>
    /// The input object(s) to be processed by the cmdlet.
    /// This parameter accepts input from the pipeline, allowing users to pipe objects directly into the cmdlet for fuzzy selection.
    /// </summary>
    [Parameter(ValueFromPipeline = true)]
    public PSObject? InputObject { get; set; }

    /// <summary>
    /// Properties to display and search on.
    /// If not specified, uses the object's default display properties or ToString() representation.
    /// </summary>
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty]
    public string[]? Property { get; set; }

    /// <summary>A list to hold all input objects received from the pipeline</summary>
    private readonly List<PSObject> _inputObjects = new();

    /// <summary>
    /// Processes each record received from the pipeline.
    /// This method is called for each input object.
    /// </summary>
    protected override void ProcessRecord()
    {
        // Collect input objects into a list for processing
        if (InputObject != null)
        {
            _inputObjects.Add(InputObject);
        }
    }

    /// <summary>
    /// Called once after all input has been processed.
    /// </summary>
    protected override void EndProcessing()
    {
        var matcher = new FuzzyMatcher();

        var displayAdapter = new ObjectDisplayAdapter(Property);
        var items = _inputObjects.Select(obj => ((object)obj, displayAdapter.GetDisplayString(obj)));

        // Prompt the user for a search query
        var prompt = "> ";
        Host.UI.Write(prompt);
        var query = Host.UI.ReadLine();

        // Perform the fuzzy matching against the collected input objects based on the user's query
        var matches = matcher.Match(items, query);

        // Display the matched objects
        foreach (var match in matches)
        {
            WriteObject($"  -  {match.DisplayString} (Score: {match.Score})");
        }
    }
}
