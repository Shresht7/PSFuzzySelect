namespace PSFuzzySelect.UI.Layouts;

/// <summary>
/// Defines the layout sections for a UI component, allowing for fixed or flexible sizing based on the available space
/// </summary>
public abstract record Size
{
    public static Size Fixed(int size) => new FixedSize(size);
    public static Size Flexible(int factor) => new FlexibleSize(factor);
    public static Size Fractional(float frac) => new FractionalSize(frac);
}

/// <summary>
/// Represents a fixed layout section that takes up a specific number of characters or lines regardless of the available space
/// </summary>
/// <param name="Size">The number of characters this section should occupy</param>
public record FixedSize(int Size) : Size;

/// <summary>
/// Represents a flexible layout section that takes up a proportion of the remaining space based on its factor relative to other flexible sections
/// </summary>
/// <param name="Factor">The factor that determines the proportion of the remaining space this section should take up relative to other flexible sections</param>
public record FlexibleSize(int Factor) : Size;

/// <summary>
/// Represents a fraction layout section that takes up a specific percentage of the available space
/// </summary>
/// <param name="Frac">The fraction of the available space this section should occupy, expressed as a float between 0 and 1</param>
public record FractionalSize(float Frac) : Size;
