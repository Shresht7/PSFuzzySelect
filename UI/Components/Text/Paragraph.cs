using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components.Text;

public class Paragraph : IComponent
{

    private readonly List<TextBlock> _lines = [];
    public int ScrollOffset { get; set; } = 0;

    public Paragraph(string content)
    {
        _lines = content
            .Split(["\r\n", "\n", "\r"], System.StringSplitOptions.None)
            .Select(line => new TextBlock(line))
            .ToList();
    }

    public Paragraph(params TextBlock[] lines)
    {
        _lines = lines.ToList();
    }

    public Paragraph AddLine(string text)
    {
        _lines.Add(new TextBlock(text));
        return this;
    }

    public Paragraph AddLine(params TextSpan[] spans)
    {
        _lines.Add(new TextBlock(spans));
        return this;
    }

    public Paragraph AddLine(TextBlock line)
    {
        _lines.Add(line);
        return this;
    }

    public Paragraph Clear()
    {
        _lines.Clear();
        ScrollOffset = 0;
        return this;
    }

    public void Render(ISurface surface)
    {
        if (_lines.Count == 0) return;  // No lines to render

        for (int i = 0; i < surface.Height; i++)
        {
            int lineIndex = ScrollOffset + i;
            if (lineIndex >= _lines.Count) break;  // No more lines to render

            var lineSurface = surface.CreateSubSurface(new Rect(0, i, surface.Width, 1));
            _lines[lineIndex].Render(lineSurface);
        }
    }
}
