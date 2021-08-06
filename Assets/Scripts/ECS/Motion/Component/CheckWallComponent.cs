using System;
using Unity.Entities;
using UnityEngine;

public struct CheckWallTag : IComponentData { }

[Serializable]
public class CheckWallComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new CheckWallTag());
    }
}