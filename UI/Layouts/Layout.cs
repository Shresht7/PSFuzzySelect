using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Layouts;

public static class Layout
{
    public static IRenderSurface[] Vertical(IRenderSurface parent, params LayoutSection[] sections)
    {
        return LayoutAlongAxis(parent, sections, isVertical: true);
    }

    public static IRenderSurface[] Horizontal(IRenderSurface parent, params LayoutSection[] sections)
    {
        return LayoutAlongAxis(parent, sections, isVertical: false);
    }

    private static IRenderSurface[] LayoutAlongAxis(IRenderSurface parent, LayoutSection[] sections, bool isVertical)
    {
        var surfaces = new IRenderSurface[sections.Length];

        // Determine the total space available along the chosen axis (height for vertical, width for horizontal)
        var space = isVertical ? parent.Height : parent.Width;

        // Calculate total fixed space
        int fixedSpace = sections.OfType<Fixed>().Sum(c => c.Size);

        // Calculate total fractional space
        var fractionSections = sections.OfType<Fraction>().ToArray();
        int fractionalSpace = fractionSections.Sum(c => (int)(c.Frac * space));

        // Calculate flexible space per item flex item
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
                Flexible f => remainingSpace * f.Factor / totalFlexFactor,
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
