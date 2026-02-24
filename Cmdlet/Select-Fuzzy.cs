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
        var displayAdapter = new ObjectDisplayAdapter(Property);
        var previews = _inputObjects.Select(obj => displayAdapter.GetDisplayString(obj));

        WriteObject($"Received {_inputObjects.Count} objects");
        foreach (var preview in previews)
        {
            WriteObject($"  -  {preview}");
        }
    }
}
