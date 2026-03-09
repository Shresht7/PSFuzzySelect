using PSFuzzySelect.UI.Components.Text;
using PSFuzzySelect.UI.Styles;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Components;

public class Spinner(bool active, string text, int frameInterval = 120) : IComponent
{
    /// <summary>
    /// The frames used to create the spinner animation effect.
    /// These characters are cycled through to give the appearance of a spinning motion when the spinner is active.
    /// </summary>
    private static readonly string[] _frames = ["⣾", "⣷", "⣯", "⣟", "⣻", "⣽", "⣾", "⣷"];

    /// <summary>The interval in milliseconds between frame updates for the spinner animation</summary>
    private readonly int _frameInterval = frameInterval;

    private Style _spinnerStyle = Style.Default.WithForeground(Color.Magenta);

    public void Render(ISurface surface)
    {
        // The text to display next to the spinner
        var spinner = new TextSpan(active ? _frames[0] : " ", _spinnerStyle);
        var display = new TextBlock()
            .Add(spinner).Add($" {text}")
            .Align(TextAlignment.Right);

        // Calculate the current frame based on the elapsed time to create an animation effect when active
        if (active)
        {
            var ms = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            var idx = (int)(ms / _frameInterval % _frames.Length);
            display = new TextBlock()
                .Add(new TextSpan(_frames[idx], _spinnerStyle)).Add($" {text}")
                .Align(TextAlignment.Right);
        }

        // Render the spinner
        display.Render(surface);
    }
}
