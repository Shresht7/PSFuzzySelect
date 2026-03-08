using PSFuzzySelect.UI.Components.Box;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

/// <summary>
/// Renders and manages the preview pane content.
/// </summary>
public class Preview : IComponent
{
    private Paragraph _paragraph = new();

    /// <summary>
    /// Replaces the preview content with the provided multi-line text.
    /// </summary>
    /// <param name="content">Text to display in the preview panel.</param>
    public void SetContent(string content)
    {
        var nextParagraph = new Paragraph(content);
        _paragraph = nextParagraph;
    }

    /// <summary>
    /// Appends a rendered line to the preview content.
    /// </summary>
    /// <param name="text">A pre-styled text line.</param>
    public void AddLine(TextBlock text)
    {
        _paragraph.AddLine(text);
    }

    /// <summary>
    /// Renders the preview inside a bordered box with padding.
    /// </summary>
    /// <param name="surface">The target surface.</param>
    public void Render(ISurface surface)
    {
        new Box.Box(_paragraph).Border(BorderStyle.Single).Padding(1).Render(surface);
    }

    /// <summary>
    /// Adjusts the preview scroll offset.
    /// </summary>
    /// <param name="delta">Positive values scroll down; negative values scroll up.</param>
    public void Scroll(int delta)
    {
        _paragraph.ScrollOffset = Math.Max(0, _paragraph.ScrollOffset + delta);
    }
}
