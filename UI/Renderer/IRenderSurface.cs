namespace PSFuzzySelect.UI.Renderer;

/// <summary>
/// Represents a 2D character-based surface that can be read from and written to
/// </summary>
public interface IRenderSurface
{
    /// <summary>Gets the width of the render surface in characters</summary>
    int Width { get; }

    /// <summary>Gets the height of the render surface in characters</summary>
    int Height { get; }

    /// <summary>Retrieves the character at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the character to retrieve</param>
    /// <param name="y">The Y coordinate of the character to retrieve</param>
    /// <returns>The character at the specified position on the render surface</returns>
    Cell GetCell(int x, int y);

    /// <summary>Retrieves the entire line of characters at the specified Y coordinate on the render surface</summary>
    /// <param name="y">The Y coordinate of the line to retrieve</param>
    /// <returns>A string representing the line of characters at the specified Y coordinate on the render surface</returns>
    // string GetLine(int y);

    /// <summary>Clears the render surface, resetting all characters to a default state</summary>
    void Clear();

    /// <summary>Draws a single character at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the character to draw</param>
    /// <param name="y">The Y coordinate of the character to draw</param>
    /// <param name="character">The character to draw at the specified position</param>
    void Write(int x, int y, char character);

    /// <summary>Draws a single cell at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the cell to draw</param>
    /// <param name="y">The Y coordinate of the cell to draw</param>
    /// <param name="cell">The cell to draw at the specified position</param>
    void Write(int x, int y, Cell cell);

    /// <summary>Draws a string of characters starting at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the first character to draw</param>
    /// <param name="y">The Y coordinate of the first character to draw</param>
    /// <param name="text">The string of characters to draw starting at the specified position</param>
    void Write(int x, int y, string text);

    /// <summary>Draws a string of cells starting at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the first cell to draw</param>
    /// <param name="y">The Y coordinate of the first cell to draw</param>
    /// <param name="text">The string of characters to draw starting at the specified position</param>
    /// <param name="style">The style to apply to the string of characters</param>
    void Write(int x, int y, string text, string? style);

    /// <summary>Creates a new sub-surface that represents a portion of the current render surface</summary>
    /// <param name="rect">The rectangle defining the area of the current render surface to use for the new sub-surface</param>
    /// <returns>A new IRenderSurface that represents the specified portion of the current render surface</returns>
    IRenderSurface CreateSubSurface(Rect rect);
}
