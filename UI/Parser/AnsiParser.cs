using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Parser;

/// <summary>
/// Provides methods for parsing ANSI escape sequences and converting them into styled UI components.
/// </summary>
public static class AnsiParser
{
    /// <summary>
    /// Parses a string containing ANSI escape sequences into a list of styled TextSpans.
    /// This allows strings with embedded formatting to be correctly measured and rendered
    /// within the UI framework.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <param name="baseStyle">The starting style to apply.</param>
    /// <returns>A list of TextSpan objects representing the styled text segments.</returns>
    public static List<TextSpan> Parse(string text, Style baseStyle)
    {
        var result = new List<TextSpan>();
        if (string.IsNullOrEmpty(text)) return result;

        var currentStyle = baseStyle;
        int lastIndex = 0;
        int i = 0;

        while (i < text.Length)
        {
            if (text[i] == '\x1b' && i + 1 < text.Length && text[i + 1] == '[')
            {
                // Found an escape sequence. 
                // First, yield the text before the sequence with the current style.
                if (i > lastIndex)
                {
                    result.Add(new TextSpan(text.AsMemory(lastIndex, i - lastIndex), currentStyle));
                }

                // Parse the CSI sequence
                i += 2; // Skip \x1b[
                int paramsStart = i;
                while (i < text.Length && text[i] >= '0' && text[i] <= '?') i++; // CSI parameters
                while (i < text.Length && text[i] >= ' ' && text[i] <= '/') i++; // CSI intermediate bytes

                if (i < text.Length)
                {
                    char finalChar = text[i];
                    i++; // Move past final character

                    if (finalChar == 'm') // SGR (Select Graphic Rendition)
                    {
                        string sequence = text.Substring(paramsStart, i - 1 - paramsStart);
                        currentStyle = UpdateStyle(currentStyle, sequence);
                    }
                }
                lastIndex = i;
            }
            else
            {
                i++;
            }
        }

        // Add remaining text
        if (lastIndex < text.Length)
        {
            result.Add(new TextSpan(text.AsMemory(lastIndex, text.Length - lastIndex), currentStyle));
        }

        return result;
    }

    /// <summary>
    /// Updates the current style based on the provided ANSI sequence.
    /// </summary>
    /// <param name="current">The current style before applying the sequence.</param>
    /// <param name="sequence">The ANSI sequence to apply.</param>
    /// <returns>The updated style after applying the sequence.</returns>
    private static Style UpdateStyle(Style current, string sequence)
    {
        if (string.IsNullOrEmpty(sequence) || sequence == "0") return Style.Default;

        var parts = sequence.Split(';');
        var style = current;

        foreach (var part in parts)
        {
            if (int.TryParse(part, out int code))
            {
                style = ApplySgrCode(style, code);
            }
        }
        return style;
    }

    /// <summary>
    /// Applies a single SGR code to the given style and returns the updated style.
    /// </summary>
    /// <param name="style">The current style before applying the SGR code.</param>
    /// <param name="code">The SGR code to apply.</param>
    /// <returns>The updated style after applying the SGR code.</returns>
    private static Style ApplySgrCode(Style style, int code)
    {
        return code switch
        {
            0 => Style.Default,
            1 => style.Bold(),
            2 => style.Dim(),
            3 => style.Italic(),
            4 => style.Underline(),
            7 => style.Inverse(),
            9 => style.Strikethrough(),
            22 => style.WithTextStyle((style.TextStyles ?? TextStyle.None) & ~(TextStyle.Bold | TextStyle.Dim)),
            23 => style.WithTextStyle((style.TextStyles ?? TextStyle.None) & ~TextStyle.Italic),
            24 => style.WithTextStyle((style.TextStyles ?? TextStyle.None) & ~TextStyle.Underline),
            27 => style.WithTextStyle((style.TextStyles ?? TextStyle.None) & ~TextStyle.Inverse),
            29 => style.WithTextStyle((style.TextStyles ?? TextStyle.None) & ~TextStyle.Strikethrough),
            >= 30 and <= 37 => style.WithForeground((Color)code),
            39 => style.WithForeground(null),
            >= 40 and <= 47 => style.WithBackground((Color)(code - 10)),
            49 => style.WithBackground(null),
            >= 90 and <= 97 => style.WithForeground((Color)code),
            >= 100 and <= 107 => style.WithBackground((Color)(code - 10)),
            _ => style,
        };
    }
}
