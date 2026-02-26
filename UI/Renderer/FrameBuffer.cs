using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Renderer;

/// <summary>
/// A rectangular frame buffer for rendering characters to the console.
/// </summary>
public class FrameBuffer : IRenderSurface
{
    /// <summary>
    /// The underlying character buffer that holds the current state of the render surface.
    /// Each element in the 2D array represents a character at a specific position on the console.
    /// </summary>
    private readonly Cell[,]? _buffer;

    public int Width { get; }

    public int Height { get; }

    private readonly FrameBuffer? _parent;
    private readonly int _offsetX = 0;
    private readonly int _offsetY = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameBuffer"/> class with the specified width and height.
    /// </summary>
    /// <remarks>This constructs a root frame buffer.</remarks>
    /// <param name="width">The width of the frame buffer in characters</param>
    /// <param name="height">The height of the frame buffer in characters</param>
    public FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        _buffer = new Cell[height, width];
        Clear();
    }

    /// <summary>
    /// SubSurface constructor: Initializes a new instance of the <see cref="FrameBuffer"/> class that represents a sub-region of a parent frame buffer
    /// </summary>
    /// <param name="parent">The parent frame buffer that this sub-surface will be
    /// rendered onto. The sub-surface will read from and write to the parent buffer at the specified offset.</param>
    /// <param name="region">The rectangular region (x, y, width, height) within the parent buffer that this sub-surface will represent</param>
    /// <remarks>The sub-surface shares the same underlying buffer as the parent, but applies an offset to all read/write operations to map to the correct region of the parent buffer</remarks>
    private FrameBuffer(FrameBuffer parent, Rect region)
    {
        Width = region.Width;
        Height = region.Height;
        _parent = parent;
        _offsetX = region.X;
        _offsetY = region.Y;
    }

    public IRenderSurface CreateSubSurface(Rect region)
    {
        var rect = GetRect();
        if (!rect.Contains(region))
        {
            throw new ArgumentException("Sub-surface region must be fully contained within the parent surface");
        }
        return new FrameBuffer(this, region);
    }

    public Rect GetRect()
    {
        return new Rect(_offsetX, _offsetY, Width, Height);
    }

    public Cell GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return new Cell(' ');
        }

        if (_parent != null)
        {
            // If this is a sub-surface, read from the parent buffer with the appropriate offset
            return _parent.GetCell(x + _offsetX, y + _offsetY);
        }

        // If this is a root surface, read directly from the local buffer
        return _buffer![y, x];
    }

    private void SetCell(int x, int y, Cell cell)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return; // Ignore writes outside the bounds of the surface
        }

        if (_parent != null)
        {
            // If this is a sub-surface, write to the parent buffer with the appropriate offset
            _parent.SetCell(x + _offsetX, y + _offsetY, cell);
        }
        else
        {
            // If this is a root surface, write directly to the local buffer
            _buffer![y, x] = cell;
        }
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                SetCell(x, y, new Cell(' '));    // Clear the buffer by setting all characters to an empty space
            }
        }
    }

    public void Write(int x, int y, char character)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            SetCell(x, y, new Cell(character));   // Write the character to the buffer at the specified position
        }
    }

    public void Write(int x, int y, Cell cell)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            SetCell(x, y, cell);   // Write the cell to the buffer at the specified position
        }
    }

    public void Write(int x, int y, string text)
    {
        // Write each character of the string to the buffer starting at the specified position
        for (int i = 0; i < text.Length && x + i < Width && y < Height; i++)
        {
            Write(x + i, y, text[i]);
        }
    }

    public void Write(int x, int y, string text, Style? style)
    {
        // Write each character of the string to the buffer starting at the specified position with the specified style
        for (int i = 0; i < text.Length && x + i < Width && y < Height; i++)
        {
            Write(x + i, y, new Cell(text[i], style));
        }
    }
}
