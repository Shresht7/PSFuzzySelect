using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Components;

public class StatusBar
{
    public static void Render(IRenderSurface surface, int matchCount, int cursor)
    {
        surface.Write(0, 0, $"[{cursor + 1}/{matchCount}]");
    }
}
