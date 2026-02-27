using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Layouts;

public static class Layout
{
    public static Blueprint Vertical(params Size[] sizes) => new Blueprint(sizes, true);
    public static Blueprint Horizontal(params Size[] sizes) => new Blueprint(sizes, false);
}
