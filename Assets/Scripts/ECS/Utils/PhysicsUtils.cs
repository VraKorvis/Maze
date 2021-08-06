using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class PhysicsUtils {

    /// <summary>
    ///   <para>Calculate a position between the points specified by current and target, moving no farther than the distance specified by maxDistanceDelta.</para>
    /// </summary>
    /// <param name="current">The position to move from.</param>
    /// <param name="target">The position to move towards.</param>
    /// <param name="maxDistanceDelta">Distance to move current per call.</param>
    /// <returns>
    ///   <para>The new position.</para>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta) {
        float dX       = target.x - current.x;
        float dY       = target.y - current.y;
        float dZ       = target.z - current.z;
        float sqrlen = (float) ((double) dX * (double) dX + (double) dY * (double) dY + (double) dZ * (double) dZ);

        if ((double) sqrlen == 0.0f || sqrlen <= (double) maxDistanceDelta * (double) maxDistanceDelta)
            return target;
        float num5 = (float) math.sqrt((double) sqrlen);
        return new float3(current.x + dX / num5 * maxDistanceDelta, current.y + dY / num5 * maxDistanceDelta,
            current.z + dZ / num5 * maxDistanceDelta);
    }

    /// <summary>
    /// Convert Quaternion to Euler (radians)
    /// </summary>
    /// <param name="quaternion"></param>
    /// <returns>Radians</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Euler(this quaternion quaternion) {
        float4  q = quaternion.value;
        double3 res;

        double sinr_cosp = +2.0 * (q.w * q.x + q.y * q.z);
        double cosr_cosp = +1.0 - 2.0 * (q.x * q.x + q.y * q.y);
        res.x = math.atan2(sinr_cosp, cosr_cosp);

        double sinp = +2.0 * (q.w * q.y - q.z * q.x);
        if (math.abs(sinp) >= 1) {
            res.y = math.PI / 2 * math.sign(sinp);
        } else {
            res.y = math.asin(sinp);
        }

        double siny_cosp = +2.0 * (q.w * q.z + q.x * q.y);
        double cosy_cosp = +1.0 - 2.0 * (q.y * q.y + q.z * q.z);
        res.z = math.atan2(siny_cosp, cosy_cosp);

        return (float3) res;
    }

    /// <summary>
    /// Calculate a position between the points specified by current and target, moving no farther than the distance specified by maxDistanceDelta.
    /// </summary>
    /// <param name="quaternion">Quaternion who need convert to Euler</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToEuler(quaternion quaternion) {
        return quaternion.Euler();
    }

    /// <summary>
    /// Calculate AABB collision
    /// </summary>
    /// <param name="a">collision AABB box first object</param>
    /// <param name="b">collision AABB box second object</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OverlapAABB(AABB a, AABB b) {
        return (a.Min.x <= b.Max.x && a.Max.x >= b.Min.x) && (a.Min.z <= b.Max.z && a.Max.z >= b.Min.z) && (a.Min.y <= b.Max.y && a.Max.y >= b.Min.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetAABBCenter(AABB aabb) {
        return (aabb.Min + aabb.Max) * 0.5f;
    }

}