using PSFuzzySelect.App;

namespace PSFuzzySelect.Core;

/// <summary>
/// Represents the result of a fuzzy match, including the original item, its display string, the calculated score, and the positions of matched characters in the display string.
/// </summary>
/// <param name="Item">The original item that was matched</param>
/// <param name="DisplayString">The string representation of the item used for matching</param>
/// <param name="Score">The calculated score of the match</param>
/// <param name="Positions">The positions of matched characters in the display string</param>
public readonly record struct MatchResult(object Item, string DisplayString, int Score, int[] Positions);

public class FuzzyMatcher
{
    /// <summary>
    /// Performs a fuzzy match against a collection of items based on a search query.
    /// </summary>
    /// <param name="items">The collection of items to match against. Each item can be of any type, such as a string or PSObject.</param>
    /// <param name="query">The search query string that the user is trying to match.</param>
    /// <returns>A list of MatchResult objects representing the matched items, their display strings, scores, and matched positions.</returns>
    public static List<MatchResult> Match(List<MatchableItem> items, string query)
    {
        var results = new List<MatchResult>(items.Count);

        // No query provided, return all items with a default score of 0
        if (string.IsNullOrWhiteSpace(query))
        {
            for (int i = 0; i < items.Count; i++)
            {
                results.Add(new MatchResult(items[i], string.Empty, 0, Array.Empty<int>()));
            }
            return results;
        }

        var queryLower = query.ToLowerInvariant();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var display = item.Display ?? item.ToString() ?? string.Empty;
            var matchInfo = TryMatch(display, queryLower);
            if (matchInfo.HasValue)
            {
                results.Add(new MatchResult(item, display, matchInfo.Value.score, matchInfo.Value.positions));
            }
        }

        // Sort by score descending, then by display string for consistent ordering
        results.Sort((a, b) =>
        {
            int cmp = b.Score.CompareTo(a.Score);
            return cmp != 0 ? cmp : string.Compare(a.DisplayString, b.DisplayString, StringComparison.Ordinal);
        });
        return results;
    }

    /// <summary>
    /// Performs an incremental fuzzy match by taking existing matches and new items, matching only the new items against the query, and merging the results while maintaining order and performance.
    /// </summary>
    /// <param name="existingMatches">The list of existing match results.</param>
    /// <param name="newItems">The list of new items to match against the query.</param>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of MatchResult objects representing the merged match results.</returns>
    public static List<MatchResult> MatchIncremental(List<MatchResult> existingMatches, List<MatchableItem> newItems, string query)
    {
        // Append new items with score 0 when the query is empty
        if (string.IsNullOrWhiteSpace(query))
        {
            var combined = new List<MatchResult>(existingMatches.Count + newItems.Count);
            combined.AddRange(existingMatches);
            foreach (var item in newItems) combined.Add(new MatchResult(item, string.Empty, 0, Array.Empty<int>()));
            return combined;
        }

        // Perform matching only on the new items and merge with existing matches to maintain order and performance
        var q = query.ToLowerInvariant();
        var newMatches = new List<MatchResult>(newItems.Count);
        for (int i = 0; i < newItems.Count; i++)
        {
            var item = newItems[i];
            var display = item.Display ?? item.ToString() ?? string.Empty;
            var matchInfo = TryMatch(display, q);
            if (matchInfo.HasValue)
            {
                newMatches.Add(new MatchResult(item, display, matchInfo.Value.score, matchInfo.Value.positions));
            }
        }

        if (newMatches.Count == 0) return existingMatches; // No new matches, return existing list as-is

        // Sort new matches by score descending, then by display string for consistent ordering
        newMatches.Sort((a, b) =>
        {
            int cmp = b.Score.CompareTo(a.Score);
            return cmp != 0 ? cmp : string.Compare(a.DisplayString, b.DisplayString, StringComparison.Ordinal);
        });

        // Merge the two sorted lists
        var merged = new List<MatchResult>(existingMatches.Count + newMatches.Count);
        int existingIndex = 0, newIndex = 0;
        while (existingIndex < existingMatches.Count && newIndex < newMatches.Count)
        {
            var existing = existingMatches[existingIndex];
            var newMatch = newMatches[newIndex];

            // Compare scores to determine order
            int cmp = newMatch.Score.CompareTo(existing.Score);

            // If scores are equal, use display string as tiebreaker to ensure consistent ordering
            if (cmp == 0) cmp = string.Compare(existing.DisplayString, newMatch.DisplayString, StringComparison.Ordinal);
            // Higher score should come first, so if cmp > 0, newMatch goes before existing
            if (cmp > 0) { merged.Add(newMatch); newIndex++; }
            // otherwise, existing goes first
            else { merged.Add(existing); existingIndex++; }
        }

        // Add any remaining items from either list
        while (existingIndex < existingMatches.Count) merged.Add(existingMatches[existingIndex++]);
        while (newIndex < newMatches.Count) merged.Add(newMatches[newIndex++]);

        // Return the final merged list
        return merged;
    }

    /// <summary>
    /// Attempts to match the query against the provided text using a simple fuzzy matching algorithm.
    /// </summary>
    /// <param name="text">The text to match against.</param>
    /// <param name="query">The query string to match.</param>
    /// <returns>A tuple containing the score and positions of matched characters, or null if no match is found.</returns>
    private static (int score, int[] positions)? TryMatch(string text, string query)
    {
        int score = 0, textIndex = 0, consecutiveMatches = 0;
        var positions = new int[query.Length];

        // Iterate through each character in the query and try to find it in the text in order
        for (int queryIndex = 0; queryIndex < query.Length; queryIndex++)
        {
            char queryChar = query[queryIndex];

            // Find next occurrence of the query character in the text (case-insensitive)
            int foundIndex = FindNextIgnoreCase(text, textIndex, queryChar);
            if (foundIndex == -1)
            {
                return null; // Character not found, no match
            }

            // Character found, calculate score and store position
            positions[queryIndex] = foundIndex;

            // Bonus for matching at start of text
            if (foundIndex == 0)
            {
                score += 20;
            }

            // Bonus for consecutive matches
            if (foundIndex == textIndex && consecutiveMatches > 0)
            {
                score += 15;
                consecutiveMatches++;
            }
            else
            {
                consecutiveMatches = 1;
            }

            // Bonus for matching at a word boundary
            if (IsWordStart(text, foundIndex))
            {
                score += 10;
            }

            // Base Score (Earlier matches are better)
            score += Math.Max(0, 100 - foundIndex);

            textIndex = foundIndex + 1;
        }

        return (score, positions);
    }

    private static int FindNextIgnoreCase(string text, int startIndex, char target)
    {
        for (int i = startIndex; i < text.Length; i++)
            if (char.ToLowerInvariant(text[i]) == target)
                return i;
        return -1;
    }

    /// <summary>
    /// Determines if the character at the given index is the start of a word.
    /// A word starts at index 0, after a separator, or at a camelCase transition.
    /// </summary>
    private static bool IsWordStart(string text, int index)
    {
        if (index == 0) return true;

        var curr = text[index];
        var prev = text[index - 1];

        // camelCase boundary (e.g., 'C' in 'myClass')
        if (char.IsLower(prev) && char.IsUpper(curr)) return true;

        // After a separator (e.g., 'f' in 'my-folder')
        bool isPrevSeparator = prev == ' ' || prev == '-' || prev == '_' || prev == '/' || prev == '\\';
        return isPrevSeparator;
    }
}
