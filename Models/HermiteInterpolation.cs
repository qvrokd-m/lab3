using System.Collections.Generic;

namespace Lab3.Models
{
    public sealed class HermiteInterpolation : Interpolation
    {
        public override double Calculate(
            IReadOnlyList<PointD> points,
            double x)
        {
            ValidatePoints(points);

            int i = FindSegment(points, x);

            PointD p0 = points[i];
            PointD p1 = points[i + 1];

            double dx = p1.X - p0.X;
            double t = (x - p0.X) / dx;

            double m0 = CalculateSlope(points, i);
            double m1 = CalculateSlope(points, i + 1);

            double h00 = 2 * t * t * t - 3 * t * t + 1;
            double h10 = t * t * t - 2 * t * t + t;
            double h01 = -2 * t * t * t + 3 * t * t;
            double h11 = t * t * t - t * t;

            return h00 * p0.Y
                   + h10 * dx * m0
                   + h01 * p1.Y
                   + h11 * dx * m1;
        }

        private static double CalculateSlope(
            IReadOnlyList<PointD> points,
            int index)
        {
            if (index == 0)
            {
                return (points[1].Y - points[0].Y) /
                       (points[1].X - points[0].X);
            }

            if (index == points.Count - 1)
            {
                int last = points.Count - 1;

                return (points[last].Y - points[last - 1].Y) /
                       (points[last].X - points[last - 1].X);
            }

            return (points[index + 1].Y - points[index - 1].Y) /
                   (points[index + 1].X - points[index - 1].X);
        }
    }
}