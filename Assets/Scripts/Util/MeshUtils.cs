using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public static class MeshUtils {

    public static Mesh CreateMesh(float width, float height) {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];
        
        /* 0, 0
         * 0, 1
         * 1, 1
         * 1, 0
         */
        
        float halfWidth  = width / 2f;
        float halfHeight = height / 2f;
        
        vertices[0] = new Vector3(-halfWidth,-halfHeight);
        vertices[1] = new Vector3(-halfWidth,+halfHeight);
        vertices[2] = new Vector3(+halfWidth,+halfHeight);
        vertices[3] = new Vector3(+halfWidth,-halfHeight);

        uv[0] = new Vector2(0,0);
        uv[1] = new Vector2(0,1);
        uv[2] = new Vector2(1,1);
        uv[3] = new Vector2(1,0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }
}
