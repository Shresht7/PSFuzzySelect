namespace PSFuzzySelect.UI.Text;

/// <summary>
/// Defines the behavior for handling text that exceeds the available space when rendering a <see cref="Text"/> component on a surface
/// </summary>
public enum TextOverflow
{
    /// <summary>
    /// The default overflow behavior where text that exceeds the available space is simply truncated and not rendered.
    /// This means that any characters that do not fit within the allocated surface will be discarded and not visible to the user.
    /// </summary>
    Clip,

    /// <summary>
    /// An overflow behavior where text that exceeds the available space is truncated and an ellipsis ("...") is appended to indicate that there is more text that is not being displayed.
    /// This provides a visual cue to the user that there is additional content that is not currently visible, while still fitting within the allocated surface.
    /// </summary>
    Ellipsis
}
