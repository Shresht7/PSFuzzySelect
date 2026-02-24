namespace PSFuzzySelect.Core;

public class MatchResult
{
    /// <summary>
    /// The original item that was matched.
    /// This can be any object that the fuzzy matcher is designed to work with,
    /// such as a string, a PSObject, or a custom data structure.
    /// </summary> 
    public object Item { get; }
    /// <summary>
    /// A string representation of the item that was used for display purposes.
    /// This is the string that will be shown to the user in the fuzzy selection interface.
    /// </summary>
    public string DisplayString { get; }
    /// <summary>
    /// The score of the match, typically a numerical value indicating how closely the item matches the search query
    /// </summary>
    public int Score { get; }
    /// <summary>
    /// The positions of the characters in the DisplayString that contributed to the match.
    /// This can be used for highlighting the matched characters in the UI.
    /// </summary>
    public int[] Positions { get; }

    /// <summary>Initializes a new instance of the MatchResult class</summary>
    /// <param name="item">The original item that was matched</param>
    /// <param name="displayString">A string representation of the item for display purposes</param>
    /// <param name="score">The score of the match</param>
    /// <param name="positions">The positions of the characters in the DisplayString that contributed to the match</param>
    public MatchResult(object item, string displayString, int score, int[] positions)
    {
        Item = item;
        DisplayString = displayString;
        Score = score;
        Positions = positions;
    }
}

public class FuzzyMatcher
{
    /// <summary>
    /// Performs a fuzzy match against a collection of items based on a search query.
    /// </summary>
    /// <param name="items">The collection of items to match against. Each item can be of any type, such as a string or PSObject.</param>
    /// <param name="query">The search query string that the user is trying to match.</param>
    /// <returns>A list of MatchResult objects representing the matched items, their display strings, scores, and matched positions.</returns>
    public List<MatchResult> Match(IEnumerable<(object item, string display)> items, string query)
    {
        // No query provided, return all items with a default score of 0
        if (string.IsNullOrWhiteSpace(query))
        {
            return items.Select(x => new MatchResult(x.item, x.display, 0, [])).ToList();
        }

        var results = new List<MatchResult>();
        var queryLower = query.ToLowerInvariant();

        foreach (var (item, display) in items)
        {
            var displayLower = display.ToLowerInvariant();
            var matchInfo = TryMatch(displayLower, queryLower);
            if (matchInfo.HasValue)
            {
                results.Add(new MatchResult(item, display, matchInfo.Value.score, matchInfo.Value.positions));
            }
        }

        // Sort by score descending, then by display string for consistent ordering
        return results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.DisplayString)
            .ToList();
    }

    /// <summary>
    /// Attempts to match the query against the provided text using a simple fuzzy matching algorithm.
    /// </summary>
    /// <param name="text">The text to match against.</param>
    /// <param name="query">The query string to match.</param>
    /// <returns>A tuple containing the score and positions of matched characters, or null if no match is found.</returns>
    private (int score, int[] positions)? TryMatch(string text, string query)
    {
        int score = 0, textIndex = 0, consecutiveMatches = 0;
        var positions = new List<int>();

        // Iterate through each character in the query and try to find it in the text in order
        foreach (char queryChar in query)
        {
            // Find next occurrence of the query character in the text, starting from the last matched position
            int foundIndex = text.IndexOf(queryChar, textIndex);
            if (foundIndex == -1)
            {
                return null; // Character not found, no match
            }

            // Character found, calculate score and store position

            // Add the position to the list
            positions.Add(foundIndex);

            // Bonus for matching at start of text
            if (foundIndex == 0)
            {
                score += 20;
            }

            // Bonus for consecutive matches
            if (foundIndex == textIndex && consecutiveMatches > 0)
            {
                // Consecutive Match Bonus
                score += 15;
                consecutiveMatches++;
            }
            else
            {
                consecutiveMatches = 1; // Reset consecutive match count
            }

            // TODO? Should do something about the magic numbers here, maybe make them parameters or constants?

            // Bonus for matching after word boundary (e.g. space, dash, underscore, camelCase)
            if (foundIndex > 0)
            {
                if (IsWordBoundary(text, foundIndex))
                {
                    score += 10;
                }
            }

            // Base Score (Earlier matches are better)
            score += Math.Max(0, 100 - foundIndex);

            textIndex = foundIndex + 1; // Move past the matched character for the next search
        }

        return (score, positions.ToArray());
    }

    /// <summary>
    /// Determines if the character at the given index in the text is a word boundary, which can be a space, separator, or camelCase transition.
    /// </summary>
    /// <param name="text">The text to check</param>
    /// <param name="index">The index of the character to check</param>
    /// <returns>True if the character at the given index is a word boundary; otherwise, false</returns>
    private static bool IsWordBoundary(string text, int index)
    {
        if (index == 0)
        {
            return true;
        }

        var curr = text[index];
        var prev = text[index - 1];

        bool isCamelCaseBoundary = char.IsLower(prev) && char.IsUpper(curr);
        bool isSeparator = curr == '-' || curr == '_' || curr == '/' || curr == '\\';
        return char.IsWhiteSpace(curr) || isSeparator || isCamelCaseBoundary;
    }
}
