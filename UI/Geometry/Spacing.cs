namespace PSFuzzySelect.UI.Geometry;

/// <summary>
/// Represents the spacing around a UI component,
/// defining the amount of space on each side (left, top, right, bottom) 
/// that should be reserved for padding or margins
/// </summary>
/// <param name="Left">The amount of space to reserve on the left side</param>
/// <param name="Top">The amount of space to reserve on the top side</param>
/// <param name="Right">The amount of space to reserve on the right side</param>
/// <param name="Bottom">The amount of space to reserve on the bottom side</param>
public readonly record struct Spacing(int Left, int Top, int Right, int Bottom)
{
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

    /// <summary>Gets a spacing instance with all sides set to zero, representing no spacing</summary>
    public static Spacing None => new(0);

    /// <summary>
    /// Creates a spacing instance with the specified horizontal spacing applied to left and right, and zero vertical spacing
    /// </summary>
    /// <param name="value">The horizontal spacing value to apply to the left and right sides.</param>
    public static Spacing Horizontal(int value) => new(value, 0);

    public int TotalHorizontal => Left + Right;
    public int TotalVertical => Top + Bottom;

    /// <summary>
    /// Creates a spacing instance with the specified vertical spacing applied to top and bottom, and zero horizontal spacing
    /// </summary>
    /// <param name="value">The vertical spacing value to apply to the top and bottom sides.</param>
    public static Spacing Vertical(int value) => new(0, value);


    /// <summary>Implicitly converts an integer to a Spacing instance, applying the same value to all sides</summary>
    /// <param name="all">The spacing value to apply to all sides</param>
    public static implicit operator Spacing(int all) => new(all);

    /// <summary>Implicitly converts a tuple of (horizontal, vertical) to a Spacing instance, applying the horizontal value to left and right, and the vertical value to top and bottom</summary>
    /// <param name="values">A tuple containing the horizontal and vertical spacing values</param>
    public static implicit operator Spacing((int horizontal, int vertical) values) => new(values.horizontal, values.vertical);

    /// <summary>Implicitly converts a tuple of (left, top, right, bottom) to a Spacing instance, applying the respective values to each side</summary>
    /// <param name="values">A tuple containing the left, top, right, and bottom spacing values</param>
    public static implicit operator Spacing((int left, int top, int right, int bottom) values) =>
        new(values.left, values.top, values.right, values.bottom);

    /// <summary>
    /// Adds two Spacing instances together, resulting in a new Spacing instance where each side
    /// is the sum of the corresponding sides of the input instances
    /// </summary>
    /// <param name="a">The first Spacing instance</param>
    /// <param name="b">The second Spacing instance</param>
    /// <returns>A new Spacing instance with each side being the sum of the corresponding sides of the input instances</returns>
    public static Spacing operator +(Spacing a, Spacing b) =>
        new(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);

    /// <summary>
    /// Subtracts one Spacing instance from another, resulting in a new Spacing instance where each side
    /// is the difference of the corresponding sides of the input instances
    /// </summary>
    /// <param name="a">The first Spacing instance</param>
    /// <param name="b">The second Spacing instance</param>
    /// <returns>A new Spacing instance with each side being the difference of the corresponding sides of the input instances</returns>
    public static Spacing operator -(Spacing a, Spacing b) =>
        new(a.Left - b.Left, a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom);
}
