using System;
using System.Collections.Generic;

namespace Lab3.Models
{
    public abstract class Interpolation
    {
        public abstract double Calculate(
            IReadOnlyList<PointD> points,
            double x);

        protected static void ValidatePoints(
            IReadOnlyList<PointD> points)
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException(
                    "Потрібно щонайменше дві опорні точки.");
            }

            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].X <= points[i - 1].X)
                {
                    throw new ArgumentException(
                        "Значення X повинні бути строго зростаючими.");
                }
            }
        }

        protected static int FindSegment(
            IReadOnlyList<PointD> points,
            double x)
        {
            if (x <= points[0].X)
                return 0;

            if (x >= points[points.Count - 1].X)
                return points.Count - 2;

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (x <= points[i + 1].X)
                    return i;
            }

            return points.Count - 2;
        }
    }
}