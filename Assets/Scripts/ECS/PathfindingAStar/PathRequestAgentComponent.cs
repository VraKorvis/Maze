using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PathRequestAgent : IComponentData {
    public Entity focus;
    public Entity owner;
    public int2 startCoord;
    public int2 destination;
}

public class PathRequestAgentComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new PathRequestAgent());
        dstManager.AddComponentData(entity, new PathAgentStatusNoneTag());
        dstManager.AddBuffer<Waypoint>(entity);
    }
}