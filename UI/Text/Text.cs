using PSFuzzySelect.UI.Renderer;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Text;

/// <summary>
/// A text component that can render multiple spans of text with different styles on a render surface.
/// </summary>
public class Text : IComponent
{
    /// <summary>
    /// The list of spans that make up the text content of this component.
    /// Each span has its own text and style, allowing for rich text rendering with multiple styles in a single text component.
    /// </summary>
    private readonly List<TextSpan> _spans = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class with no spans.
    /// This allows you to create an empty text component and add spans to it later using the <see cref="Add"/> method.
    /// If you want to initialize the text component with spans, use the constructor that takes a params array of <see cref="TextSpan"/> or the constructor that takes a single string.
    /// </summary>
    public Text() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class with the provided spans
    /// </summary>
    /// <param name="spans">The spans to initialize the text component with</param>
    public Text(params TextSpan[] spans)
    {
        _spans.AddRange(spans);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class with a single span containing the provided text and the default style
    /// </summary>
    /// <param name="Text">The text content for the span</param>
    public Text(string Text) : this(new TextSpan(Text, Style.Default)) { }

    /// <summary>
    /// Adds a span to the text component.
    /// Spans are rendered in the order they are added, so adding a span will cause it to be rendered after all previously added spans.
    /// </summary>
    /// <param name="span">The span to add</param>
    /// <returns>The current <see cref="Text"/> instance</returns>
    public Text Add(TextSpan span)
    {
        _spans.Add(span);
        return this;
    }

    public void Render(ISurface surface)
    {
        int x = 0, y = 0;
        foreach (var span in _spans)
        {
            surface.Write(x, y, span.Text, span.Style);
            x += span.Length; // Advance the x position based on the length of the span's text

            // If the x position exceeds the surface width, stop rendering this line
            if (x >= surface.Width) break;
        }
    }
}
