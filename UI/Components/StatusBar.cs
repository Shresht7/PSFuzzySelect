using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Layouts;

namespace PSFuzzySelect.UI.Components;

public class StatusBar(int matchCount, int cursor, bool isStreaming) : IComponent
{
    public void Render(ISurface surface)
    {
        new Spinner(isStreaming, $"{cursor + 1} / {matchCount}")
            .Render(surface);
    }
}
