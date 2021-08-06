using Unity.Entities;
using UnityEngine;

public struct RagTag : IComponentData { }

public class RagComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new RagTag());
    }
}