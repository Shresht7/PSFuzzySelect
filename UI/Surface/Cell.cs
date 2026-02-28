using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Surface;

/// <summary>
/// Represents a single cell in the terminal output, containing a character and its associated styles
/// </summary>
/// <param name="Character">The character to be displayed in the cell</param>
/// <param name="Style">The style to apply to the character</param>
public readonly record struct Cell(char Character, Style Style)
{
    public static Cell Empty => new(' ', Style.Default);
}
