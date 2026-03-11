namespace PSFuzzySelect.App;

/// <summary>
/// Defines the various messages that can be sent from the UI to the main loop of the fuzzy selector.
/// These messages represent user actions such as changing the search query, moving the cursor, or selecting an item.
/// The main loop will process these messages to update the state of the fuzzy selector and re-render the UI accordingly.
/// </summary>
public abstract record Message;

/// <summary>
/// Message indicating that the user has pressed a key, containing the details of the key event (e.g., which key was pressed).
/// </summary>
/// <param name="Key">The key event information, including the key that was pressed and any modifier keys</param>
public record class KeyEvent(ConsoleKeyInfo Key) : Message;

/// <summary>
/// Message indicating that the search query has changed. Contains the new query string entered by the user
/// </summary>
/// <param name="Query">The new search query entered by the user</param>
public record class QueryChange(string Query) : Message;

/// <summary>
/// Message indicating that the highlighted item in the list of matches has changed, containing the index of the newly highlighted item
/// </summary>
/// <param name="Index">The index of the newly highlighted item</param>
public record class HighlightChange(int Index) : Message;

/// <summary>
/// Message indicating that the preview content should be updated, containing the new content to display
/// </summary>
/// <param name="Content">The new preview content to display</param>
public record class UpdatePreview(string Content) : Message;

/// <summary>
/// Message requesting that the Engine dispatch a preview generation for the specified item.
/// Emitted by the application when the highlighted item changes; intercepted by the Engine to dispatch to the PreviewWorker.
/// </summary>
/// <param name="Item">The item to generate a preview for, or null to clear the preview</param>
public record class RequestPreview(object? Item) : Message;

/// <summary>
/// Message indicating that the user has selected an item from the list of matches
/// </summary>
/// <param name="Item">The item that the user has selected</param>
public record class Select(object Item) : Message;

/// <summary>
/// Message indicating that the user has confirmed their selection, which can be used to trigger any final actions or exit the fuzzy selector
/// </summary>
public record class Confirm : Message;

/// <summary>
/// Message indicating that the console window has been resized, containing the new width and height of the console
/// </summary>
/// <param name="Width">The new width of the console window</param>
/// <param name="Height">The new height of the console window</param>
public record class Resize(int Width, int Height) : Message;

/// <summary>
/// Message indicating that the user has requested to quit the fuzzy selector without making a selection
/// </summary>
public record class Quit : Message;

/// <summary>
/// Message representing an item that can be matched in the fuzzy selector,
/// containing the original item and its display string used for matching and rendering in the UI
/// </summary>
/// <param name="Item">The original item</param>
/// <param name="Display">The display string used for matching and rendering</param>
public readonly record struct MatchableItem(object Item, string Display);

/// <summary>
/// Message carrying a batch of new items streamed from the pipeline.
/// Dispatched by the cmdlet thread and processed by the FuzzySelector on the engine thread.
/// </summary>
/// <param name="Items">The batch of newly arrived items.</param>
public record class ItemsAdded(MatchableItem[] Items) : Message;

/// <summary>
/// Message indicating that the pipeline has finished producing items.
/// After this message, no more <see cref="ItemsAdded"/> messages will arrive.
/// </summary>
public record class ItemsFinished : Message;

/// <summary>
/// Periodic engine tick used for time-based updates such as timers, debouncing, and animations.
/// </summary>
public record class FrameTick : Message;
