using Unity.Entities;
using UnityEngine;

public struct SpellImmuneTag : IComponentData {
}

public class SpellImmuneTagComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new SpellImmuneTag());
    }
}