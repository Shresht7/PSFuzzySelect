using PSFuzzySelect.UI.Helpers;

namespace PSFuzzySelect.UI.Renderer;

public class ConsoleRenderer
{
    public FrameBuffer CurrentBuffer { get; set; }

    private FrameBuffer? _previousBuffer;
    private readonly int _width;
    private readonly int _height;

    public ConsoleRenderer(int width, int height)
    {
        _width = width;
        _height = height;
        CurrentBuffer = new FrameBuffer(width, height);
        _previousBuffer = null;
    }

    public FrameBuffer CreateBuffer() => new FrameBuffer(_width, _height);

    public void Render(FrameBuffer currentBuffer)
    {
        for (int y = 0; y < currentBuffer.Height; y++)
        {
            for (int x = 0; x < currentBuffer.Width; x++)
            {
                Cell currentCell = currentBuffer.GetCell(x, y);
                Cell previousCell = _previousBuffer?.GetCell(x, y) ?? new Cell(' ');

                // Only update the cell if it has changed since the last render
                if (currentCell != previousCell)
                {
                    // !! This is temporary. Eventually we want to batch updates together and minimize the number of cursor movements and writes to the console for better performance.
                    Console.SetCursorPosition(x, y);
                    if (currentCell.Style != null)
                    {
                        Console.Write(currentCell.Style?.ToAnsi());
                    }
                    Console.Write(currentCell.Character);
                    if (currentCell.Style != null)
                    {
                        Console.Write(Ansi.Reset);
                    }
                }
            }
        }

        // Update the previous buffer reference to the current buffer for the next render cycle
        _previousBuffer = currentBuffer;
    }
}
