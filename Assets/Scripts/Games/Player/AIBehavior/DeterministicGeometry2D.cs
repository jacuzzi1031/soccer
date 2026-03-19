using UnityEngine;

public static class DeterministicGeometry2D
{
    // 判断射线是否会进入矩形（AABB）
    public static bool RayIntersectsAABB(
        Vector2 rayOrigin,
        Vector2 rayDir,
        Rect aabb,
        float maxDistance,
        out float hitTime)
    {
        hitTime = 0f;

        // 防止除0
        float invDirX = Mathf.Abs(rayDir.x) > 1e-6f ? 1f / rayDir.x : float.MaxValue;
        float invDirY = Mathf.Abs(rayDir.y) > 1e-6f ? 1f / rayDir.y : float.MaxValue;

        float t1 = (aabb.xMin - rayOrigin.x) * invDirX;
        float t2 = (aabb.xMax - rayOrigin.x) * invDirX;
        float t3 = (aabb.yMin - rayOrigin.y) * invDirY;
        float t4 = (aabb.yMax - rayOrigin.y) * invDirY;

        float tMin = Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4));
        float tMax = Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4));
        // tMax离开时间
        if (tMax < 0f)
            return false;
        if (tMin > tMax)
            return false;
        // tMin 是进入时间
        float tHit = tMin >= 0f ? tMin : tMax;
        if (tHit < 0f || tHit > maxDistance)
            return false;
        hitTime = tHit;
        return true;
    }
}