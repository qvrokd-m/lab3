using System.Collections.Generic;

namespace Lab3.Models
{
    public sealed class LinearInterpolation : Interpolation
    {
        public override double Calculate(
            IReadOnlyList<PointD> points,
            double x)
        {
            ValidatePoints(points);

            int i = FindSegment(points, x);

            double x1 = points[i].X;
            double y1 = points[i].Y;
            double x2 = points[i + 1].X;
            double y2 = points[i + 1].Y;

            double t = (x - x1) / (x2 - x1);

            return y1 + t * (y2 - y1);
        }
    }
}