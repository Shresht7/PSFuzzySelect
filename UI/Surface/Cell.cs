using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Surface;

/// <summary>
/// Represents a single cell in the terminal output, containing a character and its associated styles
/// </summary>
/// <param name="character">The character to be displayed in the cell</param>
/// <param name="style">The style to apply to the character</param>
public readonly struct Cell(char character, Style style) : IEquatable<Cell>
{
    /// <summary>The character to be displayed in the cell</summary>
    public char Character { get; init; } = character;

    /// <summary>The style to apply to the character</summary>
    public Style Style { get; init; } = style;

    public static Cell Empty => new Cell(' ', Style.Default);

    // Equality members to allow comparison of Cell instances based on their content
    public bool Equals(Cell other) => Character == other.Character && Style == other.Style;
    public override bool Equals(object? obj) => obj is Cell cell && Equals(cell);
    public override int GetHashCode() => HashCode.Combine(Character, Style);

    // Equality operators for convenience
    public static bool operator ==(Cell left, Cell right) => left.Equals(right);
    public static bool operator !=(Cell left, Cell right) => !left.Equals(right);
}
