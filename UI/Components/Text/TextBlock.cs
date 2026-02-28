using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components.Text;

/// <summary>
/// A text component that can render multiple spans of text with different styles on a render surface.
/// </summary>
public class TextBlock : IComponent
{
    /// <summary>
    /// The list of spans that make up the text content of this component.
    /// Each span has its own text and style, allowing for rich text rendering with multiple styles in a single text component.
    /// </summary>
    private readonly List<TextSpan> _spans = [];

    /// <summary>
    /// The horizontal alignment of the text within its allocated surface.
    /// This determines how the text is positioned when rendered on a surface that is wider than the total length of the text spans.
    /// </summary>
    private TextAlignment _alignment = TextAlignment.Left;

    public TextBlock Align(TextAlignment alignment)
    {
        _alignment = alignment;
        return this;
    }

    private TextOverflow _overflow = TextOverflow.Clip;

    public TextBlock Overflow(TextOverflow overflow)
    {
        _overflow = overflow;
        return this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class with no spans.
    /// This allows you to create an empty text component and add spans to it later using the <see cref="Add"/> method.
    /// If you want to initialize the text component with spans, use the constructor that takes a params array of <see cref="TextSpan"/> or the constructor that takes a single string.
    /// </summary>
    public TextBlock() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class with the provided spans
    /// </summary>
    /// <param name="spans">The spans to initialize the text component with</param>
    public TextBlock(params TextSpan[] spans)
    {
        _spans.AddRange(spans);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class with a single span containing the provided text and the default style
    /// </summary>
    /// <param name="Text">The text content for the span</param>
    public TextBlock(string Text) : this(new TextSpan(Text, Style.Default)) { }

    /// <summary>
    /// Adds a span to the text component.
    /// Spans are rendered in the order they are added, so adding a span will cause it to be rendered after all previously added spans.
    /// </summary>
    /// <param name="span">The span to add</param>
    /// <returns>The current <see cref="TextBlock"/> instance</returns>
    public TextBlock Add(TextSpan span)
    {
        _spans.Add(span);
        return this;
    }

    /// <summary>
    /// Adds multiple spans to the text component.
    /// This is a convenient method for adding multiple spans at once, instead of calling <see cref="Add"/> multiple times.
    /// </summary>
    /// <param name="spans">The spans to add</param>
    /// <returns>The current <see cref="TextBlock"/> instance</returns>
    public TextBlock AddRange(IEnumerable<TextSpan> spans)
    {
        _spans.AddRange(spans);
        return this;
    }

    public void Render(ISurface surface)
    {
        // Calculate the total width to determine the starting x position
        int totalContentWidth = _spans.Sum(span => span.Length);

        // Determine if the text exceeds the available space on the surface
        bool isOverflowing = totalContentWidth > surface.Width;

        // If the text is overflowing and the overflow behavior is set to Ellipsis, reserve space for the ellipsis character
        int totalWidth = isOverflowing && _overflow == TextOverflow.Ellipsis
            ? Math.Max(0, surface.Width - 1) // Reserve space for the ellipsis
            : totalContentWidth;

        // Determine the starting x position based on alignment
        int x = _alignment switch
        {
            TextAlignment.Left => 0,
            TextAlignment.Center => Math.Max(0, (surface.Width - totalWidth) / 2),
            TextAlignment.Right => Math.Max(0, surface.Width - totalWidth),
            _ => 0,
        };
        int y = 0; // Vertical alignment is usually handled by layout containers

        foreach (var span in _spans)
        {
            surface.Write(x, y, span.Text, span.Style);
            x += span.Length; // Advance the x position based on the length of the span's text

            // If the x position exceeds the surface width, stop rendering this line
            if (x >= surface.Width) break;
        }

        // Handle overflow behavior
        if (isOverflowing && _overflow == TextOverflow.Ellipsis)
        {
            // Place the ellipsis at the end of the actually rendered text, not always at the far right
            int ellipsisX = Math.Max(0, Math.Min(surface.Width - 1, x - 1));
            surface.Write(ellipsisX, y, "…", Style.Default);
        }
    }
}
