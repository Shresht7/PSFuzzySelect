using System.Management.Automation;
using System.Text;

namespace PSFuzzySelect.Helpers;

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
        if (_properties?.Length > 0)
        {
            return GetPropertyDisplay(obj, _properties);
        }

        // Otherwise, attempt to use the object's default display properties
        var defaultProperties = GetDefaultDisplayProperties(obj);
        if (defaultProperties?.Length > 0)
        {
            return GetPropertyDisplay(obj, defaultProperties);
        }

        // Fallback to using the object's ToString() method
        return obj.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Generates a display string for a given PSObject based on the specified properties
    /// </summary>
    /// <param name="obj">The PSObject to generate a display string for</param>
    /// <param name="properties">The properties to use for generating the display string</param>
    /// <returns>A string representation of the PSObject based on the specified properties</returns>
    private string GetPropertyDisplay(PSObject obj, string[] properties)
    {
        var sb = new StringBuilder();
        bool first = true;  // Check to avoid leading separator

        foreach (var propName in properties)
        {
            // Append a separator if this is not the first property
            if (!first) sb.Append(" | ");
            first = false;

            // Append the property value
            var property = obj.Properties[propName];
            sb.Append(property?.Value?.ToString() ?? string.Empty);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retrieves the default display properties for a given PSObject
    /// </summary>
    /// <param name="obj">The PSObject to retrieve default display properties for</param>
    /// <returns>An array of property names that are considered default for display, or null if none are found</returns>
    private string[]? GetDefaultDisplayProperties(PSObject obj)
    {
        // Check for PSStandardMembers and DefaultDisplayPropertySet
        var standardMembers = obj.Members["PSStandardMembers"]?.Value as PSMemberSet;
        var displayPropertySet = standardMembers?.Properties["DefaultDisplayPropertySet"]?.Value as PSPropertySet;

        if (displayPropertySet?.ReferencedPropertyNames != null)
        {
            return displayPropertySet.ReferencedPropertyNames.ToArray();
        }

        return null;
    }
}
