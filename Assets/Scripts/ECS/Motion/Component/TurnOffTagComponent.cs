using Unity.Entities;
using UnityEngine;

public struct TurnOffTag : IComponentData { }

public class TurnOffTagComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new TurnOffTag());
    }
}