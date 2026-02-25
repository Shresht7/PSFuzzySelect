using System.Text;

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
        // The final string representation of the frame to be rendered to the console
        var frame = new StringBuilder();

        for (int y = 0; y < currentBuffer.Height; y++)
        {
            int runStart = -1;
            Style activeStyle = Style.Default;

            for (int x = 0; x < currentBuffer.Width; x++)
            {
                Cell currentCell = currentBuffer.GetCell(x, y);
                Cell previousCell = _previousBuffer?.GetCell(x, y) ?? new Cell(' ');

                // Only update the cell if it has changed since the last render
                if (currentCell != previousCell)
                {
                    // Start the first run if uninitialized
                    if (runStart == -1)
                    {
                        runStart = x;
                        activeStyle = currentCell.Style ?? Style.Default;
                        frame.Append(Ansi.CursorPosition(y, x));
                        frame.Append(activeStyle.ToAnsi());
                    }

                    // Update the active style if it changes since the last cell
                    if (currentCell.Style != activeStyle)
                    {
                        if (currentCell.Style != null)
                        {
                            // ? These null-checks are getting annoying. Maybe something can be done about this.
                            frame.Append(Ansi.Reset);
                            activeStyle = currentCell.Style ?? Style.Default;
                            frame.Append(activeStyle.ToAnsi());
                        }
                    }

                    // Append the character to the frame
                    frame.Append(currentCell.Character);
                }
                else
                {
                    // If the cell is the same as the previous render, we can skip it and reset the run if it was active
                    if (runStart != -1)
                    {
                        frame.Append(Ansi.Reset);
                        runStart = -1;
                        activeStyle = Style.Default;
                    }
                }
            }

            // Reset the style at the end of the line if it was changed
            if (runStart != -1)
            {
                frame.Append(Ansi.Reset);
            }
        }

        // Write the final frame to the console
        if (frame.Length > 0)
        {
            Console.Out.Write(frame.ToString());
            Console.Out.Flush();
        }

        // Update the previous buffer reference to the current buffer for the next render cycle
        _previousBuffer = currentBuffer;
    }
}
