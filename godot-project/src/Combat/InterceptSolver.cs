using Godot;

namespace FrontsOfWar.Combat;

// Classic pursuit-intercept math so projectiles lead moving targets (GDD
// §19 prompt 11). Returns null if the projectile is too slow to ever catch
// the target, in which case callers should fall back to aiming at the
// target's current position.
public static class InterceptSolver
{
    public static Vector2? PredictInterceptPoint(Vector2 shooterPos, Vector2 targetPos, Vector2 targetVelocity, float projectileSpeed)
    {
        Vector2 toTarget = targetPos - shooterPos;

        float a = targetVelocity.LengthSquared() - projectileSpeed * projectileSpeed;
        float b = 2f * toTarget.Dot(targetVelocity);
        float c = toTarget.LengthSquared();

        float t;
        if (Mathf.Abs(a) < 0.0001f)
        {
            // Degenerate (projectile speed ~= target speed): linear fallback.
            if (Mathf.Abs(b) < 0.0001f) return null;
            t = -c / b;
        }
        else
        {
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f) return null;

            float sqrtDisc = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDisc) / (2f * a);
            float t2 = (-b - sqrtDisc) / (2f * a);
            t = (t1 > 0f && t2 > 0f) ? Mathf.Min(t1, t2) : Mathf.Max(t1, t2);
        }

        if (t <= 0f || float.IsNaN(t)) return null;
        return targetPos + targetVelocity * t;
    }
}
