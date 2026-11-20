using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Petal;
public sealed class Sparkline : Control
{
    private double[] points = [];
    public void SetPoints(double[] values) { points = values; InvalidateVisual(); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        double w = Bounds.Width, h = Bounds.Height;
        if (w <= 6 || h <= 14) return;
        var dots = Brush.Parse("#2D9791A4");
        for (int row = 1; row < 3; row++)
            for (int col = 1; col < 7; col++)
                context.DrawEllipse(dots, null, new Point(w * col / 7, h * row / 3), .7, .7);
        if (points.Length == 0) return;
        double low = points.Min(), high = points.Max(), span = Math.Max(high - low, .01);
        var geometry = new StreamGeometry();
        Point last = default;
        using (var g = geometry.Open())
        {
            for (int i = 0; i < points.Length; i++)
            {
                last = new Point(3 + (w - 6) * i / Math.Max(points.Length - 1, 1),
                    h - 7 - (points[i] - low) / span * (h - 14));
                if (i == 0) g.BeginFigure(last, false);
                else g.LineTo(last);
            }
            g.EndFigure(false);
        }
        context.DrawGeometry(null, new Pen(Brush.Parse("#3763577D"), 4, lineJoin: PenLineJoin.Round), geometry);
        context.DrawGeometry(null, new Pen(Brush.Parse("#A395C2"), 1.5, lineJoin: PenLineJoin.Round), geometry);
        context.DrawEllipse(Brush.Parse("#B7AAD3"), null, last, 2, 2);
    }
}
