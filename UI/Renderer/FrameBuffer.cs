using PSFuzzySelect.UI.Geometry;

namespace PSFuzzySelect.UI.Renderer;

/// <summary>A rectangular frame buffer for rendering characters to the console</summary>
public class FrameBuffer : ISurface
{
    /// <summary>
    /// The underlying character buffer that holds the current state of the render surface.
    /// Each element in the 2D array represents a cell (a character with styles) at a specific position on the console.
    /// </summary>
    private readonly Cell[,] _buffer;

    public int Width { get; }

    public int Height { get; }

    public Rect Area => new Rect(0, 0, Width, Height);

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameBuffer"/> class with the specified width and height.
    /// </summary>
    /// <remarks>This constructs a root frame buffer.</remarks>
    /// <param name="width">The width of the frame buffer in cells</param>
    /// <param name="height">The height of the frame buffer in cells</param>
    public FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        _buffer = new Cell[height, width];
    }

    public Cell GetCell(int x, int y)
    {
        if (Area.Contains(x, y))
        {
            return _buffer[y, x];
        }

        return Cell.Empty;
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _buffer[y, x] = Cell.Empty;
            }
        }
    }

    public void Write(int x, int y, Cell cell)
    {
        if (Area.Contains(x, y))
        {
            _buffer[y, x] = cell;
        }
    }

    public ISurface CreateSubSurface(Rect rect) => new SubSurface(this, rect);

}
