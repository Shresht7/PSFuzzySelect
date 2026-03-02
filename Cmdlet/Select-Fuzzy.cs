using System.Management.Automation;

using PSFuzzySelect.Helpers;
using PSFuzzySelect.UI;

namespace PSFuzzySelect.Cmdlet;

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
        // Generate display strings for each input object based on the specified properties or default display logic
        var displayAdapter = new ObjectDisplayAdapter(Property);
        var items = _inputObjects.Select(obj => ((object)obj, displayAdapter.GetDisplayString(obj)));

        // Show the fuzzy selector UI and get the selected item
        var selected = FuzzySelector.Show(items);

        // Write the selected object to the pipeline if a selection was made
        if (selected != null)
        {
            WriteObject(selected);
        }
    }
}
