namespace PSFuzzySelect.UI.Styles;

/// <summary>
/// Helper class for ANSI escape code generation
/// </summary>
public static class Ansi
{
    /// <summary>The ANSI escape code prefix for control sequences</summary>
    private const string Esc = "\x1b[";

    // RESET
    // -----

    /// <summary>ANSI escape code for resetting all attributes</summary>
    public static string Reset = $"{Esc}0m";

    // CURSOR MANIPULATION
    // -------------------

    /// <summary>ANSI escape code for hiding the cursor</summary>
    /// <remarks>Use `CursorShow` to show the cursor again</remarks>
    /// <returns>`\x1b[?25l`</returns>
    public static string CursorHide => $"{Esc}?25l";

    /// <summary>ANSI escape code for showing the cursor</summary>
    /// <returns>`\x1b[?25h`</returns>
    public static string CursorShow => $"{Esc}?25h";

    /// <summary>Generates an ANSI escape code to move the cursor up by a specified number of lines</summary>
    /// <param name="number">The number of lines to move the cursor up</param>
    /// <returns>`\x1b[{number}A`</returns>
    public static string CursorUp(int number = 1) => $"{Esc}{number}A";

    /// <summary>Generates an ANSI escape code to move the cursor down by a specified number of lines</summary>
    /// <param name="number">The number of lines to move the cursor down</param>
    /// <returns>`\x1b[{number}B`</returns>
    public static string CursorDown(int number = 1) => $"{Esc}{number}B";

    /// <summary>Generates an ANSI escape code to move the cursor forward (right) by a specified number of columns</summary>
    /// <param name="number">The number of columns to move the cursor forward</param>
    /// <returns>`\x1b[{number}C`</returns>
    public static string CursorRight(int number = 1) => $"{Esc}{number}C";

    /// <summary>Generates an ANSI escape code to move the cursor backward (left) by a specified number of columns</summary>
    /// <param name="number">The number of columns to move the cursor backward</param>
    /// <returns>`\x1b[{number}D`</returns>
    public static string CursorLeft(int number = 1) => $"{Esc}{number}D";

    /// <summary>Generates an ANSI escape code to move the cursor to a specific position (row and column)</summary>
    /// <param name="row">The row number to move the cursor to (0-based index)</param>
    /// <param name="column">The column number to move the cursor to (0-based index)</param>
    /// <returns>`\x1b[{row + 1};{column + 1}H`</returns>
    public static string CursorPosition(int row, int column) => $"{Esc}{row + 1};{column + 1}H";

    // CLEAR
    // -----

    /// <summary>Generates an ANSI escape code to clear the entire screen</summary>
    /// <returns>`\x1b[2J`</returns>
    public static string ClearScreen => $"{Esc}2J";

    /// <summary>Generates an ANSI escape code to clear the current line</summary>
    /// <returns>`\x1b[2K`</returns>
    public static string ClearLine => $"{Esc}2K";

    /// <summary>Generates an ANSI escape code to clear from the cursor position to the end of the line</summary>
    /// <returns>`\x1b[K`</returns>
    public static string ClearLineFromCursor => $"{Esc}K";

    /// <summary>Generates an ANSI escape code to clear from the cursor position to the beginning of the line</summary>
    /// <returns>`\x1b[1K`</returns>
    public static string ClearLineToCursor => $"{Esc}1K";

    // STYLES
    // ------

    /// <summary>ANSI escape code for bold text</summary>
    /// <returns>`\x1b[1m`</returns>
    public static string Bold => $"{Esc}1m";

    /// <summary>ANSI escape code for unbolding text</summary>
    /// <returns>`\x1b[22m`</returns>
    public static string UnBold => $"{Esc}22m"; // Not 21 for some reason

    /// <summary>ANSI escape code for dimmed text</summary>
    /// <returns>`\x1b[2m`</returns>
    public static string Dim => $"{Esc}2m";

    /// <summary>ANSI escape code for undimmed text</summary>
    /// <returns>`\x1b[22m`</returns>
    public static string UnDim => $"{Esc}22m";

