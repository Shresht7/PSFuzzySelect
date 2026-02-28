using PSFuzzySelect.UI.Geometry;
using PSFuzzySelect.UI.Surface;

namespace PSFuzzySelect.UI.Layouts;

public class Blueprint(Size[] sections, bool isVertical)
{
    private int _gap = 0;

    public Blueprint Gap(int gap)
    {
        _gap = gap;
        return this;
    }

    public IComponent Compose(params IComponent[] components)
    {
        if (components.Length != sections.Length)
        {
            throw new ArgumentException("Number of components must match the number of sections in the blueprint");
        }
        return new Frame(sections, components, isVertical, _gap);
    }
}

public class Frame(Size[] sections, IComponent[] components, bool isVertical, int gap) : IComponent
{
    public void Render(ISurface surface)
    {
        // Determine the total space available along the chosen axis (height for vertical, width for horizontal)
        var space = isVertical ? surface.Height : surface.Width;

        // Determine the available space after accounting for gaps between sections
        int totalGapSpace = gap * Math.Max(0, sections.Length - 1);
        space = Math.Max(0, space - totalGapSpace);

        // Calculate the sizes for each section based on the available space
        var sizes = CalculateSizes(space);

        // Create sub-surfaces for each section based on the calculated sizes
        var position = 0;
        for (int i = 0; i < sizes.Length; i++)
        {
            int size = sizes[i];
            var rect = isVertical
                ? new Rect(0, position, surface.Width, size)
                : new Rect(position, 0, size, surface.Height);
            var subSurface = surface.CreateSubSurface(rect);

            // Render the component within the sub-surface
            components[i].Render(subSurface);

            // Advance the position for the next section, accounting for the gap
            position += size + gap;
        }
    }

    private int[] CalculateSizes(int space)
    {
        // Calculate total fixed space
        int fixedSpace = sections.OfType<FixedSize>().Sum(c => c.Size);

        // Calculate total fractional space
        var fractionSections = sections.OfType<FractionalSize>().ToArray();
        int fractionalSpace = fractionSections.Sum(c => (int)(c.Frac * space));

        // Calculate flexible space per flex item
        var flexSections = sections.OfType<FlexibleSize>().ToArray();
        int totalFlexFactor = flexSections.Sum(c => c.Factor);


        // Calculate remaining space after allocating fixed and fractional space
        int remainingSpace = Math.Max(0, space - fixedSpace - fractionalSpace);

        // Calculate the size for each section based on its type and the available space
        var sizes = new int[sections.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            sizes[i] = sections[i] switch
            {
                FixedSize f => Math.Max(0, f.Size),
                FractionalSize f => (int)Math.Floor(f.Frac * space),
                FlexibleSize f => totalFlexFactor > 0 ? remainingSpace * f.Factor / totalFlexFactor : 0,
                _ => 0
            };
        }


        // Distribute leftovers
        int allocatedSpace = sizes.Sum();
        int leftover = space - allocatedSpace;

        for (int pass = 0; leftover > 0 && pass < 3; pass++)
        {
            for (int i = 0; i < sizes.Length && leftover > 0; i++)
            {
                bool wants = pass switch
                {
                    0 => sections[i] is FlexibleSize,
                    1 => sections[i] is FractionalSize,
                    2 => sections[i] is FixedSize,
                    _ => false
                };
                if (wants)
                {
                    sizes[i]++;
                    leftover--;
                }
            }
        }

        return sizes;
    }
}
