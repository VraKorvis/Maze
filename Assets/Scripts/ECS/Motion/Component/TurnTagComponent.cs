using Unity.Entities;
using UnityEngine;

public struct TurnTag : IComponentData {}

public class TurnTagComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new TurnTag());
    }
}