using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct GridTag : IComponentData { }

public struct GridBuffer : IBufferElementData {
    public int      Index;
    public float3   WorldPos;
    public int2     coord;
    public CellType Type;
}

public struct CellIndex : IComponentData {
    public int  Index;
    public int2 coord;
}

[Serializable]
[RequiresEntityConversion]
public class CellIndexComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new CellIndex());
    }
}

public enum CellType {
    Empty, Ground, Wall, Mask, Trap
}