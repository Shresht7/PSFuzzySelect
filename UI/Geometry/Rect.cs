namespace PSFuzzySelect.UI.Geometry;

/// <summary>
/// Defines a rectangle area in the console with a specified position <c>(X, Y)</c> and size <c>(Width, Height)</c>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Rect"/> struct with the specified position and size
/// </remarks>
public readonly struct Rect(int x, int y, int width, int height) : IEquatable<Rect>
{
    /// <summary>The X coordinate of the rectangle's top-left corner</summary>
    public int X { get; } = x;

    /// <summary>The Y coordinate of the rectangle's top-left corner</summary>
    public int Y { get; } = y;

    /// <summary>The width of the rectangle</summary>
    public int Width { get; } = width;

    /// <summary>The height of the rectangle</summary>
    public int Height { get; } = height;

    /// <summary>Gets an empty rectangle with all dimensions set to zero</summary>
    public static Rect Empty => new(0, 0, 0, 0);

    /// <summary>Gets a value indicating whether the rectangle is empty (i.e., has zero or negative width and height)</summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

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

    /// <summary>Determines whether the rectangle contains the specified point <c>(x, y)</c></summary>
    /// <param name="x">The X coordinate of the point to test for containment</param>
    /// <param name="y">The Y coordinate of the point to test for containment</param>
    /// <returns><c>true</c> if the rectangle contains the specified point; otherwise, <c>false</c></returns>
    public bool Contains(int x, int y)
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    /// <summary>
    /// Returns a new rectangle that is inset by the specified amount from all edges of the original rectangle
    /// </summary>
    /// <param name="delta">The amount to inset from all edges</param>
    /// <returns>A new rectangle that is inset by the specified amount from all edges</returns>
    public Rect Inset(int delta) => Inset(delta, delta, delta, delta);

    /// <summary>
    /// Returns a new rectangle that is inset by the specified amounts from the horizontal and vertical edges of the original rectangle
    /// </summary>
    /// <param name="horizontal">The amount to inset from the left and right edges</param>
    /// <param name="vertical">The amount to inset from the top and bottom edges</param>
    /// <returns>A new rectangle that is inset by the specified amounts from the horizontal and vertical edges</returns>
    public Rect Inset(int horizontal, int vertical) => Inset(horizontal, vertical, horizontal, vertical);

    /// <summary>
    /// Returns a new rectangle that is inset by the specified amounts from the edges of the original rectangle
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
    /// Returns a new rectangle that is inset by the specified spacing from all edges of the original rectangle
    /// </summary>
    /// <param name="spacing">The spacing to inset from all edges</param>
    /// <returns>A new rectangle that is inset by the specified spacing from all edges</returns>
    public Rect Inset(Spacing spacing) => Inset(spacing.Left, spacing.Top, spacing.Right, spacing.Bottom);

    /// <summary>
    /// Returns a new rectangle that is offset by the specified amount from all edges of the original rectangle
    /// </summary>
    /// <param name="delta">The amount to offset from all edges</param>
    /// <returns>A new rectangle that is offset by the specified amount from all edges</returns>
    public Rect Offset(int delta) => Offset(delta, delta, delta, delta);

    /// <summary>
    /// Returns a new rectangle that is offset by the specified amounts from the horizontal and vertical edges of the original rectangle
    /// </summary>
    /// <param name="horizontal">The amount to offset from the left and right edges</param>
    /// <param name="vertical">The amount to offset from the top and bottom edges</param>
    /// <returns>A new rectangle that is offset by the specified amounts from the horizontal and vertical edges</returns>
    public Rect Offset(int horizontal, int vertical) => Offset(horizontal, vertical, horizontal, vertical);

    /// <summary>
    /// Returns a new rectangle that is offset by the specified amounts from the edges of the original rectangle
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

    /// <summary>
    /// Returns a new rectangle that is offset by the specified spacing from all edges of the original rectangle
    /// </summary>
    /// <param name="spacing">The spacing to offset from all edges</param>
    /// <returns>A new rectangle that is offset by the specified spacing from all edges</returns>
    public Rect Offset(Spacing spacing) => Offset(spacing.Left, spacing.Top, spacing.Right, spacing.Bottom);

    /// <summary>
    /// Returns a new rectangle that is translated by the specified amounts in the X and Y directions
    /// </summary>
    /// <param name="deltaX">The amount to translate in the X direction</param>
    /// <param name="deltaY">The amount to translate in the Y direction</param>
    /// <returns>A new rectangle that is translated by the specified amounts</returns>
    public Rect Translate(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY, Width, Height);

    public override bool Equals(object? obj) => obj is Rect other && Equals(other);
    public bool Equals(Rect other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public static bool operator ==(Rect left, Rect right) => left.Equals(right);
    public static bool operator !=(Rect left, Rect right) => !left.Equals(right);
}
