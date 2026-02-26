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

    // Track the terminal's current cursor position and style
    // to optimize rendering by minimizing unnecessary cursor movements and style changes
    private int _terminalX = 0;
    private int _terminalY = 0;
    private Style _terminalStyle = Style.Default;

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

    private void Setup()
    {
        Console.CursorVisible = false; // Hide the cursor for a cleaner rendering experience
        _terminalStyle = Style.Default; // Reset terminal style to default at the start
        // Unitialize to -1 to ensure the first cursor position is always updated to keep track of the terminal's cursor position
        _terminalX = -1;
        _terminalY = -1;
    }

    public void Render()
    {
        Setup();    // Ensure the console is in the correct state for rendering before we start drawing the frame

        // The final string representation of the frame to be rendered to the console
        var frame = new StringBuilder();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Cell currentCell = _backBuffer.GetCell(x, y);
                Cell previousCell = _frontBuffer.GetCell(x, y);

                // Only render the cell if it has changed since the last frame
                if (currentCell == previousCell) continue;

                // Only move the cursor if we aren't already where we need to be
                if (x != _terminalX || y != _terminalY)
                {
                    frame.Append(Ansi.CursorPosition(y, x));
                }

                // Only change the style codes if the style is different from the terminal's current "brush" style
                if (currentCell.Style != _terminalStyle)
                {
                    // We always reset before applying new styles to ensure
                    // that there are no lingering styles from the previous cell
                    // Skip if the terminal style is already default
                    if (_terminalStyle != Style.Default)
                    {
                        frame.Append(Ansi.Reset);
                    }

                    frame.Append(currentCell.Style.ToAnsi());
                    _terminalStyle = currentCell.Style;
                }

                // Write the actual character
                frame.Append(currentCell.Character);

                // Update the terminal's cursor state
                _terminalX = x + 1; // After writing a character, the terminal's cursor naturally moves to the right
                _terminalY = y;
            }

        }

        // Write the final frame to the console
        if (frame.Length > 0)
        {
            Console.Write(frame.ToString());
        }

        // Swap the buffers for the next render cycle
        (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);

        Cleanup(); // Leave the terminal in a clean state after rendering the frame
    }

    private void Cleanup()
    {
        Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
    }
}
