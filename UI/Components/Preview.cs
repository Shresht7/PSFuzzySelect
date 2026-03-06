using PSFuzzySelect.UI.Components.Box;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

public class Preview : IComponent
{
    private Paragraph _paragraph = new();
    private string _content = string.Empty;

    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            _paragraph.Clear();
            _paragraph = new Paragraph(value);
        }
    }

    public void Render(ISurface surface)
    {
        new Box.Box(_paragraph).Border(BorderStyle.Single).Padding(1).Render(surface);
    }

    public void Scroll(int Delta)
    {
        _paragraph.ScrollOffset = Math.Max(0, _paragraph.ScrollOffset + Delta);
    }
}
