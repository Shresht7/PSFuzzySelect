using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

public interface IComponent
{
    /// <summary>
    /// Renders the component onto the provided surface.
    /// The surface represents a portion of the terminal output where the component should draw itself.
    /// </summary>
    /// <param name="surface">The surface to render the component on</param>
    void Render(ISurface surface);
}

public interface IInteractiveComponent : IComponent
{
    /// <summary>
    /// Updates the state of the component based on the received message, which can represent various
    /// user actions such as changing the search query, moving the cursor, selecting an item, or quitting the selector.
    /// </summary>
    /// <param name="message">The message representing a user action.</param>
    /// <returns>A Message object representing the result of the update, or null if no action should be taken</returns>
    Message? Update(Message? message);
}
