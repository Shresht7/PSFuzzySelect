using System.Text;
using Humanizer;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Renderer;

public class ConsoleRenderer
{
    private FrameBuffer _frontBuffer;
    private FrameBuffer _backBuffer;
    private readonly int _width;
    private readonly int _height;

    public ConsoleRenderer(int width, int height)
    {
        _width = width;
        _height = height;
        _backBuffer = new FrameBuffer(width, height);
        _frontBuffer = new FrameBuffer(width, height);
    }

    public ISurface GetBackBuffer()
    {
        _backBuffer.Clear();
        return _backBuffer;
    }

    public void Render()
    {
        // The final string representation of the frame to be rendered to the console
        var frame = new StringBuilder();

        for (int y = 0; y < _height; y++)
        {
            bool inRun = false;
            Style activeStyle = Style.Default;

            for (int x = 0; x < _width; x++)
            {
                Cell currentCell = _backBuffer.GetCell(x, y);
                Cell previousCell = _frontBuffer.GetCell(x, y);

                // Only update the cell if it has changed since the last render
                if (currentCell != previousCell)
                {
                    // Start the first run if uninitialized
                    if (!inRun)
                    {
                        frame.Append(Ansi.CursorPosition(y, x));
                        inRun = true;
                        frame.Append(Ansi.Reset); // Reset to clear any previous styles
                        activeStyle = Style.Default; // Start with default style for the new run
                    }

                    // Update the active style if it changes since the last cell
                    if (currentCell.Style != activeStyle)
                    {
                        frame.Append(Ansi.Reset); // Reset to clear any previous styles
                        frame.Append(currentCell.Style.ToAnsi());
                        activeStyle = currentCell.Style;
                    }

                    // Append the character to the frame
                    frame.Append(currentCell.Character);
                }
                else
                {
                    inRun = false;
                }
            }

            // Reset the style at the end of the line if it was changed
            if (inRun)
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

        // Swap the buffers for the next render cycle
        (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
    }
}
