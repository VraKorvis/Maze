using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class SpriteToMeshComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var sr     = GetComponent<SpriteRenderer>();
        var mat    = sr.sharedMaterial;
        var sprite = sr.sprite;
        mat.mainTexture = sprite.texture;
   
        dstManager.AddSharedComponentData(entity,
            new RenderMesh {
                material = mat,
                mesh     = SpriteToMesh(sprite)
            });
    }

    private Mesh SpriteToMesh(Sprite sprite) {
        Mesh mesh = new Mesh {
            vertices  = Array.ConvertAll(sprite.vertices, vertex => (Vector3)vertex),
            uv        = sprite.uv,
            triangles = Array.ConvertAll(sprite.triangles, triangle => (int) triangle)
        };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}