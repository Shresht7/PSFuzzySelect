using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Geometry;

namespace PSFuzzySelect.UI.Surface;

/// <summary>
/// Represents a 2D cell-based surface that can be read from and written to
/// </summary>
public interface ISurface
{
    /// <summary>The width of the render surface in cells</summary>
    int Width { get; }

    /// <summary>The height of the render surface in cells</summary>
    int Height { get; }

    /// <summary>Gets the rectangular area of the render surface, starting at (0, 0) and extending to the width and height of the surface</summary>
    /// <returns>A Rect struct representing the area of the render surface</returns>
    Rect Area => new(0, 0, Width, Height);

    /// <summary>Retrieves the cell at the specified position on the render surface</summary>
    /// <param name="x">The X coordinate of the cell to retrieve</param>
    /// <param name="y">The Y coordinate of the cell to retrieve</param>
    /// <returns>The cell at the specified position on the render surface</returns>
    Cell GetCell(int x, int y);

    /// <summary>Fills the entire render surface with the specified cell</summary>
    /// <param name="cell">The cell to fill the render surface with</param>
    void Fill(Cell cell)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Write(x, y, cell);
            }
        }
    }

    /// <summary>Clears the render surface, resetting all cells to a default state</summary>
    void Clear()
    {
        Fill(Cell.Empty);
    }

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
    void Write(int x, int y, string text, Style style)
    {
        for (int i = 0; i < text.Length; i++)
        {
            Write(x + i, y, new Cell(text[i], style));
        }
    }

    /// <summary>Creates a new render surface that represents a sub-region of the current surface defined by the given rectangle</summary>
    /// <param name="rect">The rectangle defining the position and size of the sub-surface to create</param>
    /// <returns>A new render surface that represents the specified sub-region of the current surface</returns>
    ISurface CreateSubSurface(Rect rect);
}
