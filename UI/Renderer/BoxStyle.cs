using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Renderer;

/// <summary>The kind of border to draw around a box</summary>
public enum BorderStyle
{
    None,
    Single,     // ┌───────────┐
    Double,     // ╔═══════════╗
    Rounded,    // ╭───────────╮
    Thick,      // ┏━━━━━━━━━━━┓
    Hidden,     // (no visible border. useful for reserving space for borders without actually drawing them)
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
