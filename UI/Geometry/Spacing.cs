namespace PSFuzzySelect.UI.Geometry;

/// <summary>
/// Represents the spacing around a UI component, defining the amount of space on each side (left, top, right, bottom) 
/// that should be reserved for padding or margins
/// </summary>
public readonly struct Spacing
{
    /// <summary>The space on the left</summary>
    public int Left { get; }

    /// <summary>The space at the top</summary>
    public int Top { get; }

    /// <summary>The space on the right</summary>
    public int Right { get; }

    /// <summary>The space at the bottom</summary>
    public int Bottom { get; }

    /// <summary>
    /// Initializes a new instance of the Spacing struct with the same value for all sides (left, top, right, bottom)
    /// </summary>
    /// <param name="all">The spacing value to apply to all sides</param>
    public Spacing(int all) : this(all, all, all, all) { }

    /// <summary>
    /// Initializes a new instance of the Spacing struct with the specified horizontal and vertical spacing values,
    /// </summary>
    /// <param name="horizontal">The horizontal spacing value</param>
    /// <param name="vertical">The vertical spacing value</param>
    public Spacing(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }

    /// <summary>
    /// Initializes a new instance of the Spacing struct with the specified left, top, right, and bottom spacing values
    /// </summary>
    /// <param name="left">The left spacing value</param>
    /// <param name="top">The top spacing value</param>
    /// <param name="right">The right spacing value</param>
    /// <param name="bottom">The bottom spacing value</param>
    public Spacing(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>Gets a spacing instance with all sides set to zero, representing no spacing</summary>
    public static Spacing None => new Spacing(0);

    /// <summary>
    /// Creates a spacing instance with the specified horizontal spacing applied to left and right, and zero vertical spacing.
    /// </summary>
    /// <param name="value">The horizontal spacing value to apply to the left and right sides.</param>
    public static Spacing Horizontal(int value) => new Spacing(value, 0);

    /// <summary>
    /// Creates a spacing instance with the specified vertical spacing applied to top and bottom, and zero horizontal spacing.
    /// </summary>
    /// <param name="value">The vertical spacing value to apply to the top and bottom sides.</param>
    public static Spacing Vertical(int value) => new Spacing(0, value);


    /// <summary>Implicitly converts an integer to a Spacing instance, applying the same value to all sides</summary>
    /// <param name="all">The spacing value to apply to all sides</param>
    public static implicit operator Spacing(int all) => new Spacing(all);

    /// <summary>Implicitly converts a tuple of (horizontal, vertical) to a Spacing instance, applying the horizontal value to left and right, and the vertical value to top and bottom</summary>
    /// <param name="values">A tuple containing the horizontal and vertical spacing values</param>
    public static implicit operator Spacing((int horizontal, int vertical) values) => new Spacing(values.horizontal, values.vertical);

    /// <summary>Implicitly converts a tuple of (left, top, right, bottom) to a Spacing instance, applying the respective values to each side</summary>
    /// <param name="values">A tuple containing the left, top, right, and bottom spacing values</param>
    public static implicit operator Spacing((int left, int top, int right, int bottom) values) =>
        new Spacing(values.left, values.top, values.right, values.bottom);
}
