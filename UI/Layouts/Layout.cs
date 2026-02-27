using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Layouts;

public static class Layout
{
    public static Blueprint Vertical(params Section[] sections) => new Blueprint(sections, true);
    public static Blueprint Horizontal(params Section[] sections) => new Blueprint(sections, false);
}
