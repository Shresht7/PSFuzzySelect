using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

public class Preview : IComponent
{
    public void Render(ISurface surface)
    {
        new TextBlock("Preview Pane").Render(surface);
    }
}
