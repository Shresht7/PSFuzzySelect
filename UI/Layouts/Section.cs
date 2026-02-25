namespace PSFuzzySelect.UI.Layouts;

/// <summary>
/// Defines the layout sections for a UI component, allowing for fixed or flexible sizing based on the available space
/// </summary>
public abstract record LayoutSection;

/// <summary>
/// Represents a fixed layout section that takes up a specific number of characters or lines regardless of the available space
/// </summary>
/// <param name="Size">The number of characters this section should occupy</param>
public record Fixed(int Size) : LayoutSection;

/// <summary>
/// Represents a flexible layout section that takes up a proportion of the remaining space based on its factor relative to other flexible sections
/// </summary>
/// <param name="Factor">The factor that determines the proportion of the remaining space this section should take up relative to other flexible sections</param>
public record Flexible(int Factor) : LayoutSection;

/// <summary>
/// Represents a fraction layout section that takes up a specific percentage of the available space
/// </summary>
/// <param name="Frac">The fraction of the available space this section should occupy, expressed as a float between 0 and 1</param>
public record Fraction(float Frac) : LayoutSection;

// Implement Fill and Percentage if needed in the future
// public record Fill : LayoutSection;
