namespace PSFuzzySelect.UI.Surface;

public interface IComponent
{
    /// <summary>
    /// Renders the component onto the provided surface.
    /// The surface represents a portion of the terminal output where the component should draw itself.
    /// </summary>
    /// <param name="surface">The surface to render the component on</param>
    void Render(ISurface surface);
}
