/// <summary>
/// Defines the various messages that can be sent from the UI to the main loop of the fuzzy selector.
/// These messages represent user actions such as changing the search query, moving the cursor, or selecting an item.
/// The main loop will process these messages to update the state of the fuzzy selector and re-render the UI accordingly.
/// </summary>
public abstract record Message;

/// <summary>
/// Message indicating that the search query has changed. Contains the new query string entered by the user
/// </summary>
/// <param name="Query">The new search query entered by the user</param>
record class QueryChange(string Query) : Message;

/// <summary>
/// Message indicating that the user has moved the cursor up or down in the list of matches
/// </summary> 
/// <param name="Delta">The amount to move the cursor. A positive value indicates moving down, a negative value indicates moving up</param>
record class CursorMove(int Delta) : Message;

/// <summary>
/// Message indicating that the user has selected an item from the list of matches
/// </summary>
record class Select : Message;

/// <summary>
/// Message indicating that the user has requested to quit the fuzzy selector without making a selection
/// </summary>
record class Quit : Message;
