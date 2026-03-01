using PSFuzzySelect.UI.Renderer;
using PSFuzzySelect.UI.Components;

namespace PSFuzzySelect.UI;

public class Engine(IInteractiveComponent Root)
{
    /// <summary>The renderer responsible for drawing the UI components of the fuzzy selector on the console</summary>
    private readonly ConsoleRenderer _renderer = new(Console.WindowWidth, Console.WindowHeight);

    /// <summary>A flag indicating whether the fuzzy selector should quit</summary>
    private bool _shouldQuit = false;

    /// <summary>
    /// Sets up the console UI for the fuzzy selector, including hiding the cursor and preparing any necessary state before entering the main loop.
    /// This method is called once at the beginning of the Show() method to ensure that the console is in the correct state for rendering the fuzzy selector interface.
    /// </summary>
    private void Setup()
    {
        Console.CursorVisible = false;  // Hide the cursor for a cleaner UI experience
        Console.Clear();                // Clear the console to prepare for the first-paint
    }

    public void Run()
    {
        Setup(); // Perform initial setup for the console

        try
        {
            while (!_shouldQuit)
            {
                Render();
                var msg = HandleInput();
                Update(msg);
            }
        }
        finally
        {
            Cleanup();  // Clean up the console UI when exiting
        }
    }

    private Message? HandleInput()
    {
        // Handle User Input
        var key = Console.ReadKey(intercept: true);

        // Exit on Escape key
        if (key.Key == ConsoleKey.Escape)
        {
            return new Quit();
        }
        // Delegate key handling to the root component, which will propagate it to its children as needed   
        return Root.HandleKey(key);
    }

    private void Update(Message? message)
    {
        if (message == null) return; // No message to process, do nothing

        if (message is Quit)
        {
            Quit(); // Set the flag to quit the fuzzy selector
            return;
        }

        // Let the root component (and its children) process the message to update their state as needed
        Root.Update(message);
    }

    private void Render()
    {
        var buffer = _renderer.GetBackBuffer();     // Get a fresh buffer for rendering the current frame
        Root.Render(buffer);                        // Render the root component (and its children) to the buffer
        _renderer.Render();                         // Render the current buffer to the console
    }

    /// <summary>
    /// Cleans up the console UI by making the cursor visible, resetting the console colors, and clearing any residual UI elements
    /// </summary>
    private void Cleanup()
    {
        Console.ResetColor();           // Reset console colors to default
        Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
        Console.Clear();                // Clear the console to remove any residual UI elements
    }

    /// <summary>
    /// Sets the flag to quit the fuzzy selector, which will cause the main loop to exit and the Show() method to return.
    /// </summary>
    private void Quit()
    {
        _shouldQuit = true;
    }
}
