using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Renderer;

public record BoxStyle
{
    /// <summary>The spacing outside the box borders</summary>
    public Spacing Margin { get; init; } = Spacing.None;


    /// <summary>The spacing inside the box borders around the content</summary>
    public Spacing Padding { get; init; } = Spacing.None;

    /// <summary>The style to apply to the box borders</summary>
    public Style Style { get; init; } = Style.Default;

    public static BoxStyle Default => new BoxStyle();

    public BoxStyle WithMargin(Spacing margin) => this with { Margin = margin };
    public BoxStyle WithPadding(Spacing padding) => this with { Padding = padding };
    public BoxStyle WithStyle(Style style) => this with { Style = style };
}
