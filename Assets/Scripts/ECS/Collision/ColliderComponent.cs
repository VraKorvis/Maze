using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum ColliderType {
    AABB,
    /*
     * 0 = UNDEFINED
     * 1 = Sphere
     * 2 = Box
     * 3 = Capsule
     * 4 = Convex Mesh
     * 5 = Generic Mesh
    */
}

public struct AABB : IComponentData {
    public float3 Min;
    public float3 Max;
    public float3 Offset;
}

//public struct Collider : IComponentData {
//    public float Radius;
//}
//
//public struct BoxCollider : IComponentData {
//    public float3 HalfSize;
//}
//
//public struct CapsuleCollider : IComponentData {
//    public float Radius;
//    public float Height;
//}

public struct CollisionInfo : IComponentData {
    public Entity self;
    public Entity another;
}

[Serializable]
[RequiresEntityConversion]
public class ColliderComponent : MonoBehaviour, IConvertGameObjectToEntity {
    [SerializeField] private ColliderType type;

    [Tooltip("Size of box. Where 0.5f - max box, size of cell")]
    public Vector3 size;

    public Vector2 offset;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var position = transform.position;
        var pos = new float3(position.x, position.y, 0);
        var min = new float3(pos.x - size.x, pos.y - size.y, 0);
        var max = new float3(pos.x + size.x, pos.y + size.y, 0);
        dstManager.AddComponentData(entity, new AABB() {
            Min = min,
            Max = max,
            Offset = size
        });
        dstManager.AddComponentData(entity, new CollisionInfo());
    }

    private void OnDrawGizmosSelected() {
        var p = new Vector3(transform.position.x + offset.x, transform.position.y + offset.y);
        var leftBotCorner = new Vector3(p.x - size.x, p.y - size.y);
        var rightBotCorner = leftBotCorner + new Vector3(size.x * 2, 0, 0);
        var leftTopCorner = leftBotCorner + new Vector3(0, size.y * 2, 0);
        var rightTopCorner = rightBotCorner + new Vector3(0, size.y * 2, 0);

        Debug.DrawLine(leftBotCorner, rightBotCorner, Color.green);
        Debug.DrawLine(leftBotCorner, leftTopCorner, Color.green);
        Debug.DrawLine(rightBotCorner, rightTopCorner, Color.green);
        Debug.DrawLine(leftTopCorner, rightTopCorner, Color.green);
    }
}