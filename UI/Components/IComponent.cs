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

    Message? Update(Message? message) => null;
}

public interface IInteractiveComponent : IComponent
{
    /// <summary>
    /// Handles a key press event and returns a message representing the user action.
    /// This method is called when the user presses a key while the component is focused, allowing the component to respond to user input.
    /// The returned message can be used to trigger state changes or actions in the main loop of the application.
    /// </summary>
    /// <param name="key">The key that was pressed by the user</param>
    /// <returns>A Message object representing the user action, or null if no action should be taken</returns>
    Message? HandleKey(ConsoleKeyInfo key);
}
