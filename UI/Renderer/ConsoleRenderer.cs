namespace PSFuzzySelect.UI.Renderer;

public class ConsoleRenderer
{
    public static void Render(FrameBuffer currentBuffer, FrameBuffer? previousBuffer)
    {
        for (int y = 0; y < currentBuffer.Height; y++)
        {
            string currentLine = currentBuffer.GetLine(y);
            string previousLine = previousBuffer?.GetLine(y) ?? new string(' ', currentBuffer.Width);

            if (currentLine != previousLine)
            {
                Console.SetCursorPosition(0, y);   // Move the cursor to the beginning of the line
                Console.Write(currentLine);    // Write the current line to the console
            }
        }
    }
}
