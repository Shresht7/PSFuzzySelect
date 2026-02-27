using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Components;

public class StatusBar(int matchCount, int cursor) : IComponent
{
    public void Render(ISurface surface)
    {
        surface.Write(0, 0, $"[{cursor + 1}/{matchCount}]");
    }
}
