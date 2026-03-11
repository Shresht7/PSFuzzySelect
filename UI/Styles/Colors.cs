namespace PSFuzzySelect.UI.Styles;

/// <summary>
/// Represents a color in the terminal, which can be either a standard ANSI 16-color index,
/// an 8-bit (256-color) index, or a 24-bit TrueColor (RGB) value.
/// </summary>
public readonly record struct Color(int? AnsiIndex, byte? R, byte? G, byte? B)
{
    public bool IsRgb => R.HasValue && G.HasValue && B.HasValue;
    public bool IsAnsi => AnsiIndex.HasValue;

    public static Color FromAnsi(int index) => new(index, null, null, null);
    public static Color FromRgb(byte r, byte g, byte b) => new(null, r, g, b);

    // Standard 16-color palette (Compatibility with previous Enum)
    public static Color Black => FromAnsi(30);
    public static Color Red => FromAnsi(31);
    public static Color Green => FromAnsi(32);
    public static Color Yellow => FromAnsi(33);
    public static Color Blue => FromAnsi(34);
    public static Color Magenta => FromAnsi(35);
    public static Color Cyan => FromAnsi(36);
    public static Color White => FromAnsi(37);
    public static Color Default => FromAnsi(39);

    // Bright 16-color palette
    public static Color BrightBlack => FromAnsi(90);
    public static Color BrightRed => FromAnsi(91);
    public static Color BrightGreen => FromAnsi(92);
    public static Color BrightYellow => FromAnsi(93);
    public static Color BrightBlue => FromAnsi(94);
    public static Color BrightMagenta => FromAnsi(95);
    public static Color BrightCyan => FromAnsi(96);
    public static Color BrightWhite => FromAnsi(97);
    public static Color BrightDefault => FromAnsi(99);
}
