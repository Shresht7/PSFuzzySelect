using System.Management.Automation;

using PSFuzzySelect.UI;

namespace PSFuzzySelect.Cmdlet;

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
    /// Indicates whether multiple items can be selected in the fuzzy selector UI. If set to true, users can select multiple items; otherwise, only a single item can be selected.
    /// This is implemented as a switch parameter, which means it can be used without an explicit value (e.g., -MultiSelect) to enable multi-selection mode.
    /// </summary>
    [Parameter]
    public SwitchParameter MultiSelect { get; set; }

    /// <summary>
    /// Properties to display and search on.
    /// If not specified, uses the object's default display properties or ToString() representation.
    /// </summary>
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty]
    public string[]? Property { get; set; }

    /// <summary>
    /// The prompt message to display in the fuzzy selector UI.
    /// </summary>
    [Parameter]
    public string Prompt { get; set; } = "Search:";

    #endregion Parameters

    #region Fields

    /// <summary>A list to hold all input objects received from the pipeline</summary>
    private readonly List<PSObject> _inputObjects = new();

    #endregion Fields

    #region Process

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

    #endregion Process

    #region End

    /// <summary>
    /// Called once after all input has been processed.
    /// </summary>
    protected override void EndProcessing()
    {
        // Show the fuzzy selector UI and get the selected item
        var selected = FuzzySelector.Show(Prompt, _inputObjects, Property, MultiSelect.IsPresent);

        // Write the selected object (or array of objects) to the pipeline if a selection was made
        if (selected != null)
        {
            WriteObject(selected, enumerateCollection: true);
        }
    }

    #endregion End
}
