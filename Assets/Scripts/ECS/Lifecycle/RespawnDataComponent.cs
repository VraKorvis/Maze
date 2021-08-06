using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct  MarkedToRespawnTag: IComponentData{}
public struct RespawnData : IComponentData {
    public float3 point;
    public int2 VectorDirection;
    
}
[Serializable]
[RequiresEntityConversion]
public class RespawnDataComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new RespawnData() {
            point = transform.position,
            VectorDirection = GridUtils.GetDir(GetComponent<HeadingComponent>().direction),
        });
    }
}



