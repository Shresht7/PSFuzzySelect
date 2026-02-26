using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Renderer;

/// <summary>
/// Represents a 2D cell-based surface that can be read from and written to
/// </summary>
public interface ISurface
{
    /// <summary>The width of the render surface in cells</summary>
    int Width { get; }

    /// <summary>The height of the render surface in cells</summary>
    int Height { get; }

    /// <summary>
    /// The area of the render surface represented as a rectangle, 
    /// where the top-left corner is at (0, 0) and the size is defined by the Width and Height properties
    /// </summary>
    Rect Area { get; }

    /// <summary>Retrieves the cell at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the cell to retrieve</param>
    /// <param name="y">The Y coordinate of the cell to retrieve</param>
    /// <returns>The cell at the specified position on the render surface</returns>
    Cell GetCell(int x, int y);


    /// <summary>Clears the render surface, resetting all cells to a default state</summary>
    void Clear();

    /// <summary>Draws a single character at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the character to draw</param>
    /// <param name="y">The Y coordinate of the character to draw</param>
    /// <param name="character">The character to draw at the specified position</param>
    void Write(int x, int y, char character) => Write(x, y, new Cell(character, Style.Default));

    /// <summary>Draws a single cell at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the cell to draw</param>
    /// <param name="y">The Y coordinate of the cell to draw</param>
    /// <param name="cell">The cell to draw at the specified position</param>
    void Write(int x, int y, Cell cell);

    /// <summary>Draws a string of cells starting at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the first cell to draw</param>
    /// <param name="y">The Y coordinate of the first cell to draw</param>
    /// <param name="text">The string of cells to draw starting at the specified position</param>
    void Write(int x, int y, string text) => Write(x, y, text, Style.Default);

    /// <summary>Draws a string of cells with the specified style starting at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the first cell to draw</param>
    /// <param name="y">The Y coordinate of the first cell to draw</param>
    /// <param name="text">The string of cells to draw starting at the specified position</param>
    /// <param name="style">The style to apply to the cells being drawn</param>
    void Write(int x, int y, string text, Style style);

    /// <summary>Creates a new render surface that represents a sub-region of the current surface defined by the given rectangle</summary>
    /// <param name="rect">The rectangle defining the position and size of the sub-surface to create</param>
    /// <returns>A new render surface that represents the specified sub-region of the current surface</returns>
    ISurface CreateSubSurface(Rect rect);
}
