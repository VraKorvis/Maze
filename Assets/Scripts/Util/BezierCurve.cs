using System;
using UnityEngine;
using System.Collections;

public static class BezierCurve {
   
    public static void QuadraticBezier() {//[x,y]=(1−t)2P0+(1−t)tP1+t2P2
        throw new NotImplementedException();
    }

    //[x,y]=(1−t)3P0+3(1−t)2tP1+3(1−t)t2P2+t3P3
    //Vector3 [x,y] = Math.Pow(1-t, 3)*p0 + 3*Math.Pow(1-t, 2)*t*p1 + 3*Math.Pow(1-t, 2)*t*p2 +Mat.Pow(t, 3)*p3;
    public static Vector3 CubicBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;
        return p;
    }
}