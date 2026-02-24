using System.Management.Automation;

namespace PSFuzzySelect;

[Cmdlet(VerbsCommon.Select, "Fuzzy")]
[OutputType(typeof(PSObject))]
public class SelectFuzzyCmdlet : PSCmdlet
{
    protected override void ProcessRecord()
    {
        WriteObject("Hello, World!");
    }
}
