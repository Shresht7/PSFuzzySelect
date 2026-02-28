using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components.Box;

/// <summary>
/// Defines the style properties for a <see cref="Box"/> component, including margin, padding, border style, and text style
/// </summary>
public record BoxStyle
{
    /// <summary>The spacing outside the box borders</summary>
    public Spacing Margin { get; init; } = Spacing.None;


    /// <summary>The spacing inside the box borders around the content</summary>
    public Spacing Padding { get; init; } = Spacing.None;

    /// <summary>The style to apply to the box borders</summary>
    public Style BorderStyle { get; init; } = Style.Default;

    /// <summary>The style to apply to the background of the box</summary>
    public Style BackgroundStyle { get; init; } = Style.Default;

    /// <summary>The type of border to draw around the box</summary>
    public BorderStyle Border { get; init; } = Components.Box.BorderStyle.None;

    /// <summary>
    /// The default box style with no margin, no padding, the default text style, and no border
    /// </summary>
    public static BoxStyle Default => new();

    /// <summary>
    /// Creates a new <see cref="BoxStyle"/> with the specified margin, padding, text style, and border style.
    /// Any properties that are not specified will default to the values from the current style.
    /// </summary>
    /// <param name="margin">The margin to apply to the box</param>
    /// <returns>A new <see cref="BoxStyle"/> with the specified margin</returns>
    public BoxStyle WithMargin(Spacing margin) => this with { Margin = margin };

    /// <summary>
    /// Creates a new <see cref="BoxStyle"/> with the specified padding, text style, and border style.
    /// Any properties that are not specified will default to the values from the current style.
    /// </summary>
    /// <param name="padding">The padding to apply to the box</param>
    /// <returns>A new <see cref="BoxStyle"/> with the specified padding</returns
    public BoxStyle WithPadding(Spacing padding) => this with { Padding = padding };

    /// <summary>
    /// Creates a new <see cref="BoxStyle"/> with the specified text style.
    /// Any properties that are not specified will default to the values from the current style.
    /// </summary>
    /// <param name="style">The text style to apply to the box</param>
    /// <returns>A new <see cref="BoxStyle"/> with the specified text style</returns>
    public BoxStyle WithStyle(Style style) => this with { BorderStyle = style };

    /// <summary>
    /// Creates a new <see cref="BoxStyle"/> with the specified border style.
    /// Any properties that are not specified will default to the values from the current style.
    /// </summary>
    /// <param name="border">The border style to apply to the box</param>
    /// <returns>A new <see cref="BoxStyle"/> with the specified border style</returns>
    public BoxStyle WithBorder(BorderStyle border) => this with { Border = border };

    /// <summary>
    /// Creates a new <see cref="BoxStyle"/> with the specified background style.
    /// Any properties that are not specified will default to the values from the current style.
    /// </summary>
    /// <param name="background">The background style to apply to the box</param>
    /// <returns>A new <see cref="BoxStyle"/> with the specified background style</returns>
    public BoxStyle WithBackground(Style background) => this with { BackgroundStyle = background };
}
