using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Box;

public class Box(IComponent component) : IComponent
{
    public BoxStyle Style { get; private set; } = BoxStyle.Default;

    public Box Border(BorderStyle border)
    {
        Style = Style.WithBorder(border);
        return this;
    }

    public Box Padding(Spacing padding)
    {
        Style = Style.WithPadding(padding);
        return this;
    }

    public Box Margin(Spacing margin)
    {
        Style = Style.WithMargin(margin);
        return this;
    }

    public void Render(ISurface surface)
    {
        // Apply Margin
        var borderRect = new Rect(0, 0, surface.Width, surface.Height)
            .Inset(Style.Margin.Left, Style.Margin.Top, Style.Margin.Right, Style.Margin.Bottom);

        // Apply Border
        if (borderRect.Width <= 0 || borderRect.Height <= 0) return; // Not enough space to render
        var borderSurface = surface.CreateSubSurface(borderRect);
        bool hasBorder = Style.Border != BorderStyle.None;
        if (hasBorder)
        {
            DrawBorder(borderSurface);
        }

        // Apply Padding
        int b = hasBorder ? 1 : 0;
        var contentRect = new Rect(0, 0, borderSurface.Width, borderSurface.Height)
            .Inset(b + Style.Padding.Left, b + Style.Padding.Top, b + Style.Padding.Right, b + Style.Padding.Bottom);

        // Render Content
        if (contentRect.Width <= 0 || contentRect.Height <= 0) return; // Not enough space to render
        var contentSurface = borderSurface.CreateSubSurface(contentRect);
        component.Render(contentSurface);
    }

    private void DrawBorder(ISurface surface)
    {
        var border = Style.Border;

        // Draw top and bottom borders
        for (int x = 1; x < surface.Width - 1; x++)
        {
            surface.Write(x, 0, border.Top);
            surface.Write(x, surface.Height - 1, border.Bottom);
        }

        // Draw left and right borders
        for (int y = 1; y < surface.Height - 1; y++)
        {
            surface.Write(0, y, border.Left);
            surface.Write(surface.Width - 1, y, border.Right);
        }

        // Draw corners
        surface.Write(0, 0, border.TopLeft);
        surface.Write(surface.Width - 1, 0, border.TopRight);
        surface.Write(0, surface.Height - 1, border.BottomLeft);
        surface.Write(surface.Width - 1, surface.Height - 1, border.BottomRight);
    }
}
