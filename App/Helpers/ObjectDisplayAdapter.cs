using System.Management.Automation;
using System.Text;

namespace PSFuzzySelect.App.Helpers;

/// <summary>
/// Converts PSObjects into displayable string representations based on specified properties or default display properties
/// </summary>
public class ObjectDisplayAdapter(string[]? properties = null)
{
    /// <summary>
    /// An optional array of property names to use for display. 
    /// If null or empty, the adapter will attempt to use the object's default display properties or ToString() method.
    /// </summary>
    private readonly string[]? _properties = properties;

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
    private static string GetPropertyDisplay(PSObject obj, string[] properties)
    {
        var sb = new StringBuilder();
        bool first = true;  // Check to avoid leading separator

        foreach (var property in properties)
        {
            // Append a separator if this is not the first property
            if (!first) sb.Append(" | ");
            first = false;

            // Append the property value
            var value = ResolvePropertyValue(obj, property);
            sb.Append(value?.ToString() ?? string.Empty);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retrieves the default display properties for a given PSObject
    /// </summary>
    /// <param name="obj">The PSObject to retrieve default display properties for</param>
    /// <returns>An array of property names that are considered default for display, or null if none are found</returns>
    private static string[]? GetDefaultDisplayProperties(PSObject obj)
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

    /// <summary>
    /// Resolves a nested property value from a PSObject based on a dot-separated property path (e.g., "Address.City")
    /// using the PowerShell property system, which can handle complex objects and collections.
    /// </summary>
    /// <param name="obj">The PSObject to resolve the property value from</param>
    /// <param name="path">The dot-separated property path</param>
    /// <returns>The resolved property value, or null if not found</returns>
    private static object? ResolvePropertyValue(PSObject obj, string path)
    {
        if (string.IsNullOrEmpty(path)) return null; // If the path is empty, return null

        // Split the property path into parts
        var parts = path.Split('.');
        object? current = obj;

        // Traverse the property path, resolving each part using the PowerShell property system
        foreach (var part in parts)
        {
            if (current == null) return null; // If at any point we encounter a null value, return null

            // Wrap the current object in a PSObject if it is not already,
            // so we can use the PowerShell property system to find the next part
            var psCurrent = PSObject.AsPSObject(current);
            var property = psCurrent.Properties[part];

            if (property == null) return null; // Property not found
            current = property.Value;
        }

        return current; // Return the final resolved value
    }
}
