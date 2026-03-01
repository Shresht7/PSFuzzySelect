using PSFuzzySelect.UI.Renderer;
using PSFuzzySelect.UI.Components;

namespace PSFuzzySelect.UI;

/// <summary>
/// The Engine class is responsible for managing the main loop of the fuzzy selector application,
/// including rendering the UI components, handling user input, and updating the state of the application based on user interactions.
/// </summary>
/// <param name="Root">The root interactive component of the UI, which will handle input and update its state based on user actions</param>
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
    private static void Setup()
    {
        Console.CursorVisible = false;  // Hide the cursor for a cleaner UI experience
        Console.Clear();                // Clear the console to prepare for the first-paint
    }

    /// <summary>
    /// Runs the main loop of the fuzzy selector, which continuously renders the UI,
    /// handles user input, and updates the state of the fuzzy selector until the user decides to quit (e.g., by pressing the Escape key).
    /// </summary>
    public void Run()
    {
        Setup(); // Perform initial setup for the console

        try
        {
            // The Main Loop of the Application
            while (!_shouldQuit)
            {
                Render();                           // Render the current state of the UI components to the console
                var message = CaptureEvents();      // Handle user input and get the resulting message representing the user action
                Update(message);                    // Update the state of the fuzzy selector based on the message
            }
        }
        finally
        {
            Cleanup();  // Clean up the console UI when exiting
        }
    }

    /// <summary>
    /// Captures user input from the console and translates it into a Message that can be processed
    /// by the main loop to update the state of the fuzzy selector.
    /// This method is called on each iteration of the main loop after rendering to handle user interactions.
    /// </summary>
    private static Message? CaptureEvents()
    {
        // Handle User Input
        var key = Console.ReadKey(intercept: true);

        // Exit on Escape key
        if (key.Key == ConsoleKey.Escape)
        {
            return new Quit();
        }

        return new KeyEvent(key); // Wrap the raw key press event in a KeyEvent
    }

    /// <summary>
    /// Updates the state of the fuzzy selector based on the received message, which can represent various
    /// user actions such as changing the search query, moving the cursor, selecting an item, or quitting the selector.
    /// This method is called after handling user input to process the resulting message and update the state of the UI components accordingly.
    /// </summary>
    /// <param name="message">The message representing a user action, which will be processed to update the state of the fuzzy selector</param>
    private void Update(Message? message)
    {
        if (message == null) return; // No message to process, skip the update step

        if (message is Quit)
        {
            Quit(); // Set the flag to quit the fuzzy selector
            return;
        }

        // Let the root component (and its children) process the message to update their state as needed
        Root.Update(message);
    }

    /// <summary>
    /// Renders the entire UI by delegating to the root component, which will recursively render
    /// itself and its child components onto the renderer's back buffer, and then instructing the renderer to draw the buffer to the console.
    /// This method is called on each iteration of the main loop to update the display based on the current state of the UI components.
    /// </summary>
    private void Render()
    {
        var buffer = _renderer.GetBackBuffer();     // Get a fresh buffer for rendering the current frame
        Root.Render(buffer);                        // Render the root component (and its children) to the buffer surface
        _renderer.Render();                         // Render the current buffer to the console
    }

    /// <summary>
    /// Cleans up the console UI by making the cursor visible, resetting the console colors, and clearing any residual UI elements
    /// </summary>
    private static void Cleanup()
    {
        Console.ResetColor();           // Reset console colors to default
        Console.CursorVisible = true;   // Ensure the cursor is visible again when exiting
        Console.Clear();                // Clear the console to remove any residual UI elements
    }

    /// <summary>
    /// Sets the flag to quit the application, which will cause the main loop to exit and the <see cref="Run"/> method to return
    /// </summary>
    private void Quit()
    {
        _shouldQuit = true;
    }
}
