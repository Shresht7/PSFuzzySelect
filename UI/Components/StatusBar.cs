using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;

namespace PSFuzzySelect.UI.Components;

public class StatusBar(int matchCount, int cursor, bool isStreaming) : IComponent
{
    public void Render(ISurface surface)
    {
        new TextBlock($"{cursor + 1}/{matchCount}")
            .Add(isStreaming ? " (Streaming...)" : string.Empty)
            .Align(TextAlignment.Right)
            .Render(surface);
    }
}
