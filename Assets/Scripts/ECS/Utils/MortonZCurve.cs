using Unity.Mathematics;
//https://stackoverflow.com/questions/1024754/how-to-compute-a-3d-morton-number-interleave-the-bits-of-3-ints
//https://en.wikipedia.org/wiki/Z-order_curve
//https://stackoverrun.com/ru/q/8407898
public static class MortonZCurve {

   /// <summary>
   /// Convert Morton index to x,y coord
   /// </summary>
   /// <param name="z"></param>
   /// <returns></returns>
    public static int MortonIndexToXY(int z) {
       throw new System.NotImplementedException();
    }

    /// <summary>
    /// Convert XY coordinate to morton Z-curve index
    /// </summary>
    /// <param name="x">x coord</param>
    /// <param name="y">y coord</param>
    /// <returns></returns>
    public static int CoordToMortonIndex(int x, int y) {
        x = (x | (x << 16)) & 0x030000FF;
        x = (x | (x << 8)) & 0x0300F00F;
        x = (x | (x << 4)) & 0x030C30C3;
        x = (x | (x << 2)) & 0x09249249;

        y = (y | (y << 16)) & 0x030000FF;
        y = (y | (y << 8)) & 0x0300F00F;
        y = (y | (y << 4)) & 0x030C30C3;
        y = (y | (y << 2)) & 0x09249249;
        int z = x | (y << 1);
        return x | (y << 1) | (z << 2);
    }

    public static int WorldPosToMortonIndex(int2 pos) {
        return CoordToMortonIndex(pos.x, pos.y);
    }
    
    public static int WorldPosToMortonIndex(float3 pos) {
        return CoordToMortonIndex((int)pos.x, (int)pos.y);
    }
    
    public static int WorldPosToMortonIndex(AABB aabb) {
        var pos = PhysicsUtils.GetAABBCenter(aabb);
        return CoordToMortonIndex((int)pos.x, (int)pos.y);
    }
}