using Unity.Entities;
using UnityEngine;

public struct TargetForHaunting : IComponentData {
    public Entity focus;
    public CellIndex cellIndex;
}

public class TargetForHauntingComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new TargetForHaunting());
    }
}