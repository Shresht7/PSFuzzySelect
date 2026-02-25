namespace PSFuzzySelect.UI.Layouts;

/// <summary>
/// Defines the layout constraints for a UI component, allowing for fixed or flexible sizing based on the available space
/// </summary>
public abstract record LayoutConstraint;

/// <summary>
/// Represents a fixed layout constraint that takes up a specific number of characters or lines regardless of the available space
/// </summary>
/// <param name="Size">The number of characters this constraint should occupy</param>
public record Fixed(int Size) : LayoutConstraint;

/// <summary>
/// Represents a flexible layout constraint that takes up a proportion of the remaining space based on its factor relative to other flexible constraints
/// </summary>
/// <param name="Factor">The factor that determines the proportion of the remaining space this constraint should take up relative to other flexible constraints</param>
public record Flexible(int Factor) : LayoutConstraint;

// Implement Fill and Percentage if needed in the future
// public record Fill : LayoutConstraint;
// public record Percentage(float Percent) : LayoutConstraint;
