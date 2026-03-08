using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components.Text;

/// <summary>
/// A vertical text container that renders one <see cref="TextBlock"/> per line with optional scrolling.
/// </summary>
public class Paragraph : IComponent
{

    private readonly List<TextBlock> _lines = [];

    /// <summary>
    /// The index of the first visible line.
    /// </summary>
    public int ScrollOffset { get; set; } = 0;

    /// <summary>
    /// Creates a paragraph by splitting plain text into lines.
    /// </summary>
    /// <param name="content">Line content separated by CRLF, LF, or CR.</param>
    public Paragraph(string content)
    {
        _lines = content
            .Split(["\r\n", "\n", "\r"], System.StringSplitOptions.None)
            .Select(line => new TextBlock(line))
            .ToList();
    }

    /// <summary>
    /// Creates a paragraph from existing <see cref="TextBlock"/> lines.
    /// </summary>
    /// <param name="lines">Initial lines to render.</param>
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

    /// <summary>
    /// Clears all lines and resets scrolling.
    /// </summary>
    /// <returns>The current paragraph instance.</returns>
    public Paragraph Clear()
    {
        _lines.Clear();
        ScrollOffset = 0;
        return this;
    }

    /// <summary>
    /// Renders visible lines into the target surface based on <see cref="ScrollOffset"/>.
    /// </summary>
    /// <param name="surface">The target surface.</param>
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