    /// <summary>ANSI escape code for italic text</summary>
    /// <returns>`\x1b[3m`</returns>
    public static string Italic => $"{Esc}3m";

    /// <summary>ANSI escape code for unitalicizing text</summary>
    /// <returns>`\x1b[23m`</returns>
    public static string UnItalic => $"{Esc}23m";

    /// <summary>ANSI escape code for underlined text</summary>
    /// <returns>`\x1b[4m`</returns>
    public static string Underline => $"{Esc}4m";

    /// <summary>ANSI escape code for ununderlining text</summary>
    /// <returns>`\x1b[24m`</returns>
    public static string UnUnderline => $"{Esc}24m";

    /// <summary>ANSI escape code for inverse text</summary>
    /// <returns>`\x1b[7m`</returns>
    public static string Inverse => $"{Esc}7m";

    /// <summary>ANSI escape code for uninverting text</summary>
    /// <returns>`\x1b[27m`</returns>
    public static string UnInverse => $"{Esc}27m";

    /// <summary>ANSI escape code for strikethrough text</summary>
    /// <returns>`\x1b[9m`</returns>
    public static string Strikethrough => $"{Esc}9m";

    /// <summary>ANSI escape code for unstrikethrough text</summary>
    /// <returns>`\x1b[29m`</returns>
    public static string UnStrikethrough => $"{Esc}29m";

    // COLORS
    // ------

    // Cache

    private static readonly Dictionary<Color, string> _fgCache;
    private static readonly Dictionary<Color, string> _bgCache;

    // Constructor
    static Ansi()
    {
        // Pre-generate ANSI escape codes for all colors and cache them for fast retrieval
        _fgCache = new Dictionary<Color, string>();
        _bgCache = new Dictionary<Color, string>();
        foreach (Color color in Enum.GetValues(typeof(Color)))
        {
            _fgCache[color] = $"{Esc}{(int)color}m";
            _bgCache[color] = $"{Esc}{(int)color + 10}m";
        }
    }

    // Foreground Colors

    /// <summary>Generates an ANSI escape code to set the foreground color</summary>
    /// <param name="color">The color to set for the foreground</param>
    /// <returns>`\x1b[{color}m`</returns>
    public static string Foreground(Color color) => _fgCache[color];

    /// <summary>Generates an ANSI escape code to set the foreground color using RGB values</summary>
    /// <param name="r">The red component of the color (0-255)</param>
    /// <param name="g">The green component of the color (0-255)</param>
    /// <param name="b">The blue component of the color (0-255)</param>
    /// <returns>`\x1b[38;2;{r};{g};{b}m`</returns>
    public static string ForegroundRgb(int r, int g, int b) => $"{Esc}38;2;{r};{g};{b}m";

    // Background Colors

    /// <summary>Generates an ANSI escape code to set the background color</summary>
    /// <param name="color">The color to set for the background</param>
    /// <returns>`\x1b[{color + 10}m`</returns>
    public static string Background(Color color) => _bgCache[color];

    /// <summary>Generates an ANSI escape code to set the background color using RGB values</summary>
    /// <param name="r">The red component of the color (0-255)</param>
    /// <param name="g">The green component of the color (0-255)</param>
    /// <param name="b">The blue component of the color (0-255)</param>
    /// <returns>`\x1b[48;2;{r};{g};{b}m`</returns>
    public static string BackgroundRgb(int r, int g, int b) => $"{Esc}48;2;{r};{g};{b}m";

    // ALTERNATE BUFFER
    // ----------------

    /// <summary>ANSI escape code to switch to the alternate buffer</summary>
    /// <remarks>Use `AltBufferExit` to switch back to the main buffer</remarks>
    /// <returns>`\x1b[?1049h`</returns>
    public static string AltBufferEnter => $"{Esc}?1049h";

    /// <summary>ANSI escape code to switch back to the main buffer</summary>
    /// <returns>`\x1b[?1049l`</returns>
    public static string AltBufferExit => $"{Esc}?1049l";
}
