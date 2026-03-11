using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI.Components.Text;

/// <summary>
/// Represents a span of text with an associated style that can be applied to characters drawn on a render surface.
/// Internally stores text as a memory slice to allow zero-allocation slicing when rendering.
/// </summary>
public readonly record struct TextSpan
{
    private readonly ReadOnlyMemory<char> _memory;

    /// <summary>Construct a span from a string.</summary>
    public TextSpan(string text, Style style)
    {
        _memory = text is null ? ReadOnlyMemory<char>.Empty : text.AsMemory();
        Style = style;
    }

    /// <summary>Construct a span from a memory slice.</summary>
    public TextSpan(ReadOnlyMemory<char> memory, Style style)
    {
        _memory = memory;
        Style = style;
    }

    /// <summary>The style applied to this span.</summary>
    public Style Style { get; init; }

    /// <summary>Access the underlying text as a memory slice (zero-allocation).</summary>
    public ReadOnlyMemory<char> Memory => _memory;

    /// <summary>Compatibility string accessor. Calling this allocates a string from the memory.</summary>
    public string Text => _memory.ToString();

    /// <summary>Length of the span in characters.</summary>
    public int Length => _memory.Length;

    /// <summary>Implicit conversion from string for convenience.</summary>
    public static implicit operator TextSpan(string text) => new(text, Style.Default);
}
