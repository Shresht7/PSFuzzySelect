using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Layouts;

/// <summary>
/// Provides methods for laying out UI sections within a parent render surface, either vertically or horizontally
/// </summary>
public static class Layout
{
    /// <summary>
    /// Lays out the given sections vertically within the parent surface.
    /// Each section can be of type Fixed, Fraction, or Flexible,
    /// which determines how much space it occupies relative to the total available height.
    /// </summary>
    /// <param name="parent">The parent render surface</param>
    /// <param name="sections">The sections to layout</param>
    /// <returns>An array of render surfaces representing the laid out sections</returns>
    public static ISurface[] Vertical(ISurface parent, params LayoutSection[] sections)
    {
        return LayoutAlongAxis(parent, sections, isVertical: true);
    }

    /// <summary>
    /// Lays out the given sections horizontally within the parent surface.
    /// Each section can be of type Fixed, Fraction, or Flexible,
    /// which determines how much space it occupies relative to the total available width.
    /// </summary>
    /// <param name="parent">The parent render surface</param>
    /// <param name="sections">The sections to layout</param>
    /// <returns>An array of render surfaces representing the laid out sections</returns>
    public static ISurface[] Horizontal(ISurface parent, params LayoutSection[] sections)
    {
        return LayoutAlongAxis(parent, sections, isVertical: false);
    }

    /// <summary>
    /// Lays out the given sections along the specified axis (vertical or horizontal) within the parent surface.
    /// </summary>
    /// <param name="parent">The parent render surface</param>
    /// <param name="sections">The sections to layout</param>
    /// <param name="isVertical">Whether to layout vertically or horizontally</param>
    /// <returns>An array of render surfaces representing the laid out sections</returns>
    private static ISurface[] LayoutAlongAxis(ISurface parent, LayoutSection[] sections, bool isVertical)
    {
        // An array to hold the resulting render surfaces for each section
        var surfaces = new ISurface[sections.Length];

        // Determine the total space available along the chosen axis (height for vertical, width for horizontal)
        var space = isVertical ? parent.Height : parent.Width;

        // Calculate total fixed space
        int fixedSpace = sections.OfType<Fixed>().Sum(c => c.Size);

        // Calculate total fractional space
        var fractionSections = sections.OfType<Fraction>().ToArray();
        int fractionalSpace = fractionSections.Sum(c => (int)(c.Frac * space));

        // Calculate flexible space per flex item
        var flexSections = sections.OfType<Flexible>().ToArray();
        int totalFlexFactor = flexSections.Sum(c => c.Factor);

        // Calculate remaining space after allocating fixed and fractional space
        int remainingSpace = Math.Max(0, space - fixedSpace - fractionalSpace);

        // Calculate the size for each section based on its type and the available space
        var sizes = new int[sections.Length];
        for (int i = 0; i < sections.Length; i++)
        {
            sizes[i] = sections[i] switch
            {
                Fixed f => Math.Max(0, f.Size),
                Fraction f => (int)Math.Floor(f.Frac * space),
                Flexible f => totalFlexFactor > 0 ? remainingSpace * f.Factor / totalFlexFactor : 0,
                _ => 0
            };
        }

        // Distribute leftovers
        int allocatedSpace = sizes.Sum();
        int leftover = space - allocatedSpace;
        for (int i = 0; i < sections.Length; i++)
        {
            if (leftover <= 0) break; // No more space to distribute

            // Preferentially allocate leftover space to flexible sections, then fractional, then fixed
            for (int pass = 0; leftover > 0 && pass < 3; pass++)
            {
                bool wants = pass switch
                {
                    0 => sections[i] is Flexible,
                    1 => sections[i] is Fraction,
                    2 => sections[i] is Fixed,
                    _ => false
                };
                if (wants)
                {
                    sizes[i]++;
                    leftover--;
                }
            }
        }

        // Create sub-surfaces for each section based on the calculated sizes
        var position = 0;
        for (int i = 0; i < sections.Length; i++)
        {
            int size = sizes[i];
            var rect = isVertical
                ? new Rect(0, position, parent.Width, size)
                : new Rect(position, 0, size, parent.Height);
            surfaces[i] = parent.CreateSubSurface(rect);
            position += size;
        }

        return surfaces;
    }
}
