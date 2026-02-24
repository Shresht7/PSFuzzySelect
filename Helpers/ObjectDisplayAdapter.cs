using System.Management.Automation;
using System.Text;

namespace PSFuzzySelect;

/// <summary>
/// Converts PSObjects into displayable string representations based on specified properties or default display properties
/// </summary>
public class ObjectDisplayAdapter
{
    /// <summary>
    /// An optional array of property names to use for display. 
    /// If null or empty, the adapter will attempt to use the object's default display properties or ToString() method.
    /// </summary>
    private readonly string[]? _properties;

    public ObjectDisplayAdapter(string[]? properties = null)
    {
        _properties = properties;
    }

    /// <summary>
    /// Generates a display string for a given PSObject based on the specified properties or default display properties.
    /// </summary>
    /// <param name="obj">The PSObject to generate a display string for.</param>
    /// <returns>A string representation of the PSObject for display purposes.</returns>
    public string GetDisplayString(PSObject obj)
    {
        // If specific properties are provided, use them for display
        // if (_properties?.Length > 0)
        // {
        // TODO: Implement logic to extract specified properties from the PSObject and format them into a display string            
        // }

        // Otherwise, attempt to use the object's default display properties
        // TODO: Implement logic to determine default display properties based on the object's type and formatting rules

        // Fallback to using the object's ToString() method
        return obj.ToString() ?? string.Empty;
    }
}
