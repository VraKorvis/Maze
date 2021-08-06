using System;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public struct VectorUtil  {

    public static float GetAngle(Vector3 A, Vector3 B) {
        // |A·B| = |A| |B| COS(θ)
        // |A×B| = |A| |B| SIN(θ) - via S = |a|*|b| &sin(ab);
        return Mathf.Atan2(Cross(A, B), Vector3.Dot(A, B));
    }

    public static double Dot(Vector3 a, Vector3 b) {
        return a.x * b.x + a.y * b.y;
    }

    // cross for Vector3d(Vector3 a, Vector3 b) (example a = (5;5), b=(0;5)) Cross = (0;0;25) - x, y always = 0, 
    // throught matrix determinant
    //i(a.y·b.z - b.y·a.z) - j(a.x·b.z - b.x·a.z) + k(5·5 - 5·0) = i(0 - 0) - j(0 - 0) + k(25 - 0) = {0; 0; 25}
    //S = |a|*|b| &sin(ab); S = 25
    public static float Cross(Vector3 a, Vector3 b) {
        return a.x * b.y - a.y * b.x;
    }

    #region Angle2
    private static float AngleBetween(Vector3 normalized_a, Vector3 normalized_b, Vector3 n) {
        var dot_ab = Vector3.Dot(normalized_a, normalized_b);
        var cross_ab = Vector3.Cross(normalized_a, normalized_b);
        var angel = Mathf.Rad2Deg * Mathf.Acos(dot_ab);
        float sign = Mathf.Sign(Vector3.Dot(n, cross_ab));
        var singed_angel = sign * angel;
        var angel_360 = (singed_angel + 360) % 360;
        return angel_360;
    }
    #endregion

    #region AngleBetween
    private static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n) {
        // angle in [0,180] 
        float angle = Vector3.Angle(a, b);
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));
        // angle in [-179,180] 
        float signed_angle = angle * sign;
        // angle in [0,360] (not used but included here for completeness) 
        float angle360 = (signed_angle + 360) % 360;
        return angle360;
    }
    #endregion
}