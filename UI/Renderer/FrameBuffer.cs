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
    private readonly Cell[,] _buffer;

    public int Width { get; }

    public int Height { get; }

    public FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        _buffer = new Cell[height, width];
        Clear();
    }

    public char GetChar(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return _buffer[y, x].Character;   // Return the character from the buffer at the specified position
        }
        return ' ';    // Return a space character if the position is out of bounds
    }

    public string GetLine(int y)
    {
        if (y >= 0 && y < Height)
        {
            // ? Consider optimizing this by using a StringBuilder or BlockCopy/Array.Copy
            // ? Will need to look at how ANSI codes come into play before I commit to something
            char[] lineChars = new char[Width];
            for (int x = 0; x < Width; x++)
            {
                lineChars[x] = _buffer[y, x].Character;   // Get each character in the specified line
            }
            return new string(lineChars);   // Return the line as a string
        }
        return string.Empty;    // Return an empty string if the line number is out of bounds
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _buffer[y, x] = new Cell(' ');    // Clear the buffer by setting all characters to a empty space
            }
        }
    }

    public void Write(int x, int y, char character)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _buffer[y, x] = new Cell(character);   // Write the character to the buffer at the specified position
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
