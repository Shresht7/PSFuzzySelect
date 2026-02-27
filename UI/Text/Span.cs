using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Text;

/// <summary>
/// Represents a span of text with an associated style that can be applied to characters drawn on a render surface
/// </summary>
/// <param name="Text">The text content of the span</param>
/// <param name="Style">The style to apply to the text in the span</param>
public readonly record struct TextSpan(string Text, Style Style)
{
    /// <summary>
    /// Implicitly convert a <see langword="string"/> to a <see cref="TextSpan"/> with the default style.
    /// This allows you to pass in a <see langword="string"/> where a <see cref="TextSpan"/> is expected.
    /// </summary>
    /// <param name="text">The text to convert to a <see cref="TextSpan"/> with the default style</param>
    public static implicit operator TextSpan(string text) => new(text, Style.Default);

    /// <summary>The length of the text in the span, which is used for layout calculations when rendering the span on a surface</summary>
    public int Length => Text.Length;
}
