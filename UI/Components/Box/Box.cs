using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components.Box;

/// <summary>
/// A container component that can wrap any other component and apply margin, padding, and borders around it.
/// The box will render the wrapped component within the available space after accounting for the specified margin, padding, and border styles.
/// </summary>
/// <param name="component">The component to be wrapped inside the box</param>
public class Box(IComponent component) : IComponent
{
    /// <summary>
    /// The style properties for the box, including margin, padding, border style, and text style.
    /// </summary>
    public BoxStyle Style { get; private set; } = BoxStyle.Default;


    /// <summary>
    /// Sets the border style for the box. This will determine the characters used to draw the borders around the box when rendered on a surface.
    /// </summary>
    /// <param name="border">The border style to apply to the box</param>
    /// <returns>The current <see cref="Box"/> instance with the updated border style</returns>
    public Box Border(BorderStyle border)
    {
        Style = Style.WithBorder(border);
        return this;
    }

    /// <summary>
    /// Sets the padding for the box. This will determine the spacing inside the box borders around the content.
    /// </summary>
    /// <param name="padding">The padding to apply to the box</param>
    /// <returns>The current <see cref="Box"/> instance with the updated padding</returns>
    public Box Padding(Spacing padding)
    {
        Style = Style.WithPadding(padding);
        return this;
    }

    /// <summary>
    /// Sets the margin for the box. This will determine the spacing outside the box borders.
    /// </summary>
    /// <param name="margin">The margin to apply to the box</param>
    /// <returns>The current <see cref="Box"/> instance with the updated margin</returns>
    public Box Margin(Spacing margin)
    {
        Style = Style.WithMargin(margin);
        return this;
    }

    /// <summary>
    /// Renders the box on the provided surface, applying the specified margin, padding, and border styles around the wrapped component.
    /// </summary>
    /// <param name="surface">The surface on which to render the box</param>
    public void Render(ISurface surface)
    {
        // Apply Margin
        var borderRect = surface.Area.Inset(Style.Margin);

        // Apply Border
        if (borderRect.IsEmpty) return; // Not enough space to render
        var borderSurface = surface.CreateSubSurface(borderRect);
        bool hasBorder = Style.Border != BorderStyle.None;
        if (hasBorder)
        {
            DrawBorder(borderSurface);
        }

        // Apply Padding
        int borderOffset = hasBorder ? 1 : 0;
        Spacing innerOffset = borderOffset + Style.Padding;
        var contentRect = borderSurface.Area.Inset(innerOffset);

        // Render Content
        if (contentRect.IsEmpty) return; // Not enough space to render
        var contentSurface = borderSurface.CreateSubSurface(contentRect);
        component.Render(contentSurface);
    }

    private void DrawBorder(ISurface surface)
    {
        var border = Style.Border;

        // Draw top and bottom borders
        for (int x = 1; x < surface.Width - 1; x++)
        {
            surface.Write(x, 0, new Cell(border.Top, Style.Style));
            surface.Write(x, surface.Height - 1, new Cell(border.Bottom, Style.Style));
        }

        // Draw left and right borders
        for (int y = 1; y < surface.Height - 1; y++)
        {
            surface.Write(0, y, new Cell(border.Left, Style.Style));
            surface.Write(surface.Width - 1, y, new Cell(border.Right, Style.Style));
        }

        // Draw corners
        surface.Write(0, 0, new Cell(border.TopLeft, Style.Style));
        surface.Write(surface.Width - 1, 0, new Cell(border.TopRight, Style.Style));
        surface.Write(0, surface.Height - 1, new Cell(border.BottomLeft, Style.Style));
        surface.Write(surface.Width - 1, surface.Height - 1, new Cell(border.BottomRight, Style.Style));
    }
}
