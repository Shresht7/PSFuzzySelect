using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

public class StatusBar(int matchCount, int cursor, bool isStreaming) : IComponent
{
    public void Render(ISurface surface)
    {
        new Spinner(isStreaming, $"{cursor + 1} / {matchCount}")
            .Render(surface);
    }
}
