using System;
using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData {
    public Guise currentGuise;
}

[RequiresEntityConversion]
public class PlayerDataComponent : MonoBehaviour, IConvertGameObjectToEntity {
    [Tooltip("ID current Guise. Corresponds to the id guise of the enemy")]
    public Guise currentGuise;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var data = new PlayerData() {
            currentGuise = currentGuise,
        };
        dstManager.AddComponentData(entity, data);
        dstManager.AddComponentData(entity, new CellMovementTag());
        dstManager.AddComponentData(entity, new IndicesForwardCellTemp());
    }
}