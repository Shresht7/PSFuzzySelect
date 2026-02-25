namespace PSFuzzySelect.UI.Renderer;

public interface IRenderSurface
{
    /// <summary>Gets the width of the render surface in characters</summary>
    int Width { get; }

    /// <summary>Gets the height of the render surface in characters</summary>
    int Height { get; }

    /// <summary>Clears the render surface, resetting all characters to a default state</summary>
    void Clear();

    /// <summary>Draws a single character at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the character to draw</param>
    /// <param name="y">The Y coordinate of the character to draw</param>
    /// <param name="character">The character to draw at the specified position</param>
    void Write(int x, int y, char character);

    /// <summary>Draws a string of characters starting at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the first character to draw</param>
    /// <param name="y">The Y coordinate of the first character to draw</param>
    /// <param name="text">The string of characters to draw starting at the specified position</param>
    void Write(int x, int y, string text);
}
