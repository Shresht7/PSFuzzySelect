using PSFuzzySelect.UI.Components.Box;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

public class Preview : IComponent
{
    private Paragraph _paragraph = new();

    public void SetContent(string content)
    {
        var nextParagraph = new Paragraph(content);
        _paragraph = nextParagraph;
    }

    public void AddLine(TextBlock text)
    {
        _paragraph.AddLine(text);
    }

    public void Render(ISurface surface)
    {
        new Box.Box(_paragraph).Border(BorderStyle.Single).Padding(1).Render(surface);
    }

    public void Scroll(int delta)
    {
        _paragraph.ScrollOffset = Math.Max(0, _paragraph.ScrollOffset + delta);
    }
}
