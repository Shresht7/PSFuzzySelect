using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Layouts;

public class Section()
{
    public BoxStyle Style { get; private set; } = BoxStyle.Default;

    public Section Border(BorderStyle border)
    {
        Style = Style.WithBorder(border);
        return this;
    }

    public Section Padding(Spacing padding)
    {
        Style = Style.WithPadding(padding);
        return this;
    }

    public Section Margin(Spacing margin)
    {
        Style = Style.WithMargin(margin);
        return this;
    }

    public void Render(ISurface surface, IComponent content)
    {
        // For now, just pass-through
        // TODO: Apply border, padding, and margin to the rect before rendering the content
        content.Render(surface);
    }
}
