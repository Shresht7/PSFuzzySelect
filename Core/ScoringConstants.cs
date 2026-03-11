namespace PSFuzzySelect.Core;

/// <summary>
/// Contains constants used for calculating fuzzy matching scores.
/// These values determine the weight given to various matching criteria.
/// </summary>
public static class ScoringConstants
{
    /// <summary>Bonus awarded when a match occurs at the very beginning of the text.</summary>
    public const int StartOfTextBonus = 20;

    /// <summary>Bonus awarded for each consecutive character match after the first one.</summary>
    public const int ConsecutiveMatchBonus = 15;

    /// <summary>Bonus awarded when a match occurs at a word boundary (e.g., after a separator or at a camelCase transition).</summary>
    public const int WordBoundaryBonus = 10;

    /// <summary>The maximum base score awarded for a match. The actual base score decreases as the match position moves further into the text.</summary>
    public const int BaseScoreMax = 100;

    /// <summary>The default score assigned to items when no query is provided.</summary>
    public const int DefaultScore = 0;
}
