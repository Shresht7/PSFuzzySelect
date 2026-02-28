using PSFuzzySelect.UI.Geometry;

namespace PSFuzzySelect.UI.Renderer;

/// <summary>
/// Represents a sub-region of a parent render surface, allowing for drawing operations
/// to be performed within a defined area while still being part of the larger surface.
/// </summary>
/// <param name="parent">The parent render surface.</param>
/// <param name="rect">The rectangle defining the sub-region.</param>
public class SubSurface(ISurface parent, Rect rect) : ISurface
{
    private readonly ISurface _parent = parent;
    private readonly Rect _rect = rect;

    public int Width => _rect.Width;
    public int Height => _rect.Height;
    public Rect Area => _rect;

    public Cell GetCell(int x, int y)
    {
        // Clipping
        if (IsOutOfBounds(x, y)) return Cell.Empty;
        // Translate coordinates to parent surface
        return _parent.GetCell(_rect.X + x, _rect.Y + y);
    }

    public void Write(int x, int y, Cell cell)
    {
        // Clipping
        if (IsOutOfBounds(x, y)) return;
        // Translate coordinates to parent surface
        _parent.Write(_rect.X + x, _rect.Y + y, cell);
    }

    public void Clear()
    {
        // To clear out the sub-surface, we fill the entire area with empty cells
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Write(x, y, Cell.Empty);
            }
        }
    }

    /// <summary>
    /// Determines if the given coordinates are outside the bounds of the sub-surface
    /// </summary>
    /// <param name="x">The x-coordinate to check</param>
    /// <param name="y">The y-coordinate to check</param>
    /// <returns>True if the coordinates are outside the bounds, otherwise false</returns>
    bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || x >= Width || y < 0 || y >= Height;
    }


    /// <summary>
    /// Creates a new sub-surface that represents a sub-region of the current sub-surface defined by the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle defining the sub-region</param>
    /// <returns>A new sub-surface representing the specified sub-region</returns>
    public ISurface CreateSubSurface(Rect rect) => new SubSurface(this, rect);
}
