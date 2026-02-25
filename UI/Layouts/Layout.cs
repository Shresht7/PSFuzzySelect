using PSFuzzySelect.UI.Renderer;

namespace PSFuzzySelect.UI.Layouts;

public static class Layout
{
    public static IRenderSurface[] Vertical(IRenderSurface parent, params LayoutSection[] constraints)
    {
        return LayoutAlongAxis(parent, constraints, isVertical: true);
    }

    public static IRenderSurface[] Horizontal(IRenderSurface parent, params LayoutSection[] constraints)
    {
        return LayoutAlongAxis(parent, constraints, isVertical: false);
    }

    private static IRenderSurface[] LayoutAlongAxis(IRenderSurface parent, LayoutSection[] constraints, bool isVertical)
    {
        var surfaces = new IRenderSurface[constraints.Length];

        // Determine the total space available along the chosen axis (height for vertical, width for horizontal)
        var space = isVertical ? parent.Height : parent.Width;

        // Calculate total fixed space
        int fixedSize = constraints.OfType<Fixed>().Sum(c => c.Size);

        // Calculate the fractional spacing for Fraction sections
        // ? This is kinda rudimentary as it doesn't normalize/rescale the fractions if they don't sum up to 1
        int fractionalSize = constraints.OfType<Fraction>().Sum(c => (int)(c.Frac * space));

        // Calculate flexible space per item flex item
        int flexCount = constraints.Count(c => c is Flexible);
        int availableFlexSpace = space - fixedSize - fractionalSize;
        int flexPerItem = flexCount > 0 ? availableFlexSpace / flexCount : 0;

        // Apply constraints
        int position = 0;
        for (int i = 0; i < constraints.Length; i++)
        {
            // Determine the size for the current constraint
            int size = constraints[i] switch
            {
                Fixed f => f.Size,
                Flexible f => flexPerItem * f.Factor,
                Fraction f => (int)(f.Frac * space),
                _ => 0
            };

            // Create a sub-surface based on the calculated size and position
            var rect = isVertical
                ? new Rect(0, position, parent.Width, size)
                : new Rect(position, 0, size, parent.Height);
            surfaces[i] = parent.CreateSubSurface(rect);

            // Move along the axis for the next surface
            position += size;
        }

        return surfaces;
    }
}
