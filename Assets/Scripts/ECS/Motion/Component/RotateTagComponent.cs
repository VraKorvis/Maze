using Unity.Entities;
using UnityEngine;

public struct RotateTag : IComponentData {
}

public class RotateTagComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new RotateTag());
    }
}