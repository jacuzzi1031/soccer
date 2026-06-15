using Net.FixFloat;
public static class DeterministicGeometry2D
{
    // 判断射线是否会进入矩形（AABB）
    public static bool RayIntersectsAABB(
        FixedVector2 rayOrigin,
        FixedVector2 rayDir,
        FixedRect aabb,
        FixedFloat maxDistance,
        out FixedFloat hitTime)
    {
        hitTime = FixedFloat.Zero;

        FixedFloat tMin = FixedFloat.Zero;
        FixedFloat tMax = maxDistance;

        // X轴
        if (rayDir.x == FixedFloat.Zero)
        {
            // 射线与X轴平行
            if (rayOrigin.x < aabb.xMin || rayOrigin.x > aabb.xMax)
                return false;
        }
        else
        {
            FixedFloat tx1 = (aabb.xMin - rayOrigin.x) / rayDir.x;
            FixedFloat tx2 = (aabb.xMax - rayOrigin.x) / rayDir.x;

            if (tx1 > tx2)
            {
                FixedFloat temp = tx1;
                tx1 = tx2;
                tx2 = temp;
            }

            tMin = FixedMath.Max(tMin, tx1);
            tMax = FixedMath.Min(tMax, tx2);

            if (tMin > tMax)
                return false;
        }

        // Y轴
        if (rayDir.y == FixedFloat.Zero)
        {
            // 射线与Y轴平行
            if (rayOrigin.y < aabb.yMin || rayOrigin.y > aabb.yMax)
                return false;
        }
        else
        {
            FixedFloat ty1 = (aabb.yMin - rayOrigin.y) / rayDir.y;
            FixedFloat ty2 = (aabb.yMax - rayOrigin.y) / rayDir.y;

            if (ty1 > ty2)
            {
                FixedFloat temp = ty1;
                ty1 = ty2;
                ty2 = temp;
            }

            tMin = FixedMath.Max(tMin, ty1);
            tMax = FixedMath.Min(tMax, ty2);

            if (tMin > tMax)
                return false;
        }

        hitTime = tMin;
        return hitTime <= maxDistance;
    }
}