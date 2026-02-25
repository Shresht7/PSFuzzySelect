namespace PSFuzzySelect.UI.Renderer;

public class FrameBuffer : IRenderSurface
{
    /// <summary>
    /// The underlying character buffer that holds the current state of the render surface.
    /// Each element in the 2D array represents a character at a specific position on the console.
    /// </summary>
    private readonly char[,] _buffer;

    public int Width { get; }

    public int Height { get; }

    public FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        _buffer = new char[height, width];
        Clear();
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _buffer[y, x] = ' ';    // Clear the buffer by setting all characters to a empty space
            }
        }
    }

    public void Write(int x, int y, char character)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _buffer[y, x] = character;   // Write the character to the buffer at the specified position
        }
    }

    public void Write(int x, int y, string text)
    {
        // Write each character of the string to the buffer starting at the specified position
        for (int i = 0; i < text.Length; i++)
        {
            Write(x + i, y, text[i]);
        }
    }
}
