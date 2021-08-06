using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Waypoint : IBufferElementData {
    public float3 point;
}

public struct PathAgentStatus : IComponentData {
    public AgentStatus Value;
}

public struct PathAgentStatusFindTag : IComponentData { }

public struct PathAgentStatusAddPathRequestTag : IComponentData { }

public struct PathAgentStatusNoneTag : IComponentData { }

public struct PathAgentStatusReadyTag : IComponentData { }

public struct PathAgentStatusProcessTag : IComponentData {
    // public int wayBufferIndex;
}

public enum AgentStatus {
    AddPathRequest,
    Find,
    Ready,
    Wait,
    None,
    Process,
    Done
}

[RequiresEntityConversion]
public class PathAgentStatusComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public AgentStatus status;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        // dstManager.AddComponentData(entity, new PathAgentStatus() {
        //     Value = status
        // });
        // dstManager.AddBuffer<Waypoint>(entity);
    }
}