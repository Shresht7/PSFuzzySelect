using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Box;

/// <summary>Describes the style of a box</summary>
public readonly struct BorderStyle(
    char topLeft, char top, char topRight,
    char left, char right,
    char bottomLeft, char bottom, char bottomRight
) : IEquatable<BorderStyle>
{
    public char TopLeft { get; init; } = topLeft;
    public char Top { get; init; } = top;
    public char TopRight { get; init; } = topRight;
    public char Left { get; init; } = left;
    public char Right { get; init; } = right;
    public char BottomLeft { get; init; } = bottomLeft;
    public char Bottom { get; init; } = bottom;
    public char BottomRight { get; init; } = bottomRight;

    public static BorderStyle None => default;
    public static BorderStyle Single => new BorderStyle('┌', '─', '┐', '│', '│', '└', '─', '┘');
    public static BorderStyle Double => new BorderStyle('╔', '═', '╗', '║', '║', '╚', '═', '╝');
    public static BorderStyle Rounded => new BorderStyle('╭', '─', '╮', '│', '│', '╰', '─', '╯');
    public static BorderStyle Thick => new BorderStyle('┏', '━', '┓', '┃', '┃', '┗', '━', '┛');
    public static BorderStyle Hidden => new BorderStyle(' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');

    public bool Equals(BorderStyle other)
    {
        return TopLeft == other.TopLeft &&
               Top == other.Top &&
               TopRight == other.TopRight &&
               Left == other.Left &&
               Right == other.Right &&
               BottomLeft == other.BottomLeft &&
               Bottom == other.Bottom &&
               BottomRight == other.BottomRight;
    }

    public override bool Equals(object? obj) => obj is BorderStyle other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight);
    public static bool operator ==(BorderStyle left, BorderStyle right) => left.Equals(right);
    public static bool operator !=(BorderStyle left, BorderStyle right) => !left.Equals(right);
}

public record BoxStyle
{
    /// <summary>The spacing outside the box borders</summary>
    public Spacing Margin { get; init; } = Spacing.None;


    /// <summary>The spacing inside the box borders around the content</summary>
    public Spacing Padding { get; init; } = Spacing.None;

    /// <summary>The style to apply to the box borders</summary>
    public Style Style { get; init; } = Style.Default;

    /// <summary>The type of border to draw around the box</summary>
    public BorderStyle Border { get; init; } = BorderStyle.None;

    public static BoxStyle Default => new BoxStyle();

    public BoxStyle WithMargin(Spacing margin) => this with { Margin = margin };
    public BoxStyle WithPadding(Spacing padding) => this with { Padding = padding };
    public BoxStyle WithStyle(Style style) => this with { Style = style };
    public BoxStyle WithBorder(BorderStyle border) => this with { Border = border };
}
