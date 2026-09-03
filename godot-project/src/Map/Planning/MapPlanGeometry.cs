using System;
using System.Collections.Generic;

namespace FrontsOfWar.Map.Planning;

public static class MapPlanGeometry
{
    public static float Distance(PlanPoint a, PlanPoint b)
    {
        float x = a.X - b.X;
        float y = a.Y - b.Y;
        return MathF.Sqrt(x * x + y * y);
    }

    public static float PathLength(PathPlan path)
    {
        float length = 0f;
        for (int i = 1; i < path.Points.Count; i++)
            length += Distance(path.Points[i - 1], path.Points[i]);
        return length;
    }

    public static int BendCount(PathPlan path)
    {
        int bends = 0;
        for (int i = 1; i < path.Points.Count - 1; i++)
        {
            var before = path.Points[i - 1];
            var current = path.Points[i];
            var after = path.Points[i + 1];
            float first = MathF.Atan2(current.Y - before.Y, current.X - before.X);
            float second = MathF.Atan2(after.Y - current.Y, after.X - current.X);
            float delta = MathF.Abs(MathF.Atan2(MathF.Sin(second - first), MathF.Cos(second - first)));
            if (delta > 0.18f) bends++;
        }
        return bends;
    }

    public static float PointToSegmentDistance(PlanPoint point, PlanPoint start, PlanPoint end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float lengthSquared = dx * dx + dy * dy;
        if (lengthSquared <= 0.0001f) return Distance(point, start);
        float t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared;
        t = Math.Clamp(t, 0f, 1f);
        return Distance(point, new PlanPoint(start.X + t * dx, start.Y + t * dy));
    }

    public static float DistanceToPath(PlanPoint point, PathPlan path)
    {
        float nearest = float.MaxValue;
        for (int i = 1; i < path.Points.Count; i++)
            nearest = MathF.Min(nearest, PointToSegmentDistance(point, path.Points[i - 1], path.Points[i]));
        return nearest;
    }

    public static bool PathsIntersect(PathPlan first, PathPlan second, out PlanPoint intersection)
    {
        for (int i = 1; i < first.Points.Count; i++)
        for (int j = 1; j < second.Points.Count; j++)
        {
            if (StrictSegmentsIntersect(first.Points[i - 1], first.Points[i],
                second.Points[j - 1], second.Points[j], out intersection)) return true;
        }
        intersection = new PlanPoint();
        return false;
    }

    public static bool SelfIntersects(PathPlan path, out PlanPoint intersection)
    {
        for (int i = 1; i < path.Points.Count; i++)
        for (int j = i + 2; j < path.Points.Count; j++)
        {
            if (j == i + 1) continue;
            if (StrictSegmentsIntersect(path.Points[i - 1], path.Points[i],
                path.Points[j - 1], path.Points[j], out intersection)) return true;
        }
        intersection = new PlanPoint();
        return false;
    }

    public static float MinimumSeparation(IReadOnlyList<PathPlan> paths)
    {
        float nearest = float.MaxValue;
        for (int i = 0; i < paths.Count; i++)
        for (int j = i + 1; j < paths.Count; j++)
        foreach (var point in paths[i].Points)
            nearest = MathF.Min(nearest, DistanceToPath(point, paths[j]));
        return paths.Count < 2 ? 0f : nearest;
    }

    private static bool StrictSegmentsIntersect(PlanPoint a, PlanPoint b, PlanPoint c, PlanPoint d,
        out PlanPoint intersection)
    {
        float ab = Cross(a, b, c);
        float ab2 = Cross(a, b, d);
        float cd = Cross(c, d, a);
        float cd2 = Cross(c, d, b);
        if ((ab > 0f && ab2 < 0f || ab < 0f && ab2 > 0f) &&
            (cd > 0f && cd2 < 0f || cd < 0f && cd2 > 0f))
        {
            float denominator = (a.X - b.X) * (c.Y - d.Y) - (a.Y - b.Y) * (c.X - d.X);
            if (MathF.Abs(denominator) > 0.0001f)
            {
                float left = a.X * b.Y - a.Y * b.X;
                float right = c.X * d.Y - c.Y * d.X;
                intersection = new PlanPoint(
                    (left * (c.X - d.X) - (a.X - b.X) * right) / denominator,
                    (left * (c.Y - d.Y) - (a.Y - b.Y) * right) / denominator);
                return true;
            }
        }
        intersection = new PlanPoint();
        return false;
    }

    private static float Cross(PlanPoint a, PlanPoint b, PlanPoint point)
        => (b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X);
}
