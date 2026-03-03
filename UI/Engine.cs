using PSFuzzySelect.UI.Renderer;
using PSFuzzySelect.UI.Components;
using PSFuzzySelect.UI.Styles;

namespace PSFuzzySelect.UI;

/// <summary>
/// Defines the contract for an interactive application that can be rendered on the console and can update its state based on user interactions.
/// </summary>
public interface IApplication : IComponent
{
    /// <summary>
    /// Updates the state of the application based on the received message.
    /// This method is called by the Engine to process user actions and internal notifications.
    /// </summary>
    /// <param name="message">The message representing an action or event.</param>
    /// <returns>A follow-up Message object to be processed in the same frame, or null if processing is complete.</returns>
    Message? Update(Message? message);
}

/// <summary>
/// The Engine class is responsible for managing the main loop of the fuzzy selector application,
/// including rendering the UI components, handling user input, and updating the state of the application based on user interactions.
/// </summary>
/// <param name="App">The root component of the application, which will be rendered and updated by the engine</param>
public class Engine(IApplication App)
{
    /// <summary>The renderer responsible for drawing the UI components of the fuzzy selector application on the console</summary>
    private readonly ConsoleRenderer _renderer = new(Console.WindowWidth, Console.WindowHeight);

    /// <summary>A flag indicating whether the fuzzy selector should quit</summary>
    private bool _shouldQuit = false;

    /// <summary>
    /// Performs initial setup for the console, such as hiding the cursor and entering the alternate screen buffer
    /// to prepare for rendering the UI components of the fuzzy selector application.
    /// </summary>
    private static void Setup()
    {
        string ansi = string.Concat([
            Ansi.AltBufferEnter,      // Switch to the alternate screen buffer
            Ansi.CursorHide,          // Hide the cursor for a cleaner UI experience
            Ansi.ClearScreen,         // Ensure the alternate buffer is completely clear
            Ansi.CursorPosition(0, 0) // Reset cursor to the top-left corner
        ]);
        Console.Write(ansi);          // Write the ANSI escape codes to the console to apply the setup
    }

    /// <summary>
    /// Runs the main loop of the fuzzy selector application, which includes rendering the UI components, capturing user input,
    /// and updating the state of the application based on the captured input until the user decides to quit the application.
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
        // Initialize a variable to hold the captured message, which will be set when a user input is detected
        Message? message = null;

        // Event Loop: Continuously check for user input until a message is captured.
        while (message == null)
        {
            // Check if a key is available in the input buffer. If so, read it and create a KeyEvent message.
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                message = new KeyEvent(key);
            }

            // If no key is available, we can briefly sleep to avoid busy-waiting and reduce CPU usage while waiting for input
            if (message == null) Thread.Sleep(16);
        }

        return message; // Return the captured message, or null if no input was captured
    }

    /// <summary>
    /// Updates the state of the application based on the received message.
    /// It processes messages in a loop, feeding returned messages back into the application 
    /// until no more messages are produced or a Quit command is received.
    /// </summary>
    /// <param name="message">The initial message representing a user action or event.</param>
    private void Update(Message? message)
    {
        // Process messages until the application returns null or requests to quit
        while (message != null)
        {
            // Check if the message is a Quit command, and if so, set the quit flag and exit the loop
            if (message is Quit)
            {
                Quit();
                break;
            }

            // Let the application process the message and return any follow-up message to be processed in the same frame
            message = App.Update(message);
        }
    }

    /// <summary>
    /// Renders the entire UI by delegating to the root component, which will recursively render
    /// itself and its child components onto the renderer's back buffer, and then instructing the renderer to draw the buffer to the console.
    /// This method is called on each iteration of the main loop to update the display based on the current state of the UI components.
    /// </summary>
    private void Render()
    {
        var buffer = _renderer.GetBackBuffer();     // Get a fresh buffer for rendering the current frame
        App.Render(buffer);                        // Render the root component (and its children) to the buffer surface
        _renderer.Render();                         // Render the current buffer to the console
    }

    /// <summary>
    /// Cleans up the console UI by making the cursor visible, resetting the console colors, and exiting the alternate screen buffer.
    /// </summary>
    private static void Cleanup()
    {
        string ansi = string.Concat([
             Ansi.Reset,          // Reset console colors and styles to default
             Ansi.CursorShow,     // Ensure the cursor is visible again when exiting
             Ansi.AltBufferExit   // Switch back to the main screen buffer, restoring the previous screen state
        ]);
        Console.Write(ansi);      // Write the ANSI escape codes to the console to apply the cleanup
    }

    /// <summary>
    /// Sets the flag to quit the application, which will cause the main loop to exit and the <see cref="Run"/> method to return
    /// </summary>
    private void Quit()
    {
        _shouldQuit = true;
    }
}
