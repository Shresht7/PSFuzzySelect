using PSFuzzySelect.UI.Surface;
using PSFuzzySelect.UI.Components.Text;

namespace PSFuzzySelect.UI.Components;

public class StatusBar(int matchCount, int cursor) : IComponent
{
    public void Render(ISurface surface)
    {
        new TextBlock($"{cursor + 1}/{matchCount}")
            .Align(TextAlignment.Right)
            .Render(surface);
    }
}
