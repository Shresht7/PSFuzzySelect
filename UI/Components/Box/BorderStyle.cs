namespace PSFuzzySelect.UI.Components.Box;

/// <summary>Describes the style of a box</summary>
public readonly record struct BorderStyle(
    char TopLeft, char Top, char TopRight,
    char Left, char Right,
    char BottomLeft, char Bottom, char BottomRight
)
{
    public static BorderStyle None => default;
    public static BorderStyle Single => new BorderStyle('┌', '─', '┐', '│', '│', '└', '─', '┘');
    public static BorderStyle Double => new BorderStyle('╔', '═', '╗', '║', '║', '╚', '═', '╝');
    public static BorderStyle Rounded => new BorderStyle('╭', '─', '╮', '│', '│', '╰', '─', '╯');
    public static BorderStyle Thick => new BorderStyle('┏', '━', '┓', '┃', '┃', '┗', '━', '┛');
    public static BorderStyle Hidden => new BorderStyle(' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');
}
