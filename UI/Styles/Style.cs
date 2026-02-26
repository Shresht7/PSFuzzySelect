using System.Text;

namespace PSFuzzySelect.UI.Styles
;

/// <summary>
/// Defines text styles that can be applied to characters drawn on a render surface, such as bold, italic, underline, inverse, dim, and strikethrough
/// </summary>
[Flags]
public enum TextStyle
{
    None = 0,
    Bold = 1 << 0,
    Italic = 1 << 1,
    Underline = 1 << 2,
    Inverse = 1 << 3,
    Dim = 1 << 4,
    Strikethrough = 1 << 5,
}

/// <summary>
/// Represents a combination of foreground color, background color, and text styles that can be applied to characters drawn on a render surface
/// </summary>
public readonly struct Style(Ansi.Color? foreground, Ansi.Color? background, TextStyle? textStyle)
{
    /// <summary>
    /// The foreground color of the style,
    /// or null if it should not override the default foreground color
    /// </summary>
    public Ansi.Color? Foreground { get; } = foreground;

    /// <summary>
    /// The background color of the style,
    /// or null if it should not override the default background color
    /// </summary>
    public Ansi.Color? Background { get; } = background;

    /// <summary>
    /// The text styles applied to the style,
    /// or null if no text styles are applied
    /// </summary>
    public TextStyle? TextStyles { get; } = textStyle;

    /// <summary>A default style with no foreground color, no background color, and no text styles applied</summary>
    public static Style Default => new Style(null, null, null);

    /// <summary>
    /// Creates a new Style instance with the specified foreground color
    /// </summary>
    /// <param name="foreground">The foreground color to apply</param>
    /// <returns>A new Style instance with the specified foreground color</returns>
    public Style WithForeground(Ansi.Color? foreground) => new Style(foreground, Background, TextStyles);

    /// <summary>
    /// Creates a new Style instance with the specified background color
    /// </summary>
    /// <param name="background">The background color to apply</param>
    /// <returns>A new Style instance with the specified background color</returns>
    public Style WithBackground(Ansi.Color? background) => new Style(Foreground, background, TextStyles);

    /// <summary>
    /// Creates a new Style instance with the specified text styles
    /// </summary>
    /// <param name="textStyle">The text styles to apply</param>
    /// <returns>A new Style instance with the specified text styles</returns>
    public Style WithTextStyle(TextStyle? textStyle) => new Style(Foreground, Background, textStyle);

    /// <summary>
    /// Creates a new Style instance by patching the current style with the non-null properties of another style. 
    /// For each property (foreground, background, text styles), if the corresponding property in the other style is non-null,
    /// it will override the value from the current style; otherwise, the value from the current style will be retained.
    /// </summary>
    /// <param name="other">The style to patch with</param>
    /// <returns>A new Style instance with the patched properties</returns>
    public Style Patch(Style other) => new Style(
        other.Foreground ?? Foreground,
        other.Background ?? Background,
        other.TextStyles ?? TextStyles
    );


    /// <summary>
    /// Generates the ANSI escape code string that represents this style, including foreground color, background color, and text styles
    /// </summary>
    /// <returns>A string containing the ANSI escape codes for the style</returns>
    public string ToAnsi()
    {
        var ansi = new StringBuilder();

        if (Foreground.HasValue)
            ansi.Append(Ansi.Foreground(Foreground.Value));

        if (Background.HasValue)
            ansi.Append(Ansi.Background(Background.Value));

        if (TextStyles.HasValue)
        {
            if (TextStyles.Value.HasFlag(TextStyle.Bold))
                ansi.Append(Ansi.Bold);
            if (TextStyles.Value.HasFlag(TextStyle.Italic))
                ansi.Append(Ansi.Italic);
            if (TextStyles.Value.HasFlag(TextStyle.Underline))
                ansi.Append(Ansi.Underline);
            if (TextStyles.Value.HasFlag(TextStyle.Inverse))
                ansi.Append(Ansi.Inverse);
            if (TextStyles.Value.HasFlag(TextStyle.Dim))
                ansi.Append(Ansi.Dim);
            if (TextStyles.Value.HasFlag(TextStyle.Strikethrough))
                ansi.Append(Ansi.Strikethrough);
        }

        return ansi.ToString();
    }

    public override bool Equals(object? obj) => obj is Style s && Equals(s);
    public bool Equals(Style other) =>
        Foreground == other.Foreground &&
        Background == other.Background &&
        TextStyles == other.TextStyles;

    public override int GetHashCode() => HashCode.Combine(Foreground, Background, TextStyles);
    public static bool operator ==(Style left, Style right) => left.Equals(right);
    public static bool operator !=(Style left, Style right) => !left.Equals(right);

    /// <summary>Returns a new Style instance with the Bold text style applied</summary>
    public Style Bold() => WithTextStyle((TextStyles ?? TextStyle.None) | TextStyle.Bold);

    /// <summary>Returns a new Style instance with the Italic text style applied</summary>
    public Style Italic() => WithTextStyle((TextStyles ?? TextStyle.None) | TextStyle.Italic);

    /// <summary>Returns a new Style instance with the Underline text style applied</summary>
    public Style Underline() => WithTextStyle((TextStyles ?? TextStyle.None) | TextStyle.Underline);

    /// <summary>Returns a new Style instance with the Inverse text style applied</summary>
    public Style Inverse() => WithTextStyle((TextStyles ?? TextStyle.None) | TextStyle.Inverse);

    /// <summary>Returns a new Style instance with the Dim text style applied</summary>
    public Style Dim() => WithTextStyle((TextStyles ?? TextStyle.None) | TextStyle.Dim);

    /// <summary>Returns a new Style instance with the Strikethrough text style applied</summary>
    public Style Strikethrough() => WithTextStyle((TextStyles ?? TextStyle.None) | TextStyle.Strikethrough);
}
