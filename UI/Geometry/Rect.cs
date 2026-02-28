namespace PSFuzzySelect.UI.Geometry;

/// <summary>
/// Defines a rectangle area in the console with a specified position (X, Y) and size (Width, Height)
/// </summary>
public readonly struct Rect
{
    /// <summary>The X coordinate of the rectangle's top-left corner</summary>
    public int X { get; }

    /// <summary>The Y coordinate of the rectangle's top-left corner</summary>
    public int Y { get; }

    /// <summary>The width of the rectangle</summary>
    public int Width { get; }

    /// <summary>The height of the rectangle</summary>
    public int Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct with the specified position and size.
    /// </summary>
    /// <param name="x">The X coordinate of the rectangle's top-left corner</param>
    /// <param name="y">The Y coordinate of the rectangle's top-left corner</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    public Rect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>Gets an empty rectangle with all dimensions set to zero</summary>
    public static Rect Empty => new Rect(0, 0, 0, 0);

    /// <summary>Gets a value indicating whether the rectangle is empty (i.e., has zero or negative width and height).</summary>
    public bool IsEmpty => Width <= 0 && Height <= 0;

    /// <summary>Gets the top edge of the rectangle</summary>
    public int Top => Y;

    /// <summary>Gets the right edge of the rectangle</summary>
    public int Right => X + Width - 1;

    /// <summary>Gets the bottom edge of the rectangle</summary>
    public int Bottom => Y + Height - 1;

    /// <summary>Gets the left edge of the rectangle</summary>
    public int Left => X;

    /// <summary>Determines whether the rectangle contains the specified rectangle</summary>
    /// <param name="other">The rectangle to test for containment</param>
    /// <returns><c>true</c> if the rectangle contains the specified rectangle; otherwise, <c>false</c></returns>
    public bool Contains(Rect other)
    {
        return other.Left >= Left && other.Right <= Right && other.Top >= Top && other.Bottom <= Bottom;
    }

    /// <summary>Determines whether the rectangle contains the specified point</summary>
    /// <param name="x">The X coordinate of the point to test for containment</param>
    /// <param name="y">The Y coordinate of the point to test for containment</param>
    /// <returns><c>true</c> if the rectangle contains the specified point; otherwise, <c>false</c></returns>
    public bool Contains(int x, int y)
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    /// <summary>
    /// Returns a new rectangle that is inset by the specified amount from all edges of the original rectangle.
    /// </summary>
    /// <param name="delta">The amount to inset from all edges</param>
    /// <returns>A new rectangle that is inset by the specified amount from all edges</returns>
    public Rect Inset(int delta) => Inset(delta, delta, delta, delta);

    /// <summary>
    /// Returns a new rectangle that is inset by the specified amounts from the horizontal and vertical edges of the original rectangle.
    /// </summary>
    /// <param name="horizontal">The amount to inset from the left and right edges</param>
    /// <param name="vertical">The amount to inset from the top and bottom edges</param>
    /// <returns>A new rectangle that is inset by the specified amounts from the horizontal and vertical edges</returns>
    public Rect Inset(int horizontal, int vertical) => Inset(horizontal, vertical, horizontal, vertical);

    /// <summary>
    /// Returns a new rectangle that is inset by the specified amounts from the edges of the original rectangle.
    /// </summary>
    /// <param name="left">The amount to inset from the left edge</param>
    /// <param name="top">The amount to inset from the top edge</param>
    /// <param name="right">The amount to inset from the right edge</param>
    /// <param name="bottom">The amount to inset from the bottom edge</param>
    /// <returns>A new rectangle that is inset by the specified amounts</returns>
    public Rect Inset(int left, int top, int right, int bottom)
    {
        return new Rect(X + left, Y + top, Width - left - right, Height - top - bottom);
    }

    /// <summary>
    /// Returns a new rectangle that is offset by the specified amount from all edges of the original rectangle.
    /// </summary>
    /// <param name="delta">The amount to offset from all edges</param>
    /// <returns>A new rectangle that is offset by the specified amount from all edges</returns>
    public Rect Offset(int delta) => Offset(delta, delta, delta, delta);

    /// <summary>
    /// Returns a new rectangle that is offset by the specified amounts from the horizontal and vertical edges of the original rectangle.
    /// </summary>
    /// <param name="horizontal">The amount to offset from the left and right edges</param>
    /// <param name="vertical">The amount to offset from the top and bottom edges</param>
    /// <returns>A new rectangle that is offset by the specified amounts from the horizontal and vertical edges</returns>
    public Rect Offset(int horizontal, int vertical) => Offset(horizontal, vertical, horizontal, vertical);

    /// <summary>
    /// Returns a new rectangle that is offset by the specified amounts from the edges of the original rectangle.
    /// </summary>
    /// <param name="left">The amount to offset from the left edge</param>
    /// <param name="top">The amount to offset from the top edge</param>
    /// <param name="right">The amount to offset from the right edge</param>
    /// <param name="bottom">The amount to offset from the bottom edge</param>
    /// <returns>A new rectangle that is offset by the specified amounts</returns>
    public Rect Offset(int left, int top, int right, int bottom)
    {
        return new Rect(X + left, Y + top, Width + left + right, Height + top + bottom);
    }
}
