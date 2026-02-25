using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Components;

public class StatusBar
{
    public static void Render(IRenderSurface surface, int x, int y, int matchCount, int cursor)
    {
        surface.Write(x, y, $"[{cursor + 1}/{matchCount}]");
    }
}
